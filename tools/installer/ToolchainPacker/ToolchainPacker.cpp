// ToolchainPacker.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "../7zip/CPP/7zip/Compress/LzmaEncoder.h"
#include "ToolchainScanner.h"

static unsigned GetNumberOfProcessors()
{
	SYSTEM_INFO systemInfo;
	GetSystemInfo(&systemInfo);
	return (unsigned )systemInfo.dwNumberOfProcessors;
}

struct BuildJob
{
	String Directory;
	String ArchiveFile;
};

bool CompressDataBlock(File &sourceBlock, File &toolchainFile);

int _tmain(int argc, _TCHAR* argv[])
{
	if (argc < 3)
	{
		printf("Usage: ToolchainPacker <dir> <archive> [gcc/bu/gdb/newlib/tag/target/stub=xxx]\n");
		return 1;
	}

	BuildJob job;
	job.Directory = argv[1];
	job.ArchiveFile = argv[2];

	printf("Scanning %S...\n", job.Directory);
	ToolchainScanner scanner(job.Directory);
	printf("Found %d files. Searching for duplicates...\n", scanner.GetEntryCount());
	scanner.SearchForDuplicates(_T("exe"));
	auto dups = scanner.GetDuplicates();
	printf("Found %d duplicate files:\n", dups.size());
	for each(auto entry in dups)
		printf("  %S == %S\n", entry.OriginalRelativePath.c_str(), entry.TargetRelativePath.c_str());

	File archiveFile(job.ArchiveFile, FileModes::CreateOrTruncateRW);
	VersionSummary versions;

	String stub;

	for (size_t i = 3; i < argc; i++)
	{
		TCHAR *pEq = _tcschr(argv[i], '=');
		if (!pEq)
			continue;

		String key(argv[i], pEq - argv[i]), val(pEq + 1);
		if (!key.icompare(L"gcc"))
			versions.GCCVersion = val;
		else if (!key.icompare(L"bu"))
			versions.BinutilsVersion = val;
		else if (!key.icompare(L"gdb"))
			versions.GDBVersion = val;
		else if (!key.icompare(L"newlib"))
			versions.NewlibVersion = val;
		else if (!key.icompare(L"target"))
			versions.Target = val;
		else if (!key.icompare(L"tag"))
			versions.ToolchainRegistryTag = val;
		else if (!key.icompare(L"stub"))
			stub = val;
	}

	CBuffer buffer;
	if (stub != L"")
	{
		File stubFile(stub, FileModes::OpenReadOnly);
		if (stubFile.Valid())
		{
			size_t stubLen = (size_t)stubFile.GetSize();
			if (stubLen != -1)
			{
				buffer.EnsureSize(stubLen);
				if (stubFile.Read(buffer.GetData(), stubLen) == stubLen)
					buffer.SetSize(stubLen);
			}
		}
	}

	if (buffer.GetSize() > 0)
	{
		archiveFile.Write(buffer.GetData(), buffer.GetSize());
	}

	scanner.ProduceArchiveHeaders(archiveFile, versions);

	ULONGLONG compressedStart = archiveFile.GetPosition();

	printf("Producing data block...\n");
	File dataFile(job.ArchiveFile + _T(".uncompressed"), FileModes::CreateOrTruncateRW);
	scanner.ProduceUncompressedDataFile(dataFile);
	dataFile.Seek(0, FileFlags::FileBegin);

	if (!CompressDataBlock(dataFile, archiveFile))
	{
		printf("Failed to compress data block\n");
		return 1;
	}

	ULONGLONG compressedLength = archiveFile.GetPosition() - compressedStart;

	scanner.SaveCompressedBlockSize(archiveFile, compressedLength);

	printf("Done\n");
	return 0;
}

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
	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID, void **ppvObject) {return E_NOINTERFACE;}
	virtual ULONG STDMETHODCALLTYPE AddRef( void) {return InterlockedIncrement(&m_ReferenceCount);}
	//Reference counter is not actually used
	virtual ULONG STDMETHODCALLTYPE Release( void) {return InterlockedDecrement(&m_ReferenceCount);}
};

class CompressProgressReporter : public ICompressProgressInfo
{
private:
	LONG m_ReferenceCount;
	ULONGLONG _TotalSize;

public:
	CompressProgressReporter(ULONGLONG totalSize)
	  {
		  m_ReferenceCount = 0;
		  _TotalSize = totalSize;
	  }

	  STDMETHOD(SetRatioInfo)(const UInt64 *inSize, const UInt64 *outSize)
	  {
		  double percent = ((double)*inSize) / _TotalSize;
		  printf("\rCompressing... [%.2f%% done]     ", percent * 100);
		  return S_OK;
	  }

	  //IUnknown methods
	  virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID, void **ppvObject) {return E_NOINTERFACE;}
	  virtual ULONG STDMETHODCALLTYPE AddRef( void) {return InterlockedIncrement(&m_ReferenceCount);}
	  //Reference counter is not actually used
	  virtual ULONG STDMETHODCALLTYPE Release( void) {return InterlockedDecrement(&m_ReferenceCount);}
};

bool CompressDataBlock(File &sourceBlock, File &toolchainFile)
{
	NCompress::NLzma::CEncoder *pEncoder = new NCompress::NLzma::CEncoder;

	pEncoder->AddRef();

	BSTR mf = L"BT4";
	bool eos = false;
	int dictionary = 1 << 26;
	int numThreads = GetNumberOfProcessors();

	UInt32 posStateBits = 2;
	UInt32 litContextBits = 3; // for normal files
	// UInt32 litContextBits = 0; // for 32-bit data
	UInt32 litPosBits = 0;
	// UInt32 litPosBits = 2; // for 32-bit data
	UInt32 algorithm = 1;
	UInt32 numFastBytes = 128;
	UInt32 matchFinderCycles = 16 + numFastBytes / 2;

	PROPID propIDs[] = 
	{
		NCoderPropID::kDictionarySize,
		NCoderPropID::kPosStateBits,
		NCoderPropID::kLitContextBits,
		NCoderPropID::kLitPosBits,
		NCoderPropID::kAlgorithm,
		NCoderPropID::kNumFastBytes,
		NCoderPropID::kMatchFinder,
		NCoderPropID::kEndMarker,
		NCoderPropID::kNumThreads,
		NCoderPropID::kMatchFinderCycles,
	};
	const int kNumPropsMax = sizeof(propIDs) / sizeof(propIDs[0]);

	PROPVARIANT properties[kNumPropsMax];
	for (int p = 0; p < 6; p++)
		properties[p].vt = VT_UI4;

	properties[0].ulVal = (UInt32)dictionary;
	properties[1].ulVal = (UInt32)posStateBits;
	properties[2].ulVal = (UInt32)litContextBits;
	properties[3].ulVal = (UInt32)litPosBits;
	properties[4].ulVal = (UInt32)algorithm;
	properties[5].ulVal = (UInt32)numFastBytes;

	properties[6].vt = VT_BSTR;
	properties[6].bstrVal = (BSTR)(const wchar_t *)mf;

	properties[7].vt = VT_BOOL;
	properties[7].boolVal = eos ? VARIANT_TRUE : VARIANT_FALSE;

	properties[8].vt = VT_UI4;
	properties[8].ulVal = (UInt32)numThreads;

	// it must be last in property list
	properties[9].vt = VT_UI4;
	properties[9].ulVal = (UInt32)matchFinderCycles;

	int numProps = kNumPropsMax - 1;

	if (pEncoder->SetCoderProperties(propIDs, properties, numProps) != S_OK)
	{
		pEncoder->Release();
		return false;
	}

	FileWrapper outWrapper(&toolchainFile), inWrapper(&sourceBlock);

	pEncoder->WriteCoderProperties(&outWrapper);

	UInt64 inSize = sourceBlock.GetSize(), outSize = 0;
	CompressProgressReporter reporter(inSize);
	printf("\n");
	HRESULT result = pEncoder->Code(&inWrapper, &outWrapper, NULL, NULL, &reporter);
	pEncoder->Release();
	if (result != S_OK)
		return false;

	return true;
}
