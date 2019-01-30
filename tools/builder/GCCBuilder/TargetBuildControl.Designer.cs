namespace GCCBuilder
{
    partial class TargetBuildControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetBuildControl));
            CodeEditPP.SelectionStyle selectionStyle1 = new CodeEditPP.SelectionStyle();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblElapsed = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.cbCygwin = new System.Windows.Forms.CheckBox();
            this.targetSettingsEditor1 = new GCCBuilder.TargetSettingsEditor();
            this.fastUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.txtLog = new CodeEditPP.BasicTextEditor();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(456, 404);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 28;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblElapsed
            // 
            this.lblElapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblElapsed.AutoSize = true;
            this.lblElapsed.Location = new System.Drawing.Point(104, 409);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(41, 13);
            this.lblElapsed.TabIndex = 24;
            this.lblElapsed.Text = "label11";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(3, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(667, 33);
            this.label5.TabIndex = 23;
            this.label5.Text = "Downloading....";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(2, 186);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(693, 23);
            this.progressBar1.TabIndex = 22;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(0, 404);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 21;
            this.button1.Text = "Build";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Target files|*.tgtx";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Target files|*.tgtx";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Location = new System.Drawing.Point(664, 404);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(29, 23);
            this.button3.TabIndex = 41;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button2_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(631, 404);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(27, 23);
            this.button2.TabIndex = 42;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button3_Click);
            // 
            // cbCygwin
            // 
            this.cbCygwin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCygwin.AutoSize = true;
            this.cbCygwin.Location = new System.Drawing.Point(586, 149);
            this.cbCygwin.Name = "cbCygwin";
            this.cbCygwin.Size = new System.Drawing.Size(107, 17);
            this.cbCygwin.TabIndex = 43;
            this.cbCygwin.Text = "Build with cygwin";
            this.cbCygwin.UseVisualStyleBackColor = true;
            // 
            // targetSettingsEditor1
            // 
            this.targetSettingsEditor1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetSettingsEditor1.Location = new System.Drawing.Point(6, 3);
            this.targetSettingsEditor1.Name = "targetSettingsEditor1";
            this.targetSettingsEditor1.Size = new System.Drawing.Size(687, 144);
            this.targetSettingsEditor1.TabIndex = 29;
            // 
            // fastUpdateTimer
            // 
            this.fastUpdateTimer.Enabled = true;
            this.fastUpdateTimer.Interval = 200;
            this.fastUpdateTimer.Tick += new System.EventHandler(this.fastUpdateTimer_Tick);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.AutoScroll = true;
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.BorderlessMode = false;
            this.txtLog.CaretRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.txtLog.ConsoleMode = false;
            this.txtLog.EnableManualResizeOverride = false;
            this.txtLog.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtLog.FreezeCursor = false;
            this.txtLog.FreezeHorizontalScroll = false;
            this.txtLog.HideCursor = false;
            this.txtLog.HideSideBars = true;
            this.txtLog.HScrollVisible = true;
            this.txtLog.LineListVisible = true;
            this.txtLog.LineOffset = 0;
            this.txtLog.Location = new System.Drawing.Point(2, 216);
            this.txtLog.MainSelectionStyle = selectionStyle1;
            this.txtLog.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtLog.Name = "txtLog";
            this.txtLog.OverwriteMode = false;
            this.txtLog.ReadOnly = false;
            this.txtLog.SingleLineMode = false;
            this.txtLog.Size = new System.Drawing.Size(691, 181);
            this.txtLog.SmartScrollEnabled = false;
            this.txtLog.TabIndex = 44;
            this.txtLog.TabSize = 4;
            this.txtLog.UnderlineLongBinaryNumbers_Deprecated = false;
            this.txtLog.Unsaved = false;
            this.txtLog.VScrollVisible = true;
            // 
            // TargetBuildControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.cbCygwin);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.targetSettingsEditor1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.lblElapsed);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button1);
            this.Name = "TargetBuildControl";
            this.Size = new System.Drawing.Size(698, 430);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblElapsed;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private TargetSettingsEditor targetSettingsEditor1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox cbCygwin;
        private System.Windows.Forms.Timer fastUpdateTimer;
        private CodeEditPP.BasicTextEditor txtLog;
    }
}
