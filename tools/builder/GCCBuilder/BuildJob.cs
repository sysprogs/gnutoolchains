using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCCBuilder
{
    public class BuildJob
    {
        public struct BinutilsSettings
        {
            public string BinutilsVersion;
        }

        public struct GCCSettings
        {
            public string GCCVersion;
            public string GMPVersion;
            public string MPFRVersion;
            public string MPCVersion;
        }

        public struct TargetSettings
        {
            public string Target;
            public string UserFriendlyName;
            public string SysrootSuffix;
            public bool DisableSharedLibraries;
            public string AdditionalArgs;

            public LIBCSettings Libc;
            public bool XMLSupportInGDB;
        }

        public enum LIBCType
        {
            Glibc,
            Newlib,
            None,
        }

        public struct LIBCSettings
        {
            public LIBCType Type;
            public string Version;

            public string UserFriendlyVersion
            {
                get
                {
                    switch (Type)
                    {
                        case LIBCType.Glibc:
                            return "Glibc " + Version;
                        case LIBCType.Newlib:
                            return "Newlib " + Version;
                        default:
                            return "No Libc";
                    }
                }
            }
        }

        public struct GDBSettings
        {
            public string GDBVersion;
        }

        public struct BuildSettings
        {
            public string LocalDirectory;
            public string OutputDirectory;
            public string SiteDirectory;
            public string ExtraFilesDirectory;
        }

        public BuildSettings Build;

        public TargetSettings Target;
        public BinutilsSettings Binutils;
        public GCCSettings GCC;
        public GDBSettings GDB;

        public int ParalleliationLevel;
        public bool NoParallelGCC;

        public string JobString
        {
            get
            {
                string baseName = string.Format("bu-{0}+gcc-{1}+gmp-{2}+mpfr-{3}+mpc-{4}", Binutils.BinutilsVersion, GCC.GCCVersion, GCC.GMPVersion, GCC.MPFRVersion, GCC.MPCVersion);
                switch (Target.Libc.Type)
                {
                    case LIBCType.Glibc:
                        baseName += string.Format("+glibc-{0}", Target.Libc.Version);
                        break;
                    case LIBCType.Newlib:
                        baseName += string.Format("+newlib-{0}", Target.Libc.Version);
                        break;
                }
                return baseName;
            }
        }
    }
}
