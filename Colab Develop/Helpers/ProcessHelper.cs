using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Colab.Develop
{
    public static class ProcessHelper
    {
        public static ProcessStartInfo CreateStartInfo(String app, String parameters)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(app, parameters);
            processStartInfo.ErrorDialog = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(app);
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            return processStartInfo;
        }

        public static List<String> Run(String app, String pars, LogRichTextBox logging = null)
        {
            ProcessStartInfo processStartInfo = ProcessHelper.CreateStartInfo(app, pars);
            LoggingProcess process = new LoggingProcess(logging);
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.waitForAppExitHandle.WaitOne();
            var output = process.OutputList;
            process.Dispose();
            return output;
        }
    }

    public class LoggingProcess : Process
    {
        public String Errors { get; set; }
        public List<String> OutputList { get; set; }
        public AutoResetEvent waitForAppExitHandle = new AutoResetEvent(false);

        private LogRichTextBox _logbox;

        public LoggingProcess(LogRichTextBox logbox)
        {
            EnableRaisingEvents = true;
            _logbox = logbox;
            OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived); 
            ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
            Exited += process_Exited;
            Errors = "";
            OutputList = new List<string>();
        }

        void process_Exited(object sender, EventArgs e)
        {
            waitForAppExitHandle.Set();
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_logbox != null)
                _logbox.LogError(e.Data);
            Errors += e.Data;
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_logbox != null)
                _logbox.LogInfo(e.Data);
            if (!String.IsNullOrEmpty(e.Data))
                OutputList.Add(e.Data.ToLower());
        }
    }
}
