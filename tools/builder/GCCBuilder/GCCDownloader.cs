using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace GCCBuilder
{
    class GCCDownloader
    {
        BuildJob _Job;
        string _TargetDirectory;

        class DownloadThreadContext
        {
            public int IndexInProgressArray;
            public string URL;
        }

        long[] _ProgressArray;
        long[] _TotalSizeArray;
        int FilesRemaining;

        IBuildProgressReporter _Reporter;

        public GCCDownloader(string targetDirectory, BuildJob job, IBuildProgressReporter reporter)
        {
            _Job = job;
            _TargetDirectory = targetDirectory;
            _Reporter = reporter;
        }

        public void Start()
        {
            string tagFile = Path.Combine(_TargetDirectory, _Job.JobString) + ".download";
            if (File.Exists(tagFile))
            {
                _Reporter.ReportTextLine(string.Format("Found {0}. Skipping download phase...", tagFile));
                if (DownloadCompleted != null)
                    DownloadCompleted(this, null);
                return;
            }

            string urlGCC = string.Format("ftp://ftp.gnu.org/gnu/gcc/gcc-{0}/gcc-{0}.tar.gz", _Job.GCC.GCCVersion);
            string urlGMP = string.Format("https://ftp.gnu.org/pub/gnu/gmp/gmp-{0}.tar.bz2", _Job.GCC.GMPVersion);
            string urlMPFR = string.Format("http://www.mpfr.org/mpfr-{0}/mpfr-{0}.tar.bz2", _Job.GCC.MPFRVersion);
            string urlBinutils = string.Format("http://ftp.gnu.org/gnu/binutils/binutils-{0}.tar.bz2", _Job.Binutils.BinutilsVersion);
            string urlMPC = string.Format("http://www.multiprecision.org/mpc/download/mpc-{0}.tar.gz", _Job.GCC.MPCVersion);
            string urlGDB = string.Format("http://ftp.gnu.org/gnu/gdb/gdb-{0}.tar.gz", _Job.GDB.GDBVersion);

            string urlGlibc = null, urlNewlib = null;
            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Glibc)
                urlGlibc = string.Format("http://ftp.gnu.org/gnu/glibc/glibc-{0}.tar.bz2", _Job.Target.Libc.Version);
            if (_Job.Target.Libc.Type == BuildJob.LIBCType.Newlib)
                urlNewlib = string.Format("ftp://sourceware.org/pub/newlib/newlib-{0}.tar.gz", _Job.Target.Libc.Version);
    //            urlNewlib = string.Format("ftp://sources.redhat.com/pub/newlib/newlib-{0}.tar.gz", _Job.Target.Libc.Version);
            FilesRemaining = 6;
            if (urlGlibc != null)
                FilesRemaining++;
            if (urlNewlib != null)
                FilesRemaining++;

            _ProgressArray = new long[FilesRemaining];
            _TotalSizeArray = new long[FilesRemaining];

            int idx = 0;

            _Reporter.ReportTextLine(string.Format("Downloading {0} files...", FilesRemaining));

            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlGCC });
            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlGMP });
            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlMPFR });
            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlBinutils });
            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlMPC });
            DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlGDB });

            if (urlGlibc != null)
                DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlGlibc });
            if (urlNewlib != null)
                DoStartDownload(new DownloadThreadContext { IndexInProgressArray = idx++, URL = urlNewlib});
        }

        void OnProgressChanged(int idx, long received, long total)
        {
            _TotalSizeArray[idx] = total;
            _ProgressArray[idx] = received;

            long totalSize = _TotalSizeArray.Sum();
            long done = _ProgressArray.Sum();

            double progress = ((double)done) / totalSize;
            if (DownloadProgress != null)
                DownloadProgress(totalSize, done, progress);
        }

        void DoStartDownload(DownloadThreadContext ctx)
        {
            WebClient clt = new WebClient();
            clt.DownloadProgressChanged += (s, e) => OnProgressChanged(ctx.IndexInProgressArray, e.BytesReceived, e.TotalBytesToReceive);
            clt.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(clt_DownloadFileCompleted);
            
            int idx = ctx.URL.LastIndexOf('/');
            string localFile = ctx.URL.Substring(idx + 1);

            string fullPath = Path.Combine(_TargetDirectory, localFile);
            _Reporter.ReportTextLine(string.Format("{0} => {1}", ctx.URL, fullPath));

            clt.DownloadFileAsync(new Uri(ctx.URL), fullPath);
        }

        void clt_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (Interlocked.Decrement(ref FilesRemaining) == 0)
            {
                if (DownloadCompleted != null)
                    DownloadCompleted(this, null);

                File.WriteAllText(Path.Combine(_TargetDirectory, _Job.JobString) + ".download", DateTime.Now.ToString());
            }
        }

        public EventHandler DownloadCompleted;
        public delegate void ProgressHandler(long totalSize, long done, double progress);
        public ProgressHandler DownloadProgress;

    }
}
