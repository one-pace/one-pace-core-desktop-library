using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FileTools.NET.Utils
{
    public static class FileInfoUtils
    {
        private static readonly string invalidFileNameChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        private static readonly string invalidFileNameRegexPattern = $@"([{invalidFileNameChars}]*\.+$)|([{invalidFileNameChars}]+)";
        public static string MakeValidFileName(string fileName)
        {
            return Regex.Replace(fileName, invalidFileNameRegexPattern, "_");
        }
        public static FileInfo GetWhere(string arguments)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = "where";
                p.StartInfo.Arguments = arguments;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    return null;
                }
                return new FileInfo(output.Substring(0, output.IndexOf(Environment.NewLine)));
            }
            catch (Win32Exception)
            {
                return null;
            }
        }
    }
}
