namespace GCCBuilder
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.targetBuildControl1 = new GCCBuilder.TargetBuildControl();
            this.jobEditor1 = new GCCBuilder.JobEditor();
            this.SuspendLayout();
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "XML files|*.xml";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "XML files|*.xml";
            // 
            // targetBuildControl1
            // 
            this.targetBuildControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetBuildControl1.JobEditor = this.jobEditor1;
            this.targetBuildControl1.Location = new System.Drawing.Point(15, 159);
            this.targetBuildControl1.Name = "targetBuildControl1";
            this.targetBuildControl1.Size = new System.Drawing.Size(664, 515);
            this.targetBuildControl1.TabIndex = 19;
            // 
            // jobEditor1
            // 
            this.jobEditor1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.jobEditor1.Location = new System.Drawing.Point(12, 12);
            this.jobEditor1.Name = "jobEditor1";
            this.jobEditor1.Size = new System.Drawing.Size(667, 141);
            this.jobEditor1.TabIndex = 18;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 686);
            this.Controls.Add(this.targetBuildControl1);
            this.Controls.Add(this.jobEditor1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private JobEditor jobEditor1;
        private TargetBuildControl targetBuildControl1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

