using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace GCCBuilder
{
    public partial class JobEditor : UserControl
    {
        public JobEditor()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BuildJob Job
        {
            get
            {
                BuildJob job = new BuildJob
                {
                    Binutils = new BuildJob.BinutilsSettings { BinutilsVersion = cbBinutils.Text },
                    GCC = new BuildJob.GCCSettings
                    {
                        GCCVersion = cbGCC.Text,
                        GMPVersion = cbGMP.Text,
                        MPFRVersion = cbMPFR.Text,
                        MPCVersion = cbMPC.Text,
                    },
                    GDB = new BuildJob.GDBSettings { GDBVersion = cbGDB.Text },

                    ParalleliationLevel = (int)numericUpDown1.Value,
                    NoParallelGCC = cbSerialGCC.Checked,
                    Build = new BuildJob.BuildSettings
                    {
                        LocalDirectory = textBox1.Text,
                        OutputDirectory = textBox2.Text,
                        SiteDirectory = txtSite.Text,
                        ExtraFilesDirectory = txtToolchainFiles.Text,
                    }

                };


                return job;
            }
            set
            {
                if (value == null)
                    return;

                cbBinutils.Text = value.Binutils.BinutilsVersion;

                cbGCC.Text = value.GCC.GCCVersion;
                cbGMP.Text = value.GCC.GMPVersion;
                cbMPFR.Text = value.GCC.MPFRVersion;
                cbMPC.Text = value.GCC.MPCVersion;

                cbGDB.Text = value.GDB.GDBVersion;

                numericUpDown1.Value = value.ParalleliationLevel;
                cbSerialGCC.Checked = value.NoParallelGCC;

                textBox1.Text = value.Build.LocalDirectory;
                textBox2.Text = value.Build.OutputDirectory;
                txtSite.Text = value.Build.SiteDirectory;
                txtToolchainFiles.Text = value.Build.ExtraFilesDirectory;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                using (var fs = File.OpenRead(openFileDialog1.FileName))
                    Job = (BuildJob)new XmlSerializer(typeof(BuildJob)).Deserialize(fs);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                using (var fs = File.CreateText(saveFileDialog1.FileName))
                    new XmlSerializer(typeof(BuildJob)).Serialize(fs, Job);
        }

    }
}
