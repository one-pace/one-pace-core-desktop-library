using OnePaceCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OnePaceCore.Converters
{
    public static class ASS2SRTConverter
    {
        private const string TIME_FORMAT_ASS = "H:mm:ss.ff";
        private const string TIME_FORMAT_SRT = "HH:mm:ss,fff";

        public static void CreateSRT(FileInfo assFile)
        {
            string[] contents = GetSRTContents(assFile);
            string pathOut = $"{assFile.Directory}\\{assFile.GetFileNameWithoutExtension()}.srt";
            File.WriteAllLines(pathOut, contents);
        }
        public static string[] GetSRTContents(FileInfo assFile)
        {
            var srtLines = new List<string>();
            using (FileStream fs = new FileStream(assFile.ToString(), FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                int dialogueLines = 0;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.StartsWith("Dialogue: "))
                    {
                        string[] lineInfo = line.Substring(10).Split(',');
                        string lineNumber = (++dialogueLines).ToString();
                        string startTime = DateTime.ParseExact(lineInfo[1], TIME_FORMAT_ASS, null).ToString(TIME_FORMAT_SRT);
                        string endTime = DateTime.ParseExact(lineInfo[2], TIME_FORMAT_ASS, null).ToString(TIME_FORMAT_SRT);
                        string text = string.Empty;
                        for (int i = 9; i < lineInfo.Length; i++)
                        {
                            if (i > 9)
                            {
                                text += ",";
                            }

                            text += lineInfo[i].Replace("\\N", Environment.NewLine);
                        }

                        MatchCollection matches = Regex.Matches(text, @"\{[^\}]*\}");
                        foreach (Match match in matches)
                        {
                            text = text.Replace(match.Value, "");
                        }

                        srtLines.Add(lineNumber);
                        srtLines.Add($"{startTime} --> {endTime}");
                        srtLines.Add(text);
                        srtLines.Add("");
                    }
                }
            }
            return srtLines.ToArray();
        }
    }
}
