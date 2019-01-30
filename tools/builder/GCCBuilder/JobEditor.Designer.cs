namespace GCCBuilder
{
    partial class JobEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JobEditor));
            this.cbBinutils = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbGDB = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cbMPC = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cbMPFR = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbGMP = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbGCC = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSerialGCC = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.txtToolchainFiles = new System.Windows.Forms.TextBox();
            this.txtSite = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // cbBinutils
            // 
            this.cbBinutils.FormattingEnabled = true;
            this.cbBinutils.Location = new System.Drawing.Point(348, 27);
            this.cbBinutils.Name = "cbBinutils";
            this.cbBinutils.Size = new System.Drawing.Size(121, 21);
            this.cbBinutils.TabIndex = 8;
            this.cbBinutils.Text = "2.22";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(259, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Binutils version:";
            // 
            // cbGDB
            // 
            this.cbGDB.FormattingEnabled = true;
            this.cbGDB.Location = new System.Drawing.Point(551, 27);
            this.cbGDB.Name = "cbGDB";
            this.cbGDB.Size = new System.Drawing.Size(128, 21);
            this.cbGDB.TabIndex = 9;
            this.cbGDB.Text = "7.5.1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(475, 30);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(70, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "GDB version:";
            // 
            // cbMPC
            // 
            this.cbMPC.FormattingEnabled = true;
            this.cbMPC.Location = new System.Drawing.Point(551, 0);
            this.cbMPC.Name = "cbMPC";
            this.cbMPC.Size = new System.Drawing.Size(128, 21);
            this.cbMPC.TabIndex = 10;
            this.cbMPC.Text = "0.8";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(475, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "MPC version:";
            // 
            // cbMPFR
            // 
            this.cbMPFR.FormattingEnabled = true;
            this.cbMPFR.Location = new System.Drawing.Point(348, 0);
            this.cbMPFR.Name = "cbMPFR";
            this.cbMPFR.Size = new System.Drawing.Size(121, 21);
            this.cbMPFR.TabIndex = 11;
            this.cbMPFR.Text = "2.4.1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(259, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "MPFR version:";
            // 
            // cbGMP
            // 
            this.cbGMP.FormattingEnabled = true;
            this.cbGMP.Location = new System.Drawing.Point(104, 27);
            this.cbGMP.Name = "cbGMP";
            this.cbGMP.Size = new System.Drawing.Size(121, 21);
            this.cbGMP.TabIndex = 12;
            this.cbGMP.Text = "4.2.4";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "GMP version:";
            // 
            // cbGCC
            // 
            this.cbGCC.FormattingEnabled = true;
            this.cbGCC.Location = new System.Drawing.Point(104, 0);
            this.cbGCC.Name = "cbGCC";
            this.cbGCC.Size = new System.Drawing.Size(121, 21);
            this.cbGCC.TabIndex = 13;
            this.cbGCC.Text = "4.7.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "GCC version:";
            // 
            // cbSerialGCC
            // 
            this.cbSerialGCC.AutoSize = true;
            this.cbSerialGCC.Location = new System.Drawing.Point(127, 55);
            this.cbSerialGCC.Name = "cbSerialGCC";
            this.cbSerialGCC.Size = new System.Drawing.Size(220, 17);
            this.cbSerialGCC.TabIndex = 18;
            this.cbSerialGCC.Text = "Disable parallelization when building GCC";
            this.cbSerialGCC.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(77, 54);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(44, 20);
            this.numericUpDown1.TabIndex = 30;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 56);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "Parallel jobs:";
            // 
            // txtToolchainFiles
            // 
            this.txtToolchainFiles.Location = new System.Drawing.Point(385, 103);
            this.txtToolchainFiles.Name = "txtToolchainFiles";
            this.txtToolchainFiles.Size = new System.Drawing.Size(287, 20);
            this.txtToolchainFiles.TabIndex = 36;
            this.txtToolchainFiles.Text = "E:\\PROJECTS\\sysprogs\\support\\ToolchainFiles ";
            // 
            // txtSite
            // 
            this.txtSite.Location = new System.Drawing.Point(385, 80);
            this.txtSite.Name = "txtSite";
            this.txtSite.Size = new System.Drawing.Size(287, 20);
            this.txtSite.TabIndex = 38;
            this.txtSite.Text = "f:\\sysprogs-site\\toolchains";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(256, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 13);
            this.label5.TabIndex = 33;
            this.label5.Text = "Extra files for toolchains:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(294, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Site directory:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(94, 106);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(104, 20);
            this.textBox2.TabIndex = 37;
            this.textBox2.Text = "f:\\gnu\\out";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 109);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 13);
            this.label7.TabIndex = 32;
            this.label7.Text = "Output directory:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(94, 80);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(103, 20);
            this.textBox1.TabIndex = 35;
            this.textBox1.Text = "f:\\gnu\\auto";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 83);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 13);
            this.label9.TabIndex = 31;
            this.label9.Text = "Local directory:";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Location = new System.Drawing.Point(647, 51);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(29, 23);
            this.button3.TabIndex = 39;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(614, 51);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(27, 23);
            this.button2.TabIndex = 40;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Toolchain job files|*.jobx";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Toolchain job files|*.jobx";
            // 
            // JobEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtToolchainFiles);
            this.Controls.Add(this.txtSite);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.cbSerialGCC);
            this.Controls.Add(this.cbBinutils);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbGDB);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cbMPC);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cbMPFR);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbGMP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbGCC);
            this.Controls.Add(this.label1);
            this.Name = "JobEditor";
            this.Size = new System.Drawing.Size(679, 137);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbBinutils;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbGDB;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cbMPC;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbMPFR;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbGMP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbGCC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbSerialGCC;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtToolchainFiles;
        private System.Windows.Forms.TextBox txtSite;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}
