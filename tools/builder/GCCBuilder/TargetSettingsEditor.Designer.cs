namespace GCCBuilder
{
    partial class TargetSettingsEditor
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
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbNoShared = new System.Windows.Forms.CheckBox();
            this.rbLibc = new System.Windows.Forms.RadioButton();
            this.rbNewlib = new System.Windows.Forms.RadioButton();
            this.rbGlibc = new System.Windows.Forms.RadioButton();
            this.cbNewlib = new System.Windows.Forms.ComboBox();
            this.cbGlibc = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSysroot = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtConfig = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cbGDBXML = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(103, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(180, 20);
            this.textBox2.TabIndex = 15;
            this.textBox2.Text = "arm-eabi";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(2, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Target:";
            // 
            // cbNoShared
            // 
            this.cbNoShared.AutoSize = true;
            this.cbNoShared.Location = new System.Drawing.Point(474, 6);
            this.cbNoShared.Name = "cbNoShared";
            this.cbNoShared.Size = new System.Drawing.Size(164, 17);
            this.cbNoShared.TabIndex = 17;
            this.cbNoShared.Text = "Disable shared library support";
            this.cbNoShared.UseVisualStyleBackColor = true;
            // 
            // rbLibc
            // 
            this.rbLibc.AutoSize = true;
            this.rbLibc.Location = new System.Drawing.Point(432, 23);
            this.rbLibc.Name = "rbLibc";
            this.rbLibc.Size = new System.Drawing.Size(101, 17);
            this.rbLibc.TabIndex = 21;
            this.rbLibc.TabStop = true;
            this.rbLibc.Text = "Do not build libc";
            this.rbLibc.UseVisualStyleBackColor = true;
            // 
            // rbNewlib
            // 
            this.rbNewlib.AutoSize = true;
            this.rbNewlib.Checked = true;
            this.rbNewlib.Location = new System.Drawing.Point(219, 22);
            this.rbNewlib.Name = "rbNewlib";
            this.rbNewlib.Size = new System.Drawing.Size(80, 17);
            this.rbNewlib.TabIndex = 22;
            this.rbNewlib.TabStop = true;
            this.rbNewlib.Text = "Use newlib:";
            this.rbNewlib.UseVisualStyleBackColor = true;
            // 
            // rbGlibc
            // 
            this.rbGlibc.AutoSize = true;
            this.rbGlibc.Location = new System.Drawing.Point(6, 21);
            this.rbGlibc.Name = "rbGlibc";
            this.rbGlibc.Size = new System.Drawing.Size(72, 17);
            this.rbGlibc.TabIndex = 23;
            this.rbGlibc.TabStop = true;
            this.rbGlibc.Text = "Use glibc:";
            this.rbGlibc.UseVisualStyleBackColor = true;
            // 
            // cbNewlib
            // 
            this.cbNewlib.FormattingEnabled = true;
            this.cbNewlib.Location = new System.Drawing.Point(305, 22);
            this.cbNewlib.Name = "cbNewlib";
            this.cbNewlib.Size = new System.Drawing.Size(121, 21);
            this.cbNewlib.TabIndex = 19;
            this.cbNewlib.Text = "1.20.0";
            // 
            // cbGlibc
            // 
            this.cbGlibc.FormattingEnabled = true;
            this.cbGlibc.Location = new System.Drawing.Point(92, 21);
            this.cbGlibc.Name = "cbGlibc";
            this.cbGlibc.Size = new System.Drawing.Size(121, 21);
            this.cbGlibc.TabIndex = 20;
            this.cbGlibc.Text = "2.14.1";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbNewlib);
            this.groupBox1.Controls.Add(this.rbLibc);
            this.groupBox1.Controls.Add(this.cbGlibc);
            this.groupBox1.Controls.Add(this.cbNewlib);
            this.groupBox1.Controls.Add(this.rbGlibc);
            this.groupBox1.Controls.Add(this.cbGDBXML);
            this.groupBox1.Location = new System.Drawing.Point(3, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(676, 58);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Libc";
            // 
            // txtSysroot
            // 
            this.txtSysroot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSysroot.Location = new System.Drawing.Point(190, 87);
            this.txtSysroot.Name = "txtSysroot";
            this.txtSysroot.Size = new System.Drawing.Size(489, 20);
            this.txtSysroot.TabIndex = 26;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 90);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(163, 13);
            this.label12.TabIndex = 25;
            this.label12.Text = "Sysroot directory name (optional):";
            // 
            // txtConfig
            // 
            this.txtConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfig.Location = new System.Drawing.Point(190, 113);
            this.txtConfig.Name = "txtConfig";
            this.txtConfig.Size = new System.Drawing.Size(489, 20);
            this.txtConfig.TabIndex = 28;
            this.txtConfig.Text = "--enable-interwork --enable-multilib --with-float=soft";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 116);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(157, 13);
            this.label9.TabIndex = 27;
            this.label9.Text = "Additional configuration options:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(297, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(349, 3);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(119, 20);
            this.txtName.TabIndex = 15;
            this.txtName.Text = "ARM";
            // 
            // cbGDBXML
            // 
            this.cbGDBXML.AutoSize = true;
            this.cbGDBXML.Location = new System.Drawing.Point(539, 23);
            this.cbGDBXML.Name = "cbGDBXML";
            this.cbGDBXML.Size = new System.Drawing.Size(123, 17);
            this.cbGDBXML.TabIndex = 17;
            this.cbGDBXML.Text = "XML support in GDB";
            this.cbGDBXML.UseVisualStyleBackColor = true;
            // 
            // TargetSettingsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtConfig);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtSysroot);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbNoShared);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label7);
            this.Name = "TargetSettingsEditor";
            this.Size = new System.Drawing.Size(679, 144);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbNoShared;
        private System.Windows.Forms.RadioButton rbLibc;
        private System.Windows.Forms.RadioButton rbNewlib;
        private System.Windows.Forms.RadioButton rbGlibc;
        private System.Windows.Forms.ComboBox cbNewlib;
        private System.Windows.Forms.ComboBox cbGlibc;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtSysroot;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtConfig;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.CheckBox cbGDBXML;
    }
}
