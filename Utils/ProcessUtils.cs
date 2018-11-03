using OnePaceCore.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileTools.NET.Utils
{
    public static class ProcessUtils
    {
        public static Process StartWithBatchCode(string batchCode, bool waitForExit, FileInfo input, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            return StartWithBatchCode(batchCode, waitForExit, new List<FileInfo> { input }, windowStyle);
        }

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
                string pattern = "%~[a-z]{0,}" + (i + 1) + @"(:\(.{1}=.*?\)){0,}";
                while (Regex.IsMatch(arguments, pattern))
                {
                    Match match = Regex.Match(arguments, pattern);
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
                    foreach (Match replaceMatch in Regex.Matches(match.Value, @":\(.{1}=.*?\)"))
                    {
                        string old = replaceMatch.Value.Substring(2, 1);
                        string replacement = replaceMatch.Value.Substring(4, replaceMatch.Value.Length - 5);
                        fullInput = fullInput.Replace(old, replacement);
                    }
                    arguments = arguments.Remove(match.Index, match.Length);
                    arguments = arguments.Insert(match.Index, fullInput);
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
