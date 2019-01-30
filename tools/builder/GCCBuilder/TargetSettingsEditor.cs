using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GCCBuilder
{
    public partial class TargetSettingsEditor : UserControl
    {
        public TargetSettingsEditor()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BuildJob.TargetSettings Settings
        {
            get
            {
                    var Target =  new BuildJob.TargetSettings
                    {
                        Target = textBox2.Text,
                        SysrootSuffix = txtSysroot.Text,
                        DisableSharedLibraries = cbNoShared.Checked,
                        AdditionalArgs = txtConfig.Text,
                        UserFriendlyName = txtName.Text,
                        XMLSupportInGDB = cbGDBXML.Checked,
                    };

                if (rbGlibc.Checked)
                {
                    Target.Libc.Type = BuildJob.LIBCType.Glibc;
                    Target.Libc.Version = cbGlibc.Text;
                }
                else if (rbNewlib.Checked)
                {
                    Target.Libc.Type = BuildJob.LIBCType.Newlib;
                    Target.Libc.Version = cbNewlib.Text;
                }
                else
                    Target.Libc.Type = BuildJob.LIBCType.None;

                return Target;
            }
            set
            {
                textBox2.Text = value.Target;
                txtName.Text = value.UserFriendlyName;
                txtSysroot.Text = value.SysrootSuffix;

                cbNoShared.Checked = value.DisableSharedLibraries;
                txtConfig.Text = value.AdditionalArgs;

                cbGDBXML.Checked = value.XMLSupportInGDB;

                switch (value.Libc.Type)
                {
                    case BuildJob.LIBCType.Glibc:
                        cbGlibc.Text = value.Libc.Version;
                        rbGlibc.Checked = true;
                        break;
                    case BuildJob.LIBCType.Newlib:
                        cbNewlib.Text = value.Libc.Version;
                        rbNewlib.Checked = true;
                        break;
                    case BuildJob.LIBCType.None:
                        rbLibc.Checked = true;
                        break;
                }
            }
        }
    }
}
