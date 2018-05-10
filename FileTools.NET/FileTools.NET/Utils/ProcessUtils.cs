using System.Diagnostics;
using System.IO;

namespace FileTools.NET.Utils
{
    public static class ProcessUtils
    {
        public static Process Start(string fileName, string arguments, bool waitForExit, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            Process process = new Process { StartInfo = new ProcessStartInfo { FileName = fileName, Arguments = arguments, WindowStyle = windowStyle } };
            process.Start();
            if (waitForExit)
            {
                process.WaitForExit();
            }
            return process;
        }
    }
}
