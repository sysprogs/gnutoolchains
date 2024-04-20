#pragma once
#include <list>
#include <string>
#include <map>
#include <bzscore/file.h>
#include <bzscore/assert.h>
#include <bzshlp/algorithms/filealgs.h>
#include "../ToolchainArchive/ArchiveFormat.h"

using namespace BazisLib;

struct VersionSummary
{
	String Target;
	String GCCVersion;
	String GDBVersion;
	String BinutilsVersion;
	String NewlibVersion;
	String ToolchainRegistryTag;
};

static DynamicStringA StringToUTF8String(const TempString& str)
{
#ifdef UNICODE
	DynamicStringA ret;
	char* pszStr = ret.PreAllocate(str.length() * 2, false);
	if (!pszStr)
		return ret;
	size_t done = WideCharToMultiByte(CP_UTF8, 0, str.GetConstBuffer(), (unsigned)str.length(), pszStr, ret.GetAllocated(), NULL, NULL);
	if (!done && !str.empty())
	{
		fprintf(stderr, "Failed to convert string to UTF8: %S\n", DynamicString(str).c_str());
		fflush(stderr);
		exit(1);
	}

	ret.SetLength(done);
	return ret;
#else
	return str;
#endif
}

class ToolchainScanner
{
private:
	struct Entry
	{
		String RelativePath;
		bool IsDirectory;
		ULONGLONG FileSize;
		AdvancedFileInfo Info;

		Entry()
		{
			IsDirectory = false;
			FileSize = 0;
		}

		Entry(const String& relPath, bool isDir)
		{
			RelativePath = relPath;
			IsDirectory = isDir;
			FileSize = 0;
		}
	};

	struct DuplicateEntry
	{
		String OriginalRelativePath;
		String TargetRelativePath;
	};

	std::list<Entry> _Entries;
	std::list<DuplicateEntry> _Duplicates;
	std::list<DuplicateEntry> _Symlinks;

	String _Directory;
	ULONGLONG _StubSize;
	char _TmpBuf[65536];

	CBuffer _BigBuf1, _BigBuf2;

private:
	void ScanDirectoryRecursive(const String& dir, const String& toolchainDir)
	{
		wchar_t pathBuf[512];
		int fullToolchainDirLen = 0;
		if (GetFullPathNameW(toolchainDir.c_str(), _countof(pathBuf), pathBuf, 0))
			fullToolchainDirLen = wcslen(pathBuf);

		for (BazisLib::Directory::enumerator it = BazisLib::Directory(dir).FindFirstFileW(); it.Valid(); it.Next())
		{ 
			if (it.IsAPseudoEntry())
				continue;
			String fn = it.GetFullPath();
			ASSERT(fn.substr(0, toolchainDir.length()) == toolchainDir);
			Entry entry(fn.substr(toolchainDir.length() + 1), it.IsDirectory());

			if (!it.IsDirectory())
				entry.FileSize = it.GetSize();

			ActionStatus status;
			entry.Info.Attributes = it.GetAttributes(&status);

			if ((entry.Info.Attributes & FILE_ATTRIBUTE_REPARSE_POINT) && !it.IsDirectory())
			{
				BazisLib::File file(fn, BazisLib::FileModes::OpenReadOnly.AddRawFlags(FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT));
				int done = file.DeviceIoControl(FSCTL_GET_REPARSE_POINT, NULL, 0, _TmpBuf, sizeof(_TmpBuf));
				if (done >= 0)
				{
					if (*((unsigned*)_TmpBuf) == IO_REPARSE_TAG_SYMLINK)
					{
						int pathBufferOffset = 8 + 12;
						int nameOffset = *reinterpret_cast<unsigned short*>(_TmpBuf + 8);
						int nameLen = *reinterpret_cast<unsigned short*>(_TmpBuf + 10);
						int flags = *reinterpret_cast<unsigned*>(_TmpBuf + 16);
						BazisLib::DynamicStringW str(reinterpret_cast<wchar_t*>(_TmpBuf + pathBufferOffset + nameOffset), nameLen / 2);
						DuplicateEntry dupEntry;
						 
						String resolvedFn = fn;
						if (GetFinalPathNameByHandleW(file.GetHandleForSingleUse(), pathBuf, _countof(pathBuf), 0))
							resolvedFn = pathBuf + 4; //Skip '\\?\'

						if (_tcsnicmp(fn.c_str(), toolchainDir.c_str(), toolchainDir.size()))
							throw String(_T("File path is outside toolchain dir path: ") + fn);
						if (_tcsnicmp(resolvedFn.c_str(), toolchainDir.c_str(), toolchainDir.size()))
							throw String(_T("Resolved path is outside toolchain dir path: ") + resolvedFn);

						dupEntry.TargetRelativePath = fn.substr(toolchainDir.length() + 1);
						auto targetPath = Path::Combine(Path::GetDirectoryName(resolvedFn), str);

						if (GetFullPathNameW(targetPath.c_str(), _countof(pathBuf), pathBuf, 0))
						{
							dupEntry.OriginalRelativePath = pathBuf + fullToolchainDirLen + 1;
						}
						else
						{
							dupEntry.OriginalRelativePath = targetPath.substr(toolchainDir.length() + 1);
						}

						_Symlinks.push_back(dupEntry);
						continue;
					}
				}
			}

			entry.Info.Attributes &= ~FILE_ATTRIBUTE_REPARSE_POINT;

			BazisLib::DateTime creationTime, lastWriteTime, lastAccessTime;
			it.GetFileTimes(&creationTime, &lastWriteTime, &lastAccessTime);
			entry.Info.CreationTime = *creationTime.GetFileTime();
			entry.Info.LastWriteTime = *lastWriteTime.GetFileTime();
			entry.Info.LastAccessTime = *lastAccessTime.GetFileTime();

			_Entries.push_back(entry);

			if (it.IsDirectory())
				ScanDirectoryRecursive(fn, toolchainDir);
		}
	}

	bool AreFilesEqual(const String& fn1, const String& fn2)
	{
		File f1(fn1, FileModes::OpenReadOnly), f2(fn2, FileModes::OpenReadOnly);
		if (!f1.Valid())
			throw String(_T("Can't open ") + fn1);
		if (!f2.Valid())
			throw String(_T("Can't open ") + fn2);

		if (f1.GetSize() != f2.GetSize())
			return false;

		_BigBuf1.EnsureSize(1024 * 1024);
		_BigBuf2.EnsureSize(1024 * 1024);

		ULONGLONG size = f1.GetSize(), done = 0;
		while (done < size)
		{
			ULONGLONG remaining = size - done;
			if (remaining > _BigBuf1.GetAllocated())
				remaining = _BigBuf2.GetAllocated();

			size_t todo = (size_t)remaining;
			if (f1.Read(_BigBuf1.GetData(), todo) != todo)
				throw new String(_T("Cannot read ") + fn1);
			if (f2.Read(_BigBuf2.GetData(), todo) != todo)
				throw new String(_T("Cannot read ") + fn2);

			if (memcmp(_BigBuf1.GetData(), _BigBuf2.GetData(), todo))
				return false;

			done += todo;
		}

		return true;
	}

public:
	ToolchainScanner(const String& directory)
		: _Directory(directory)
		, _StubSize(0)
	{
		ScanDirectoryRecursive(directory, directory);
	}

	size_t GetEntryCount() { return _Entries.size(); }
	size_t GetSymlinkCount() { return _Symlinks.size(); }

	void SearchForDuplicates(LPCTSTR lpExtension)
	{
		int done = 0;
		for (std::list<Entry>::iterator it1 = _Entries.begin(); it1 != _Entries.end(); it1++)
		{
			done++;
			if (it1->IsDirectory)
				continue;
			if (lpExtension && (Path::GetExtensionExcludingDot(it1->RelativePath) != lpExtension))
				continue;

			for (std::list<Entry>::iterator it2 = it1; it2 != _Entries.end(); it2++)
			{
				if (it2 == it1)
					continue;

				if (it2->FileSize != it1->FileSize)
					continue;

				if (it1->IsDirectory || it2->IsDirectory)
					continue;

				if (AreFilesEqual(Path::Combine(_Directory, it1->RelativePath), Path::Combine(_Directory, it2->RelativePath)))
				{
					DuplicateEntry entry;
					entry.OriginalRelativePath = it1->RelativePath;
					entry.TargetRelativePath = it2->RelativePath;
					_Duplicates.push_back(entry);
					_Entries.erase(it2--);
				}

				if (it2 == _Entries.end())
					break;
			}

		}
	}

private:
	template <size_t _Size> void WriteFixedField(char(&buffer)[_Size], const String& str)
	{
		auto ansiStr = StringToUTF8String(str);
		size_t len = min(ansiStr.length(), _Size - 1);
		memset(buffer, 0, sizeof(buffer));
		memcpy(buffer, ansiStr.c_str(), len);
	}

public:
	void ProduceArchiveHeaders(File& file, const VersionSummary& versions, File* pLogFile = nullptr)
	{
		_StubSize = file.Seek(0, FileFlags::FileCurrent);

		CBuffer stringBuf;
		stringBuf.EnsureSize(1);
		stringBuf.SetSize(1);
		*((char*)stringBuf.GetData()) = 0;
		std::map<String, unsigned> knownStrings;

		ToolchainArchiveHeader hdr;
		memset(&hdr, 0, sizeof(hdr));
		memcpy(hdr.Signature, s_ToolchainArchiveSignature, sizeof(hdr.Signature));
		hdr.FormatVersion = ToolchainArchiveHeader::kFormatVersion;
		hdr.FileRecordCount = (unsigned)_Entries.size();
		hdr.HardlinkRecordCount = (unsigned)(_Duplicates.size() + _Symlinks.size());

		WriteFixedField(hdr.Target, versions.Target);
		WriteFixedField(hdr.GCCVersion, versions.GCCVersion);
		WriteFixedField(hdr.GDBVersion, versions.GDBVersion);
		WriteFixedField(hdr.BinutilsVersion, versions.BinutilsVersion);
		WriteFixedField(hdr.NewlibVersion, versions.NewlibVersion);
		WriteFixedField(hdr.ToolchainRegistryTag, versions.ToolchainRegistryTag);

		file.Write(&hdr, sizeof(hdr));

		if (pLogFile)
			pLogFile->WriteLine(DynamicString(L"=== Files ==="));


		for each (const Entry & entry in _Entries)
		{
			FileEntry serializedEntry = { 0, };
			if (entry.IsDirectory)
				serializedEntry.Flags |= kFileFlagDirectory;
			serializedEntry.SizeInBytes = entry.FileSize;
			serializedEntry.NameOffset = stringBuf.GetSize();
			serializedEntry.Info = entry.Info;

			if (pLogFile)
				pLogFile->WriteLine(DynamicString::sFormat(L"[%c] %s: %d bytes", entry.IsDirectory ? 'D' : 'F', entry.RelativePath.c_str(), entry.FileSize));

			auto str = StringToUTF8String(entry.RelativePath);
			stringBuf.append(str.c_str(), str.length() + 1);
			file.Write(&serializedEntry, sizeof(serializedEntry));

			knownStrings[entry.RelativePath] = serializedEntry.NameOffset;
		}

		std::list<DuplicateEntry>* pLists[2] = { &_Duplicates, &_Symlinks };

		if (pLogFile)
			pLogFile->WriteLine(DynamicString(L"=== Duplicates ==="));

		for (auto* pList : pLists)
		{
			for (const DuplicateEntry& entry : *pList)
			{
				HardLinkEntry serializedEntry = { 0, };

				if (pLogFile)
					pLogFile->WriteLine(DynamicString::sFormat(L"%s => %s", entry.TargetRelativePath.c_str(), entry.OriginalRelativePath.c_str()));

				serializedEntry.OriginalNameOffset = knownStrings[entry.OriginalRelativePath];
				if (!serializedEntry.OriginalNameOffset)
				{
					printf("Unknown path \"%S\" referenced from %S\n", entry.OriginalRelativePath.c_str(), entry.TargetRelativePath.c_str());
					fflush(stdout);
					__asm int 3;
				}
				serializedEntry.NameOffset = stringBuf.GetSize();

				auto str = StringToUTF8String(entry.TargetRelativePath);
				knownStrings[entry.TargetRelativePath] = serializedEntry.NameOffset;

				stringBuf.append(str.c_str(), str.length() + 1);
				file.Write(&serializedEntry, sizeof(serializedEntry));
			}
		}

		hdr.StringBlockSize = stringBuf.GetSize();
		hdr.StringBlockOffset = (unsigned)file.GetSize() - _StubSize;

		file.Write(stringBuf.GetData(), stringBuf.GetSize());

		hdr.FileContentsOffset = (unsigned)file.GetSize() - _StubSize;
		hdr.CompressionAlgorithm = ToolchainArchiveHeader::kLZMA;

		file.Seek(_StubSize, FileFlags::FileBegin);
		file.Write(&hdr, sizeof(hdr));
		file.Seek(_StubSize + hdr.FileContentsOffset, FileFlags::FileBegin);
	}

	void ProduceUncompressedDataFile(File& file)
	{
		for each (const Entry & entry in _Entries)
		{
			if (entry.IsDirectory)
				continue;
			File sourceFile(Path::Combine(_Directory, entry.RelativePath), FileModes::OpenReadOnly);
			if (sourceFile.GetSize() != entry.FileSize)
			{
				printf("Mismatching size for %S\n", entry.RelativePath.c_str());
				fflush(stdout);
				__asm int 3;
			}

			ULONGLONG done = Algorithms::BulkCopy(&sourceFile, &file, entry.FileSize, 1024 * 1024);
			if (done != entry.FileSize)
				__asm int 3;
		}
	}

	void SaveCompressedBlockSize(File& file, ULONGLONG blockLen)
	{
		ToolchainArchiveHeader hdr;
		file.Seek(_StubSize, FileFlags::FileBegin);
		file.Read(&hdr, sizeof(ToolchainArchiveHeader));
		hdr.CompressedBlockLength = blockLen;
		file.Seek(_StubSize, FileFlags::FileBegin);
		file.Write(&hdr, sizeof(ToolchainArchiveHeader));
	}


};