// GCCInstaller.cpp : main source file for GCCInstaller.exe
//

#include "stdafx.h"

#include "resource.h"

#include "MainDlg.h"

CAppModule _Module;

int InstallUnattended(String &fn, size_t offset, wchar_t *pDirectory, wchar_t *pProgressToken, bool bAlreadyElevated, LPTSTR lpCommandLine, bool allUsers, bool writeLog);

int WINAPI _tWinMain(HINSTANCE hInstance, HINSTANCE /*hPrevInstance*/, LPTSTR lpstrCmdLine, int /*nCmdShow*/)
{
	HRESULT hRes = ::CoInitialize(NULL);
// If you are running on NT 4.0 or higher you can use the following call instead to 
// make the EXE free threaded. This means that calls come in on a random RPC thread.
//	HRESULT hRes = ::CoInitializeEx(NULL, COINIT_MULTITHREADED);
	ATLASSERT(SUCCEEDED(hRes));

	// this resolves ATL window thunking problem when Microsoft Layer for Unicode (MSLU) is used
	::DefWindowProc(NULL, 0, 0, 0L);

	AtlInitCommonControls(ICC_BAR_CLASSES);	// add flags to support other controls

	hRes = _Module.Init(NULL, hInstance);
	ATLASSERT(SUCCEEDED(hRes));

	int nRet = 0;

	int argc = 0;
	wchar_t **argv = CommandLineToArgvW(GetCommandLineW(), &argc);
	wchar_t *pFN = NULL;

	wchar_t *pAutoInstallDir = NULL, *pProgressToken = NULL;
	bool bAlreadyElevated = false, allUsers = false, writeLog = false;

	for (int i = 1; i < argc; i++)
	{
		if (argv[i][0] == '/')
		{
			const TCHAR autoInstall[] = L"/AUTOINSTALL:", progressToken[] = L"/PROGRESSTOKEN:";
			if (!memcmp(argv[i], autoInstall, sizeof(autoInstall) - sizeof(wchar_t)))
			{
				pAutoInstallDir = argv[i] + __countof(autoInstall) - 1;
			}
			else if (!memcmp(argv[i], progressToken, sizeof(progressToken) - sizeof(wchar_t)))
			{
				pProgressToken = argv[i] + __countof(progressToken) - 1;
			}
			else if (!_tcsicmp(argv[i], _T("/ELEVATED")))
				bAlreadyElevated = true;
			else if (!_tcsicmp(argv[i], _T("/ALLUSERS")))
				allUsers = true;
			else if (!_tcsicmp(argv[i], _T("/LOG")))
				writeLog = true;
		}
		else
			pFN = argv[i];
	}

	String fn;
#if _DEBUG
	size_t offset = 758272;
#else
	size_t offset = 825856;
#endif


	if (pFN)
	{
		fn = pFN;
		if (fn.find(L".exe") != fn.length() - 4)
			offset = 0;
	}
	else
	{
		DWORD done = GetModuleFileName(GetModuleHandle(NULL), fn.PreAllocate(MAX_PATH, false), MAX_PATH);
		fn.SetLength(done);
	}

	if (pAutoInstallDir)
		nRet = InstallUnattended(fn, offset, pAutoInstallDir, pProgressToken, bAlreadyElevated, lpstrCmdLine, allUsers, writeLog);
	else
	{
		ToolchainInstaller installer(fn, offset);
		if (!installer.Valid())
			MessageBox(HWND_DESKTOP, _T("Installation file is corrupt. Please visit http://gnutoolchains.com and re-download it."), _T("Error"), MB_ICONERROR);
		else
		{
			CMainDlg dlgMain(&installer, writeLog);
			nRet = dlgMain.DoModal();
		}
	}

	_Module.Term();
	::CoUninitialize();

	return nRet;
}

//--------------------------------------------------------------------------

struct SharedStatusStruct
{
	ULONGLONG Progress;
	ULONGLONG MaxProgress;
	int CurrentStatusPlusOne;	//-1 means "in progress"
	int ErrorTextLength;
	wchar_t wszErrorText[4000];
};

class PermissiveSecurityDescriptor
{
private:
	SECURITY_DESCRIPTOR m_SD;
	SECURITY_ATTRIBUTES m_SA;
	bool m_Used;
public:
	PermissiveSecurityDescriptor()
		: m_Used(false)
	{
		InitializeSecurityDescriptor (&m_SD, SECURITY_DESCRIPTOR_REVISION);
		SetSecurityDescriptorDacl (&m_SD, TRUE, NULL, FALSE);
		m_SA.nLength = sizeof(m_SA);
		m_SA.lpSecurityDescriptor = &m_SD;
		m_SA.bInheritHandle = FALSE;
	}

	operator SECURITY_ATTRIBUTES *()
	{
		m_Used = true;
		return &m_SA;
	}

	~PermissiveSecurityDescriptor()
	{
		ASSERT(m_Used);
	}
};

class UnattendedInstaller : public IInstallerCallbacks
{
private:
	SharedStatusStruct *m_pStatus;
	HANDLE m_hMapping;

public:
	UnattendedInstaller(wchar_t *pProgressToken)
		: m_hMapping(INVALID_HANDLE_VALUE)
		, m_pStatus(NULL)
	{
		TCHAR tszMappingName[MAX_PATH];
		_sntprintf_s(tszMappingName, __countof(tszMappingName), _TRUNCATE, L"com_gnutoolchains_StatusMapping_%s", pProgressToken);
		PermissiveSecurityDescriptor desc;
		m_hMapping = CreateFileMapping(INVALID_HANDLE_VALUE, desc, PAGE_READWRITE, 0, sizeof(SharedStatusStruct), tszMappingName);
		bool exists = (GetLastError() == ERROR_ALREADY_EXISTS);
		m_pStatus = (SharedStatusStruct *)MapViewOfFile(m_hMapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
		if ((m_hMapping == INVALID_HANDLE_VALUE) || !m_pStatus)
		{
			if (m_hMapping != INVALID_HANDLE_VALUE)
				CloseHandle(m_hMapping);
		}
		if (m_pStatus)
			memset(m_pStatus, 0, sizeof(SharedStatusStruct));
	}

	~UnattendedInstaller()
	{
		if (m_pStatus)
			UnmapViewOfFile(m_pStatus);
		if (m_hMapping != INVALID_HANDLE_VALUE)
			CloseHandle(m_hMapping);
	}

private:
	void ReportStatus(ActionStatus status, String errorText = L"")
	{
		if (!m_pStatus)
			return;
		int code = status.GetErrorCode();

		m_pStatus->ErrorTextLength = 0;
		String str = errorText;
		if (str.empty())
			str = status.GetMostInformativeText();
		int len = str.length();
		if (len >= __countof(m_pStatus->wszErrorText))
			len = __countof(m_pStatus->wszErrorText) - 1;

		memcpy(m_pStatus->wszErrorText, str.c_str(), len * sizeof(wchar_t));
		m_pStatus->wszErrorText[len] = 0;

		m_pStatus->ErrorTextLength = len;
		m_pStatus->CurrentStatusPlusOne = 1 + code;
	}

	void ReportProgress(ULONGLONG val, ULONGLONG max)
	{
		if (!m_pStatus)
			return;
		m_pStatus->MaxProgress = max;
		m_pStatus->Progress = val;
	}

public:
	int Install(String &fn, size_t offset, wchar_t *pDirectory, bool allUsers, bool writeLog)
	{
		ToolchainInstaller installer(fn, offset);
		if (!installer.Valid())
		{
			ActionStatus st = MAKE_STATUS(UnknownError);
			ReportStatus(st, _T("Installation file is corrupt. Please visit http://gnutoolchains.com and re-download it."));
			return st.GetErrorCode();
		}

		InstallationParameters  parameters;
		parameters.TargetDirectory = pDirectory;
		parameters.CreateHardlinks = true;
		parameters.AddToPath = false;
		parameters.AllUsers = allUsers;
		parameters.CreateInstallationLog = writeLog;

		installer.StartInstallationInSeparateThread(parameters, this, true);
		return 0;
	}

public:
	virtual void OnProgress(ULONGLONG total, ULONGLONG done, double ratio)
	{
		ReportProgress(done, total);
	}

	virtual void OnCompleted(ActionStatus status, BazisLib::String extraInfo, bool hasWarnings)
	{
		ReportStatus(status);
	}

	virtual void UpdateProgressText(const String & text) override
	{
	}
};

bool CheckWriteAccess(wchar_t *pDir)
{
	String strDir = pDir;
	String strFN = strDir + L"\\gcc_instal_test.dat";
	{
		File testFile(strFN, FileModes::CreateOrOpenRW);
		if (!testFile.Valid())
			return false;
		testFile.Close();
		File::Delete(strFN);
	}

	{
		if (!CreateDirectory(strFN.c_str(), NULL))
			return false;
		RemoveDirectory(strFN.c_str());
	}

	return true;
}

int InstallUnattended(String &fn, size_t offset, wchar_t *pDirectory, wchar_t *pProgressToken, bool bAlreadyElevated, LPTSTR lpCommandLine, bool allUsers, bool writeLog)
{
	Directory::Create(pDirectory);

	if ((allUsers || !CheckWriteAccess(pDirectory)) && !bAlreadyElevated)
	{
		String strNewCmdLine;
		TCHAR tszMe[MAX_PATH];
		GetModuleFileName(GetModuleHandle(NULL), tszMe, _countof(tszMe));

		strNewCmdLine.Format(_T("%s /ELEVATED"), lpCommandLine);
		SHELLEXECUTEINFO info = {0,};
		info.cbSize = sizeof(info);
		info.lpVerb = _T("runas");
		info.lpFile = tszMe;
		info.lpParameters = strNewCmdLine.c_str();

		info.fMask = SEE_MASK_NOCLOSEPROCESS;
		if (!ShellExecuteEx(&info))
			return GetLastError();

		WaitForSingleObject(info.hProcess, INFINITE);
		DWORD result = -1;
		GetExitCodeProcess(info.hProcess, &result);
		CloseHandle(info.hProcess);
		return result;
	}

	return UnattendedInstaller(pProgressToken).Install(fn, offset, pDirectory, allUsers, writeLog);
}
