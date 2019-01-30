#pragma once

#include <bzscore/buffer.h>
#include <bzscore/string.h>
#include <bzscore/autofile.h>
#include "../ToolchainArchive/ArchiveFormat.h"
#include "../../../imported/lzma/CPP/7zip/Compress/LzmaDecoder.h"
#include <bzscore/thread.h>
#include <bzscore/Win32/registry.h>

using namespace BazisLib;

static String UTF8StringToString(const TempStringA &str)
{
	String ret;
	wchar_t *pwszStr = ret.PreAllocate(str.length(), false);
	if (!pwszStr)
		return ret;
	size_t done = MultiByteToWideChar(CP_UTF8, 0, str.GetConstBuffer(), (unsigned)str.length(), pwszStr, (unsigned)str.length());
	ret.SetLength(done);
	return ret;
}

static bool ClearReadOnlyAttributes(const String &fn)
{
	DWORD attr = GetFileAttributes(fn.c_str());
	if (attr != INVALID_FILE_ATTRIBUTES && (attr & (FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_SYSTEM)))
	{
		SetFileAttributes(fn.c_str(), attr & ~(FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_SYSTEM));
		return true;
	}

	return false;
}

static ActionStatus HardlinkOrCopyFile(BazisLib::String fnOrig, BazisLib::String fnNew, bool canHardlink, BazisLib::File &logFile, BazisLib::String &errorInfo)
{
	DWORD existingAttr = GetFileAttributes(fnNew.c_str());
	if (existingAttr != INVALID_FILE_ATTRIBUTES)
	{
		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Deleting old %s...", fnNew.c_str()));
		ClearReadOnlyAttributes(fnNew);
		File::Delete(fnNew);
	}

	if (canHardlink && CreateHardLink(fnNew.c_str(), fnOrig.c_str(), NULL))
	{
		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Hardlinked %s => %s", fnOrig.c_str(), fnNew.c_str()));
		return MAKE_STATUS(Success);
	}

	if (logFile.Valid())
		logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Copying %s => %s", fnOrig.c_str(), fnNew.c_str()));


	if (!CopyFile(fnOrig.c_str(), fnNew.c_str(), FALSE))
	{
		errorInfo = BazisLib::String::sFormat(L"Failed to copy %s to %s", fnOrig.c_str(), fnNew.c_str());

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"CopyFile(%s => %s): error %d", fnOrig.c_str(), fnNew.c_str(), GetLastError()));
		return MAKE_STATUS(ActionStatus::FailFromLastError());
	}

	return MAKE_STATUS(Success);
}

static ActionStatus CopyDirectoryRecursively(BazisLib::String originalPath, BazisLib::String newPath, BazisLib::File &logFile, BazisLib::String &errorInfo, bool canHardlink)
{
	Directory dir(originalPath);
	Directory::Create(newPath);

	for (Directory::enumerator iter = dir.FindFirstFile(); iter.Valid(); iter.Next())
	{
		if (iter.IsAPseudoEntry())
			continue;

		BazisLib::String fnOrig = originalPath + L"\\" + iter.GetRelativePath();
		BazisLib::String fnNew = newPath + L"\\" + iter.GetRelativePath();

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Copying %s to %s", fnOrig.c_str(), fnNew.c_str()));

		if (iter.IsDirectory())
		{
			ActionStatus st = CopyDirectoryRecursively(fnOrig, fnNew, logFile, errorInfo, canHardlink);
			if (!st.Successful())
				return st;
		}
		else
		{
			ActionStatus st = HardlinkOrCopyFile(fnOrig, fnNew, canHardlink, logFile, errorInfo);
		}
	}

	return MAKE_STATUS(Success);
}

struct IInstallerCallbacks
{
public:
	virtual void OnProgress(ULONGLONG total, ULONGLONG done, double ratio) = 0;
	virtual void OnCompleted(ActionStatus status, BazisLib::String extraInfo) = 0;
	virtual void UpdateProgressText(const String &text) = 0;
};

struct InstallationParameters
{
	String TargetDirectory;
	bool CreateHardlinks;
	bool AddToPath;
	bool AllUsers;
	bool CreateInstallationLog;
};

class ToolchainInstaller
{
private:
	bool _Valid;
	CBuffer _StringBuffer;

	ToolchainArchiveHeader _Header;
	SolidVector<FileEntry> _Files;
	SolidVector<HardLinkEntry> _HardLinks;
	File _File;
	ULONGLONG _BaseOffset;

	ULONGLONG _TotalFileSize;

	BazisLib::MemberThread *m_pInstallerThread;

public:
	ToolchainInstaller(const String &packageFile, ULONGLONG packageFileOffset)
		: _File(packageFile, FileModes::OpenReadOnly)
		, _BaseOffset(packageFileOffset)
		, _Valid(false)
		, _TotalFileSize(0)
		, m_pInstallerThread(NULL)
	{
		if (!_File.Valid())
			return;
		_File.Seek(_BaseOffset, FileFlags::FileBegin);
		if (_File.Read(&_Header, sizeof(_Header)) != sizeof(_Header))
			return;

		if (memcmp(_Header.Signature, s_ToolchainArchiveSignature, sizeof(_Header.Signature)))
			return;
		if (_Header.FormatVersion != ToolchainArchiveHeader::kFormatVersion)
			return;
		if (_Header.CompressionAlgorithm != ToolchainArchiveHeader::kLZMA)
			return;

		if (!_Files.EnsureSize(_Header.FileRecordCount))
			return;
		size_t todo = _Header.FileRecordCount * sizeof(FileEntry);
		if (_File.Read(_Files.GetData(), todo) != todo)
			return;
		_Files.SetUsed(_Header.FileRecordCount);

		if (!_HardLinks.EnsureSize(_Header.HardlinkRecordCount))
			return;
		todo = _Header.HardlinkRecordCount * sizeof(HardLinkEntry);
		if (_File.Read(_HardLinks.GetData(), todo) != todo)
			return;
		_HardLinks.SetUsed(_Header.HardlinkRecordCount);

		_File.Seek(_BaseOffset + _Header.StringBlockOffset, FileFlags::FileBegin);
		if (!_StringBuffer.EnsureSize(_Header.StringBlockSize))
			return;
		if (_File.Read(_StringBuffer.GetData(), _Header.StringBlockSize) != _Header.StringBlockSize)
			return;
		_StringBuffer.SetSize(_Header.StringBlockSize);

		for (size_t i = 0; i < _Files.GetUsed(); i++)
		{
			if (_Files[i].NameOffset >= _StringBuffer.GetSize())
				return;
			_TotalFileSize += _Files[i].SizeInBytes;
		}

		_Valid = true;
	}

	~ToolchainInstaller()
	{
		delete m_pInstallerThread;
	}

	bool Valid() { return _Valid; }

private:
	IInstallerCallbacks *_pCallbacks;
	InstallationParameters _Parameters;

private:
	class CallbackWrapper : public ICompressProgressInfo, public ISequentialOutStream
	{
		size_t _CurrentlyWrittenFile;
		ULONGLONG _CurrentFileOffset;
		ToolchainInstaller *_pInstaller;
		long m_ReferenceCount;
		File *_pCurrentFile;

		BazisLib::File &_LogFile;
		BazisLib::String &_LastMeaningfulError;

	public:
		CallbackWrapper(ToolchainInstaller *pInstaller, BazisLib::File &logFile, BazisLib::String &extraInfoBuffer)
			: _LogFile(logFile)
			, _LastMeaningfulError(extraInfoBuffer)
		{
			_pInstaller = pInstaller;
			_CurrentlyWrittenFile = 0;
			_CurrentFileOffset = 0;
			m_ReferenceCount = 0;
			_pCurrentFile = NULL;
		}

		STDMETHOD(SetRatioInfo)(const UInt64 *inSize, const UInt64 *outSize)
		{
			if (_pInstaller->_pCallbacks)
				_pInstaller->_pCallbacks->OnProgress(_pInstaller->_TotalFileSize, *outSize, ((double)*outSize) / _pInstaller->_TotalFileSize);
			return S_OK;
		}


		STDMETHOD(Write)(const void *data, UInt32 size, UInt32 *processedSize)
		{
			UInt32 &done = *processedSize;
			done = 0;
			char *pData = (char *)data;

			while (size)
			{
				if (_CurrentlyWrittenFile >= _pInstaller->_Files.GetUsed())
				{
					ASSERT(FALSE);
					if (_LogFile.Valid())
						_LogFile.WriteLine(BazisLib::DynamicString::sFormat(L"Invalid file index. Corrupt archive?"));
					return E_FAIL;
				}

				const FileEntry &rec = _pInstaller->_Files[_CurrentlyWrittenFile];
				if (rec.Flags & kFileFlagDirectory)
				{
					String fn = Path::Combine(_pInstaller->_Parameters.TargetDirectory, UTF8StringToString(DynamicStringA((const char *)_pInstaller->_StringBuffer.GetData(rec.NameOffset))));

					ASSERT(!_CurrentFileOffset);
					if (!Directory::Exists(fn))
					{
						ActionStatus st = Directory::Create(fn);
						if (_LogFile.Valid())
							_LogFile.WriteLine(BazisLib::DynamicString::sFormat(L"[%d] CreateDirectory(%s) => %d", _CurrentlyWrittenFile, fn.c_str(), st.GetErrorCode()));

						if (!st.Successful())
							return st.ConvertToHResult();
					}
					else
					{
						if (_LogFile.Valid())
							_LogFile.WriteLine(BazisLib::DynamicString::sFormat(L"[%d] Directory %s already exists", _CurrentlyWrittenFile, fn.c_str()));

					}
					if (rec.Info.Attributes)
						SetFileAttributes(fn.c_str(), rec.Info.Attributes);
					_CurrentlyWrittenFile++;
				}
				else
				{
					ASSERT(_CurrentFileOffset <= rec.SizeInBytes);

					if (!_pCurrentFile)
					{
						String fn = Path::Combine(_pInstaller->_Parameters.TargetDirectory, UTF8StringToString(DynamicStringA((const char *)_pInstaller->_StringBuffer.GetData(rec.NameOffset))));

						int idx = fn.rfind('\\');
						if (idx == -1)
							idx = 0;
						else
							idx++;

						if (_pInstaller->_pCallbacks)
							_pInstaller->_pCallbacks->UpdateProgressText(L"Installing " + fn.substr(idx) + L"...");

						ASSERT(!_CurrentFileOffset);
						ActionStatus st;
						_pCurrentFile = new File(fn, FileModes::CreateOrTruncateRW, &st);
						if (!st.Successful())
						{
							if (ClearReadOnlyAttributes(fn))
							{
								delete _pCurrentFile;
								_pCurrentFile = new File(fn, FileModes::CreateOrTruncateRW, &st);
							}
						}

						if (_LogFile.Valid())
							_LogFile.WriteLine(BazisLib::DynamicString::sFormat(L"[%d] CreateFile(%s) => %d", _CurrentlyWrittenFile, fn.c_str(), st.GetErrorCode()));

						if (!st.Successful())
						{
							_LastMeaningfulError = BazisLib::String::sFormat(L"Failed to create %s: %s", fn.c_str(), st.GetMostInformativeText().c_str());
							return st.ConvertToHResult();
						}
					}

					ULONGLONG remaining = rec.SizeInBytes - _CurrentFileOffset;
					size_t todo = size;
					if (todo > remaining)
						todo = (size_t)remaining;

					if (_pCurrentFile->Write(pData, todo) != todo)
						return HRESULT_FROM_WIN32(ERROR_IO_DEVICE);

					_CurrentFileOffset += todo;
					size -= todo;
					pData += todo;
					done += todo;

					ASSERT(_CurrentFileOffset <= rec.SizeInBytes);
					if (_CurrentFileOffset >= rec.SizeInBytes)
					{
						if (_LogFile.Valid())
							_LogFile.WriteLine(BazisLib::DynamicString::sFormat(L"[%d] closing file...", _CurrentlyWrittenFile));

						if (_pCurrentFile)
						{
							BazisLib::DateTime creationTime(rec.Info.CreationTime), lastWriteTime(rec.Info.LastWriteTime), lastAccessTime(rec.Info.LastAccessTime);
							_pCurrentFile->SetFileTimes(&creationTime, &lastWriteTime, &lastAccessTime);
						}

						delete _pCurrentFile;
						_pCurrentFile = NULL;

						String fn = Path::Combine(_pInstaller->_Parameters.TargetDirectory, UTF8StringToString(DynamicStringA((const char *)_pInstaller->_StringBuffer.GetData(rec.NameOffset))));

						if (rec.Info.Attributes)
							SetFileAttributes(fn.c_str(), rec.Info.Attributes);

						_CurrentFileOffset = 0;
						_CurrentlyWrittenFile++;
					}
				}
			}
			return S_OK;
		}

		virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID, void **ppvObject) { return E_NOINTERFACE; }
		virtual ULONG STDMETHODCALLTYPE AddRef(void) { return InterlockedIncrement(&m_ReferenceCount); }
		//Reference counter is not actually used
		virtual ULONG STDMETHODCALLTYPE Release(void) { return InterlockedDecrement(&m_ReferenceCount); }
	};

	class FileWrapper : public ISequentialOutStream, public ISequentialInStream
	{
	private:
		LONG m_ReferenceCount;
		File *_pFile;

	public:

		FileWrapper(File *pFile)
		{
			_pFile = pFile;
			m_ReferenceCount = 1;
		}

		STDMETHOD(Write)(const void *data, UInt32 size, UInt32 *processedSize)
		{
			*processedSize = _pFile->Write(data, size);
			if (*processedSize != size)
				__asm int 3;
			return S_OK;
		}

		STDMETHOD(Read)(void *data, UInt32 size, UInt32 *processedSize)
		{
			*processedSize = _pFile->Read(data, size);
			return S_OK;
		}


		//IUnknown methods
		virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID, void **ppvObject) { return E_NOINTERFACE; }
		virtual ULONG STDMETHODCALLTYPE AddRef(void) { return InterlockedIncrement(&m_ReferenceCount); }
		//Reference counter is not actually used
		virtual ULONG STDMETHODCALLTYPE Release(void) { return InterlockedDecrement(&m_ReferenceCount); }
	};

	ActionStatus CreateDirectoryRecursive(String dir)
	{
		if (Directory::Exists(dir))
			return MAKE_STATUS(Success);

		if (dir.size() == 2 && dir[1] == ':')
			return MAKE_STATUS(FileNotFound);

		String parent = Path::GetDirectoryName(dir);
		if (parent == dir)
			return MAKE_STATUS(FileNotFound);

		ActionStatus st = CreateDirectoryRecursive(parent);
		if (!st.Successful())
			return st;

		return Directory::Create(dir);
	}

	ActionStatus DoInstall(BazisLib::String &extraInfo)
	{
		extraInfo = L"";
		if (_pCallbacks)
			_pCallbacks->UpdateProgressText(_T("Preparing..."));

		ActionStatus st = CreateDirectoryRecursive(_Parameters.TargetDirectory);
		if (!st.Successful())
			return st;

		File logFile(BazisLib::Path::Combine(_Parameters.TargetDirectory, L"install.log"), _Parameters.CreateInstallationLog ? BazisLib::CreateOrTruncateRW : BazisLib::OpenReadOnly);
		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Reading archive header..."));

		_File.Seek(_BaseOffset + _Header.FileContentsOffset, FileFlags::FileBegin);
		byte properties[LZMA_PROPS_SIZE];
		if (_File.Read(properties, sizeof(properties)) != sizeof(properties))
			return MAKE_STATUS(ReadError);
		CComPtr<NCompress::NLzma::CDecoder> pDecoder = new NCompress::NLzma::CDecoder;
		if (pDecoder->SetDecoderProperties2(properties, LZMA_PROPS_SIZE) != S_OK)
			return MAKE_STATUS(UnknownError);

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Starting unpacking..."));

		if (_pCallbacks)
			_pCallbacks->UpdateProgressText(_T("Unpacking..."));

		CallbackWrapper wrp(this, logFile, extraInfo);
		FileWrapper file(&_File);
		UInt64 size = _Header.CompressedBlockLength, outSize = _TotalFileSize;
		HRESULT hR = pDecoder->Code(&file, &wrp, &size, &outSize, &wrp);
		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Unpacking complete: code 0x%08x...", hR));

		if (!SUCCEEDED(hR))
			return MAKE_STATUS(ActionStatus::FromHResult(hR));

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Creating %d hardlinks...", _HardLinks.GetUsed()));

		for (size_t i = 0; i < _HardLinks.GetUsed(); i++)
		{
			String fnOrig = Path::Combine(_Parameters.TargetDirectory, UTF8StringToString(DynamicStringA((const char *)_StringBuffer.GetData(_HardLinks[i].OriginalNameOffset))));
			String fnNew = Path::Combine(_Parameters.TargetDirectory, UTF8StringToString(DynamicStringA((const char *)_StringBuffer.GetData(_HardLinks[i].NameOffset))));

			bool doCopy = false;
			if (File::Exists(fnNew))
				File::Delete(fnNew);

			int idx = fnNew.rfind('\\');
			if (idx == -1)
				idx = 0;
			else
				idx++;

			DWORD attr = GetFileAttributes(fnOrig.c_str());
			ActionStatus st;
			if ((attr != INVALID_FILE_ATTRIBUTES) && (attr & FILE_ATTRIBUTE_DIRECTORY))
			{
				if (_pCallbacks)
					_pCallbacks->UpdateProgressText(L"Copying " + fnNew.substr(idx) + L"...");
				st = CopyDirectoryRecursively(fnOrig, fnNew, logFile, extraInfo, _Parameters.CreateHardlinks);
			}
			else
			{
				if (_pCallbacks)
					_pCallbacks->UpdateProgressText(L"Hardlinking " + fnNew.substr(idx) + L"...");

				st = HardlinkOrCopyFile(fnOrig, fnNew, _Parameters.CreateHardlinks, logFile, extraInfo);
			}

			if (!st.Successful())
				return st;
		}

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Updating registry..."));

		if (_Header.ToolchainRegistryTag[0])
		{
			String regTag = ANSIStringToString(TempStrPointerWrapperA(_Header.ToolchainRegistryTag));
			String regPath = String::sFormat(_T("SOFTWARE\\Free Software Foundation\\%s"), regTag.c_str());

			RegistryKey key(_Parameters.AllUsers ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, regPath.c_str(), 0, true, &st);

			if (logFile.Valid())
				logFile.WriteLine(BazisLib::DynamicString::sFormat(L"CreateKey(%s) => %d", regPath.c_str(), st.GetErrorCode()));

			if (!st.Successful())
				return st;

			key[L"BINUTILS"] = _Parameters.TargetDirectory;
			key[L"G++"] = _Parameters.TargetDirectory;
			key[L"GCC"] = _Parameters.TargetDirectory;

			RegistryKey key2(_Parameters.AllUsers ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER, _T("SOFTWARE\\Sysprogs\\GNUToolchains"), 0, true, &st);

			if (logFile.Valid())
				logFile.WriteLine(BazisLib::DynamicString::sFormat(L"CreateKey(SOFTWARE\\Sysprogs\\GNUToolchains) => %d", st.GetErrorCode()));

			if (!st.Successful())
				return st;

			key2[regTag.c_str()] = _Parameters.TargetDirectory;
		}

		if (_Parameters.AddToPath)
		{
			DWORD_PTR result;
			if (_Parameters.AllUsers)
			{
				RegistryKey key(HKEY_LOCAL_MACHINE, _T("System\\CurrentControlSet\\Control\\Session Manager\\Environment"), 0, true, &st);
				if (logFile.Valid())
					logFile.WriteLine(BazisLib::DynamicString::sFormat(L"System\\CurrentControlSet\\Control\\Session Manager\\Environment) => %d", st.GetErrorCode()));

				if (!st.Successful())
					return st;
				String path = key[L"Path"];
				String pathSuffix = _Parameters.TargetDirectory + _T("\\bin");

				if (path.ifind(pathSuffix) == -1)
				{
					path += _T(";");
					path += pathSuffix;
					key[L"Path"].AssignExpandable(path.c_str());

					if (_pCallbacks)
						_pCallbacks->UpdateProgressText(L"Updating environment variables...");

					SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 1, (LPARAM)_T("Environment"), SMTO_NORMAL, 1000, &result);
				}
			}
			else
			{
				RegistryKey key(HKEY_CURRENT_USER, _T("Environment"), 0, true, &st);
				if (logFile.Valid())
					logFile.WriteLine(BazisLib::DynamicString::sFormat(L"HKCU\\Environment) => %d", st.GetErrorCode()));

				if (!st.Successful())
					return st;
				String path = key[L"Path"];
				String pathSuffix = _Parameters.TargetDirectory + _T("\\bin");

				if (path.ifind(pathSuffix) == -1)
				{
					path += _T(";");
					path += pathSuffix;
					key[L"Path"].AssignExpandable(path.c_str());

					if (_pCallbacks)
						_pCallbacks->UpdateProgressText(L"Updating environment variables...");

					SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, (LPARAM)_T("Environment"), SMTO_NORMAL, 1000, &result);
				}
			}
		}

		if (_pCallbacks)
			_pCallbacks->UpdateProgressText(L"Installation complete");

		if (logFile.Valid())
			logFile.WriteLine(BazisLib::DynamicString::sFormat(L"Installation complete"));
		return MAKE_STATUS(Success);
	}

	int InstallationThreadBody()
	{
		BazisLib::String info;
		ActionStatus st = DoInstall(info);
		if (_pCallbacks)
			_pCallbacks->OnCompleted(st, info);
		return st.ConvertToHResult();
	}

public:
	void StartInstallationInSeparateThread(const InstallationParameters &parameters, IInstallerCallbacks *pCallbacks, bool waitForCompletion = false)
	{
		_pCallbacks = pCallbacks;
		_Parameters = parameters;

		delete m_pInstallerThread;
		m_pInstallerThread = new BazisLib::MemberThread(this, &ToolchainInstaller::InstallationThreadBody);
		m_pInstallerThread->Start();
		if (waitForCompletion)
			m_pInstallerThread->Join();
	}


	const ToolchainArchiveHeader *GetHeader()
	{
		return &_Header;
	}

	void AbortInstallation()
	{
		if (m_pInstallerThread)
			m_pInstallerThread->Terminate();
	}

};