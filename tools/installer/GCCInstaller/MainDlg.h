// MainDlg.h : interface of the CMainDlg class
//
/////////////////////////////////////////////////////////////////////////////

#pragma once
#include "ToolchainInstaller.h"
#include <bzscore/sync.h>

enum {WMX_STATUS = WM_USER + 1};

class CMainDlg : public CDialogImpl<CMainDlg>, public IInstallerCallbacks
{
private:
	ToolchainInstaller *m_pInstaller;
	bool _NeedUACForAllUsers;
	bool _WriteLog;
	
	String m_LastStatus, _ExtraErrorInfo;
	Win32::CCriticalSection m_StatusLock;

public:
	enum { IDD = IDD_MAINDLG };

	BEGIN_MSG_MAP(CMainDlg)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		MESSAGE_HANDLER(WM_TIMER, OnTimer)
		COMMAND_ID_HANDLER(IDCANCEL, OnCancel)
		COMMAND_HANDLER(IDOK, BN_CLICKED, OnBnClickedOk)
		MESSAGE_HANDLER(WMX_STATUS, OnCustomError)
		COMMAND_HANDLER(IDC_AGREE, BN_CLICKED, OnBnClickedAgree)
		COMMAND_HANDLER(IDC_ALLUSERS, BN_CLICKED, OnBnClickedAllusers)
		COMMAND_HANDLER(IDC_LOCALUSER, BN_CLICKED, OnBnClickedLocaluser)
		COMMAND_HANDLER(IDC_BROWSE, BN_CLICKED, OnBnClickedBrowse)
	END_MSG_MAP()

	CMainDlg(ToolchainInstaller *pInstaller, bool writeLog)
		: m_pInstaller(pInstaller)
		, _WriteLog(writeLog)
	{
		_NeedUACForAllUsers = false;
	}

	void EnableSelectionGUI(bool enable);

// Handler prototypes (uncomment arguments if needed):
//	LRESULT MessageHandler(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
//	LRESULT CommandHandler(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
//	LRESULT NotifyHandler(int /*idCtrl*/, LPNMHDR /*pnmh*/, BOOL& /*bHandled*/)

	LRESULT OnInitDialog(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnTimer(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnCustomError(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnCancel(WORD /*wNotifyCode*/, WORD wID, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedOk(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

	virtual void OnProgress(ULONGLONG total, ULONGLONG done, double percentage);
	virtual void OnCompleted(ActionStatus status, BazisLib::String extraErrorInfo);
	virtual void UpdateProgressText(const String &text);


	LRESULT OnBnClickedAgree(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedAllusers(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedLocaluser(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBrowse(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
};
