// MainDlg.cpp : implementation of the CMainDlg class
//
/////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "resource.h"

#include "MainDlg.h"
#include <bzscore/Win32/registry.h>

void CMainDlg::EnableSelectionGUI(bool enable)
{
	::EnableWindow(GetDlgItem(IDC_AGREE), enable);
	::EnableWindow(GetDlgItem(IDC_EDIT1), enable);
	::EnableWindow(GetDlgItem(IDC_BROWSE), enable);
	::EnableWindow(GetDlgItem(IDC_LOCALUSER), enable);
	::EnableWindow(GetDlgItem(IDC_ALLUSERS), enable);
	::EnableWindow(GetDlgItem(IDC_HARDLINK), enable);
	::EnableWindow(GetDlgItem(IDC_ADDTOPATH), enable);
}

LRESULT CMainDlg::OnInitDialog(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
{
	// center the dialog on the screen
	CenterWindow();

	HRSRC hRes = FindResource(GetModuleHandle(0), _T("GPL"), _T("LICENSE"));
	HGLOBAL hResObj = LoadResource(GetModuleHandle(NULL), hRes);
	LPVOID lpData = LockResource(hResObj);
	size_t size = SizeofResource(GetModuleHandle(0), hRes);

	if (size && lpData)
	{
		char *pData = new char[size + 1];
		memcpy(pData, lpData, size);
		pData[size] = 0;
		::SetDlgItemTextA(m_hWnd, IDC_EDIT2, pData);
		delete pData;
	}


	// set icons
	HICON hIcon = (HICON)::LoadImage(_Module.GetResourceInstance(), MAKEINTRESOURCE(IDR_MAINFRAME), 
		IMAGE_ICON, ::GetSystemMetrics(SM_CXICON), ::GetSystemMetrics(SM_CYICON), LR_DEFAULTCOLOR);
	SetIcon(hIcon, TRUE);
	HICON hIconSmall = (HICON)::LoadImage(_Module.GetResourceInstance(), MAKEINTRESOURCE(IDR_MAINFRAME), 
		IMAGE_ICON, ::GetSystemMetrics(SM_CXSMICON), ::GetSystemMetrics(SM_CYSMICON), LR_DEFAULTCOLOR);
	SetIcon(hIconSmall, FALSE);

	SendDlgItemMessage(IDC_PROGRESS1, PBM_SETRANGE, 0, MAKELONG(0, 1000));

	SetDlgItemText(IDC_EDIT1, (String(_T("C:\\SysGCC\\") + ANSIStringToString(TempStrPointerWrapperA(m_pInstaller->GetHeader()->Target)))).c_str());
	SendDlgItemMessage(IDC_HARDLINK, BM_SETCHECK, BST_CHECKED);
	SendDlgItemMessage(IDC_ADDTOPATH, BM_SETCHECK, BST_CHECKED);

	SetTimer(123, 100);

	ActionStatus st;
	RegistryKey regKey(HKEY_LOCAL_MACHINE, _T("SOFTWARE"), 0, true, &st);
	if (!st.Successful())
	{
		SendDlgItemMessage(IDC_LOCALUSER, BM_SETCHECK, BST_CHECKED);
		_NeedUACForAllUsers = true;
	}
	else
		SendDlgItemMessage(IDC_ALLUSERS, BM_SETCHECK, BST_CHECKED);

	::EnableWindow(GetDlgItem(IDOK), false);

	HFONT hFont = CreateFont(-40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ANTIALIASED_QUALITY, 0, _T("Calibri"));
	SendDlgItemMessage(IDC_CAPTION, WM_SETFONT, (WPARAM)hFont, TRUE);

	SetDlgItemText(IDC_CAPTION, String::sFormat(L"GNU toolchain for %S", m_pInstaller->GetHeader()->Target).c_str());

	SetDlgItemText(IDC_BINUTILS, ANSIStringToString(TempStrPointerWrapperA(m_pInstaller->GetHeader()->BinutilsVersion)).c_str());
	SetDlgItemText(IDC_GCC, ANSIStringToString(TempStrPointerWrapperA(m_pInstaller->GetHeader()->GCCVersion)).c_str());
	SetDlgItemText(IDC_GDB, ANSIStringToString(TempStrPointerWrapperA(m_pInstaller->GetHeader()->GDBVersion)).c_str());
	if (m_pInstaller->GetHeader()->NewlibVersion[0])
		SetDlgItemText(IDC_NEWLIB, ANSIStringToString(TempStrPointerWrapperA(m_pInstaller->GetHeader()->NewlibVersion)).c_str());
	else
	{
		::ShowWindow(GetDlgItem(IDC_NEWLIB_LBL), SW_HIDE);
		::ShowWindow(GetDlgItem(IDC_NEWLIB), SW_HIDE);
	}


	//SendDlgItemMessage(IDC_ADDTOPATH, BM_SETCHECK, BST_CHECKED);

	return TRUE;
}

LRESULT CMainDlg::OnTimer(UINT, WPARAM, LPARAM, BOOL &)
{
	FastLocker<Win32::CCriticalSection> lck(m_StatusLock);
	SetDlgItemText(IDC_PROGRESSTEXT, m_LastStatus.c_str());
	return 0;
}

LRESULT CMainDlg::OnCancel(WORD /*wNotifyCode*/, WORD wID, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	m_pInstaller->AbortInstallation();
	EndDialog(wID);
	return 0;
}


LRESULT CMainDlg::OnBnClickedOk(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& bHandled)
{
	// TODO: Add your control notification handler code here
	InstallationParameters parameters;
	TCHAR tszDir[MAX_PATH] = {0,};
	GetDlgItemText(IDC_EDIT1, tszDir, _countof(tszDir));

	parameters.TargetDirectory = tszDir;
	if (parameters.TargetDirectory.find(' ') != -1)
	{
		MessageBox(_T("Installation path should not contain spaces!"), NULL, MB_ICONERROR);
		return 0;
	}

	parameters.CreateHardlinks = !!(SendDlgItemMessage(IDC_HARDLINK, BM_GETCHECK) & BST_CHECKED);
	parameters.AddToPath = !!(SendDlgItemMessage(IDC_ADDTOPATH, BM_GETCHECK) & BST_CHECKED);
	parameters.AllUsers = !!(SendDlgItemMessage(IDC_ALLUSERS, BM_GETCHECK) & BST_CHECKED);
	parameters.CreateInstallationLog = _WriteLog;

	if (_NeedUACForAllUsers && parameters.AllUsers)
	{
		TCHAR tszMe[MAX_PATH];
		GetModuleFileName(GetModuleHandle(NULL), tszMe, _countof(tszMe));
		if ((int)ShellExecute(HWND_DESKTOP, _T("runas"), tszMe, _WriteLog ? L"/LOG" : NULL, NULL, SW_SHOW) > 32)
		{
			EndDialog(0);
			return 0;
		}
	}

	::EnableWindow(GetDlgItem(IDOK), FALSE);
	EnableSelectionGUI(false);
	m_pInstaller->StartInstallationInSeparateThread(parameters, this);

	bHandled = TRUE;
	return 0;
}

void CMainDlg::OnProgress( ULONGLONG total, ULONGLONG done, double ratio )
{
	::PostMessage(GetDlgItem(IDC_PROGRESS1), PBM_SETPOS, (int)(ratio * 1000), 0);
}

void CMainDlg::OnCompleted( ActionStatus status )
{
	::EnableWindow(GetDlgItem(IDOK), TRUE);
	EnableSelectionGUI(true);
	SendMessage(WMX_STATUS, 0, (LPARAM)&status);
}

void CMainDlg::UpdateProgressText(const String & text)
{
	FastLocker<Win32::CCriticalSection> lck(m_StatusLock);
	m_LastStatus = text;
}

LRESULT CMainDlg::OnCustomError( UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM lParam, BOOL& /*bHandled*/ )
{
	ActionStatus *pStatus = (ActionStatus *)lParam;
	if (pStatus)
	{
		if (pStatus->Successful())
		{
			SendDlgItemMessage(IDC_PROGRESS1, PBM_SETPOS, 1000, 0);
			MessageBox(_T("Installation succeeded"), _T("Information"), MB_ICONINFORMATION);
			EndDialog(0);
		}
		else
			MessageBox(pStatus->GetMostInformativeText().c_str(), NULL, MB_ICONERROR);
	}
	return 0;
}


LRESULT CMainDlg::OnBnClickedAgree(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	bool agree = !!(SendDlgItemMessage(IDC_AGREE, BM_GETCHECK) & BST_CHECKED);
	::EnableWindow(GetDlgItem(IDOK), agree);

	return 0;
}


LRESULT CMainDlg::OnBnClickedAllusers(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	if (_NeedUACForAllUsers)
	{
		bool allUsers = !!(SendDlgItemMessage(IDC_ALLUSERS, BM_GETCHECK) & BST_CHECKED);
		if (allUsers)
			SendDlgItemMessage(IDOK, BCM_SETSHIELD, 0, TRUE);
		else
			SendDlgItemMessage(IDOK, BCM_SETSHIELD, 0, FALSE);
	}
	return 0;
}


LRESULT CMainDlg::OnBnClickedLocaluser(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	if (_NeedUACForAllUsers)
	{
		bool allUsers = !!(SendDlgItemMessage(IDC_ALLUSERS, BM_GETCHECK) & BST_CHECKED);
		if (allUsers)
			SendDlgItemMessage(IDOK, BCM_SETSHIELD, 0, TRUE);
		else
			SendDlgItemMessage(IDOK, BCM_SETSHIELD, 0, FALSE);
	}
	return 0;
}


LRESULT CMainDlg::OnBnClickedBrowse(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	CFolderDialog dlg(m_hWnd);
	GetDlgItemText(IDC_EDIT1, dlg.m_szFolderPath, _countof(dlg.m_szFolderPath));
	if (dlg.DoModal() == IDOK)
	{
		if (dlg.m_szFolderPath[0] && dlg.m_szFolderPath[_tcslen(dlg.m_szFolderPath) - 1] == '\\')
			_tcsncat(dlg.m_szFolderPath, _T("SysGCC"), _countof(dlg.m_szFolderPath));
		else
			_tcsncat(dlg.m_szFolderPath, _T("\\SysGCC"), _countof(dlg.m_szFolderPath));
		SetDlgItemText(IDC_EDIT1, dlg.m_szFolderPath);
	}

	return 0;
}
