using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using CodeEditPP;

namespace GCCBuilder
{
    public partial class TargetBuildControl : UserControl, IBuildProgressReporter
    {
        public TargetBuildControl()
        {
            InitializeComponent();
        }

        JobEditor _JobEditor;

        public GCCBuilder.JobEditor JobEditor
        {
            get { return _JobEditor; }
            set { _JobEditor = value; }
        }

        BuildJob _Job;
        string _ExtraLibs;

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            txtLog.BackColor = Color.Black;
            txtLog.ForeColor = Color.LightGray;
            txtLog.ReplaceDocument(new CodeEditPP.CodeDocument());
            _Job = _JobEditor.Job;
            _Job.Target = targetSettingsEditor1.Settings;
            _ExtraLibs = "/q/gnu/extralibs";

            new Thread(WorkerThreadBody).Start();
        }

        void WorkerThreadBody()
        {
            try
            {
                BuildToolkit toolkit;
                if (cbCygwin.Checked)
                    toolkit = new BuildToolkit { IsCygwin = true, RootDir = BuildManager.CygwinDirectory };
                else
                    toolkit = new BuildToolkit { IsCygwin = false, RootDir = @"c:\MinGW" };

                new BuildManager(_Job, toolkit, _ExtraLibs, this).DoBuild();
                ReportCompletion(null);
            }
            catch (System.Exception ex)
            {
                ReportCompletion(ex);
            }
        }

        DateTime _LastLogUpdateTime = DateTime.Now, _LastStatusUpdate = DateTime.Now;

        void LineReceived(GCCToolchainBuilder builder, string line)
        {
            _LastLogUpdateTime = DateTime.Now;
        }

        string _OptionalComment;

        private void timer1_Tick(object sender, EventArgs e)
        {
            var ts = (DateTime.Now - _LastLogUpdateTime);
            var tsB = (DateTime.Now - _LastStatusUpdate);

            string status = "";
            if (_OptionalComment != null)
                status = string.Format("{2} [{0:d2}:{1:d2}]", (int)tsB.TotalMinutes, tsB.Seconds, _OptionalComment);

            if (ts.TotalSeconds > 5)
                status += string.Format("; FROZEN FOR {0:d2}:{1:d2}!!!", (int)ts.TotalMinutes, ts.Seconds);
            else
                status += "...";

            lblElapsed.Text = status;
            lblElapsed.Visible = !button1.Enabled;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                using (var fs = File.CreateText(saveFileDialog1.FileName))
                    new XmlSerializer(typeof(BuildJob.TargetSettings)).Serialize(fs, targetSettingsEditor1.Settings);
        }

        public void ReportProgress(string statusText, long progressValue, long progressMax, string optionalComment)
        {
            _LastStatusUpdate = DateTime.Now;
            if (InvokeRequired)
                BeginInvoke(new ThreadStart(() => ReportProgress(statusText, progressValue, progressMax, optionalComment)));
            else
            {
                _OptionalComment = optionalComment;
                if (statusText != null)
                    label5.Text = statusText;
                if (progressMax == 0)
                    progressBar1.Style = ProgressBarStyle.Marquee;
                else
                {
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    if (progressValue > progressMax)
                        progressValue = progressMax;
                    progressBar1.Value = (int)((progressValue * progressBar1.Maximum) / progressMax);
                }
            }
        }

        public void ReportCompletion(Exception ex)
        {
            if (InvokeRequired)
                BeginInvoke(new ThreadStart(() => ReportCompletion(ex)));
            else
            {
                button1.Enabled = true;
                if (ex == null)
                    MessageBox.Show("Build complete", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                {
                    txtLog.BackColor = Color.Red;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                using (var fs = File.OpenRead(openFileDialog1.FileName))
                    targetSettingsEditor1.Settings = (BuildJob.TargetSettings)new XmlSerializer(typeof(BuildJob.TargetSettings)).Deserialize(fs);
        }

        int _UpdatesSinceLastTick;
        const int MaxUpdatesPerTick = 8;

        object _PendingTextLock = new object();
        string _PendingText = null; //Text queued from another thread without causing an immediate update. Will be processed by timer handler.


        public void HandleOutputFromArbitraryThread(string text)
        {
            lock (_PendingTextLock)
                _PendingText += text;

            if (System.Threading.Interlocked.Increment(ref _UpdatesSinceLastTick) < MaxUpdatesPerTick)
                try
                {
                    if (InvokeRequired)
                        BeginInvoke(new System.Threading.ThreadStart(HandlePendingText));
                }
                catch { }
        }

        void HandlePendingText()
        {
            string txt;
            lock (_PendingTextLock)
            {
                txt = _PendingText;
                _PendingText = null;
            }

            if (txt != null)
            {
                CodeDocument.Location loc;
                int lineCnt = txtLog.LineCount;
                if (lineCnt == 0)
                    loc = new ReadonlyCodeDocument.Location(0, 0);
                else
                    loc = new ReadonlyCodeDocument.Location(txtLog.GetLine(lineCnt - 1).Length, lineCnt - 1);
                txtLog.ReplaceTextInDocument(loc, loc, true, txt);
            }
        }

        private void fastUpdateTimer_Tick(object sender, EventArgs e)
        {
            _UpdatesSinceLastTick = 0;
            HandlePendingText();
        }



        public void ReportTextLine(string text)
        {
            HandleOutputFromArbitraryThread(text.Replace("\n", "\r\n") + "\r\n");
            _LastLogUpdateTime = DateTime.Now;
        }

        EventHandler _StopCallback;

        public EventHandler StopCallback
        {
            set
            {
                _StopCallback = value;
                BeginInvoke(new ThreadStart(() => btnStop.Enabled = value != null));
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            var cb = _StopCallback;
            if (cb != null)
                cb(this, EventArgs.Empty);
        }
    }
}
