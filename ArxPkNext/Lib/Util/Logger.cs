using System;
using System.Diagnostics;
using System.IO;

namespace Poker.Lib.Util
{
    public class Logger
    {
        string _fileName;

        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
                this._fileName = this._fileName.Replace("[Y]", DateTime.Now.Year.ToString());
                this._fileName = this._fileName.Replace("[M]", DateTime.Now.ToString("MM"));
                this._fileName = this._fileName.Replace("[D]", DateTime.Now.ToString("dd"));
            }
        }

        public string ProcName { get; set; }
        public string ProcRelease { get; set; }
        public string ProcConfiguration { get; set; }

        public Logger(string pFileName, string pProcName, string pVersion)
        {
            this.FileName = pFileName;
            this.ProcName = pProcName;
            this.ProcRelease = pVersion;

#if DEBUG
            ProcConfiguration = "Debug";
#else
            ProcConfiguration = "Release";
#endif
        }

        public void Truncate()
        {
            // Open file and truncate it
            using (FileStream fs = new FileStream(FileName, FileMode.Truncate)) { }
            WriteLine("NOTICE", "---------------------");
            WriteLine("NOTICE", "----- Truncated -----");
            WriteLine("NOTICE", "---------------------");
        }

        public void WriteLine(string pLogType, string pLogStr)
        {
            WriteLine(string.Format("{0} : {1}", pLogType, pLogStr));
        }

        public void WriteLine(string pLogStr)
        {
            StackTrace stackTrace = new StackTrace();
            string pCallingMethod = stackTrace.GetFrame(1).GetMethod().Name;

            if (pCallingMethod == "WriteLine")
            {
                pCallingMethod = stackTrace.GetFrame(2).GetMethod().Name;
            }

            string logLine = string.Format(
                "{0} - [{1} / r{2} / {3} @ {4}] - {5}{6}",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                ProcConfiguration,
                ProcRelease,
                ProcName,
                pCallingMethod,
                pLogStr,
                Environment.NewLine
            );
            File.AppendAllText(this.FileName, logLine);
        }

        public void WriteLineMulti(string pLogStr)
        {
            if (!string.IsNullOrEmpty(pLogStr))
            {
                string[] lines = pLogStr.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                foreach (string s in lines)
                {
                    if (!string.IsNullOrEmpty(s.Trim()))
                    {
                        this.WriteLine("    " + s.Trim());
                    }
                }
            }
        }
    }
}
