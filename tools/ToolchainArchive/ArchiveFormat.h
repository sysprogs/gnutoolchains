#pragma once

static const char s_ToolchainArchiveSignature[] = "XXGNUTOOLCHAINv100";

struct ToolchainArchiveHeader
{
	enum {kFormatVersion = 2};
	enum {kLZMA = 'AMZL'};

	char Signature[sizeof(s_ToolchainArchiveSignature)];
	unsigned FormatVersion;
	unsigned FileRecordCount;
	unsigned HardlinkRecordCount;
	unsigned StringBlockOffset;
	unsigned StringBlockSize;

	unsigned CompressionAlgorithm;
	unsigned FileContentsOffset;
	ULONGLONG CompressedBlockLength;

	char Target[16];
	char GCCVersion[16];
	char GDBVersion[16];
	char BinutilsVersion[16];
	char NewlibVersion[16];
	char ToolchainRegistryTag[128];
};

enum 
{
	kFileFlagDirectory = 1,
};

struct AdvancedFileInfo
{
	FILETIME LastWriteTime = FILETIME();
	FILETIME CreationTime = FILETIME();
	FILETIME LastAccessTime = FILETIME();
	unsigned Attributes = 0;
};

struct FileEntry
{
	unsigned NameOffset;
	unsigned Flags;
	ULONGLONG SizeInBytes;
	AdvancedFileInfo Info;
};

struct HardLinkEntry
{
	unsigned NameOffset;
	unsigned OriginalNameOffset;
};