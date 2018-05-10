using FileTools.NET.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileTools.NET.Utils
{
    public static class ProcessUtils
    {
        public static Process StartWithBatchCode(string batchCode, bool waitForExit, IList<FileInfo> inputs, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            if (string.IsNullOrWhiteSpace(batchCode))
            {
                return null;
            }
            string arguments = "";
            string[] spaces = batchCode.Split(' ');
            if (spaces.Length > 1)
            {
                arguments = string.Join(" ", spaces.Skip(1));
            }
            string fileName = spaces[0];
            for (int i = 0; i < inputs.Count; i++)
            {
                FileInfo input = inputs[i];
                foreach (Match match in Regex.Matches(arguments, @"%~[a-z]{0,}\d"))
                {
                    string fullInput = "";
                    var controlLetterMatches = Regex.Matches(match.Value, @"[a-z]");
                    if (controlLetterMatches.Count == 0)
                    {
                        fullInput += input.ToString();
                    }
                    else
                    {
                        foreach (Match controlLetters in controlLetterMatches)
                        {
                            switch (controlLetters.Value)
                            {
                                case "f": fullInput += input.ToString(); break;
                                case "d": fullInput += input.GetDriveRoot(); break;
                                case "p": fullInput += input.GetPathOnly(); break;
                                case "n": fullInput += input.GetFileNameWithoutExtension(); break;
                                case "x": fullInput += input.Extension; break;
                                case "s": fullInput += input.ToString(); break;
                                case "a": fullInput += input.Attributes.ToString(); break;
                                case "t": fullInput += input.LastWriteTimeUtc.ToLongDateString(); break;
                                case "z": fullInput += input.Length.ToString(); break;
                                default: break;
                            }
                        }
                    }
                    arguments = arguments.Replace(match.Value, fullInput);
                }
            }
            return Start(fileName, arguments, waitForExit, windowStyle);
        }
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
