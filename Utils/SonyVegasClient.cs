using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using OnePaceCore.Converters;
using OnePaceCore.Extensions;
using OnePaceCore.Enums;

namespace OnePaceCore.Utils
{
    public class SonyVegasClient
    {
        public delegate void Ping(string message, bool error = false);
        private static string VegCreatorDirectory
        {
            get
            {
                return Path.GetTempPath() + "VegCreator";
            }
        }
        public SonyVegasVersion Version { get; set; }
        private string _vegas;
        public SonyVegasClient(SonyVegasVersion version)
        {
            Version = version;
            _vegas = version.GetProcessName();
        }
        {
        public void StartVegas(string arguments, bool waitForExit, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            StartProcess(_vegas, arguments, waitForExit, windowStyle);
        }

        public void StartProcess(string fileName, string arguments, bool waitForExit, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal)
        {
            Process process = new Process { StartInfo = new ProcessStartInfo { FileName = fileName, Arguments = arguments, WindowStyle = windowStyle } };
            if (waitForExit)
            {
                process.WaitForExit();
            }
            process.Start();
        }

        public void Render(string file, string renderer, string template, bool waitForExit = false)
        {
            string script = AssemblyUtils.GetAssemblyDirectory() + "\\Render" + Version + ".cs";
            StartVegas($"\"{file}\" -script \"{script}?renderer={renderer}&template={template}\"", waitForExit);
        }
        /// <summary>
        /// Takes a raw (mp4 or veg file) and mkv file and imports the subtitles from the mkv as regions into the veg file. Requires mkvextract, python, prass, and the RegionsImporter script.
        /// </summary>
        /// <param name="raw">The .veg or .mp4 file. If it's an .mp4 it will create a .veg and place the .mp4 inside</param>
        /// <param name="attach">The .mkv/.ass/.srt file to extract subtitles from</param>
        /// <param name="shiftMS">How much to shift the subtitles in milliseconds when importing the regions</param>
        /// <param name="subsTrackIndex">The track index of the subtitle file in the mkv</param>
        /// <param name="ping">Pings back a message for every step in the process and an error boolean indicating if the process has failed.</param>
        public void ImportRegions(FileInfo raw, FileInfo attach, int shiftMS, int subsTrackIndex, Ping ping = null)
        {
            raw = new FileInfo(raw.FullName);
            attach = new FileInfo(attach.FullName);
            FileInfo regionsimporter = new FileInfo(AssemblyUtils.GetAssemblyDirectory() + "\\RegionsImporter" + (int)Version + ".cs");
            Directory.CreateDirectory(VegCreatorDirectory);
            if (attach.Extension == ".mkv")
            {
                Write("Extracting .ass file...", ping);
                StartProcess("mkvextract", "tracks \"" + attach + "\" " + subsTrackIndex + ":\"" + VegCreatorDirectory + "\\0.ass\"", true, ProcessWindowStyle.Hidden);
                Write("Done.", ping);
                attach = new FileInfo(VegCreatorDirectory + "\\0.ass");
            }
            else if (attach.Extension == ".srt")
            {
                Write($"Converting to .ass...", ping);
                StartProcess("python", "-m prass convert-srt \"" + attach + "\" -o \"" + VegCreatorDirectory + "\\0.ass\"", true, ProcessWindowStyle.Hidden);
                Write("Done.", ping);
                attach = new FileInfo(VegCreatorDirectory + "\\0.ass");
            }
            else if (attach.Extension == ".ass")
            {
                attach.CopyTo(VegCreatorDirectory + "\\0.ass", true);
                attach = new FileInfo(VegCreatorDirectory + "\\0.ass");
            }
            else
            {
                throw new ArgumentException("Invalid attach extension");
            }
            Write($"Shifting time {shiftMS}ms...", ping);
            StartProcess("python", $"-m prass shift --by {shiftMS}ms \"{attach}\" -o \"{attach}\"", true, ProcessWindowStyle.Hidden);
            Write("Done.", ping);
            Write("Converting .ass file to .srt file...", ping);
            ASS2SRTConverter.CreateSRT(attach);
            Write("Done.", ping);
            FileInfo srt = new FileInfo($"{VegCreatorDirectory}\\0.srt");
            if (raw.Extension == ".mp4")
            {
                string mp4name = string.Empty;
                Match m = null;
                if (IsMatch(raw.GetFileNameWithoutExtension(), " One Piece - [0-9]+ ", out m))
                {
                    mp4name = m.Value.Substring(13, m.Value.Length - 13 - 1);
                }
                else if (IsMatch(raw.GetFileNameWithoutExtension(), "^[0-9]+ \\[", out m))
                {
                    mp4name = m.Value.Substring(0, m.Value.Length - 2);
                }
                else if (IsMatch(raw.GetFileNameWithoutExtension(), "^[0-9]+$", out m))
                {
                    mp4name = m.Value;
                }
                else
                {
                    mp4name = "raw";
                }
                string vegpath = $"{raw.Directory}\\{mp4name}.veg";
                Write("Importing regions...", ping);
                StartVegas($"-script \"{regionsimporter}?savewhendone=true&closeonfinish=true&file={srt}&makeveg=true&media={raw}&output={vegpath}\"", true, ProcessWindowStyle.Hidden);
                Write("Done.", ping);
                raw = new FileInfo(vegpath);
            }
            else
            {
                Write("Importing regions...", ping);
                StartVegas($"\"{raw}\" -script \"{regionsimporter}?savewhendone=true&closeonfinish=true&file={srt}\"", true);
                Write("Done.", ping);
            }
            Directory.Delete($@"{VegCreatorDirectory}", true);
        }

        public void ExportRegions(string vegPath)
        {
            if (!File.Exists(vegPath))
            {
                throw new FileNotFoundException($"Could not find a part of the path '{vegPath}'.");
            }
            FileInfo regionsExporter = new FileInfo(AssemblyUtils.GetAssemblyDirectory() + "\\RegionsExporter" + (int)Version + ".cs");
            StartVegas($"\"{vegPath}\" -script \"{regionsExporter}\"", true, ProcessWindowStyle.Hidden);
        }

        public static void Write(string message, Ping ping, bool error = false)
        {
            Console.WriteLine(message);
            ping?.Invoke(message, error);
        }
        public static bool FindFiles(string directoryPath, out FileInfo raw, out FileInfo yibis)
        {
            raw = null;
            yibis = null;
            bool foundr = false;
            bool foundy = false;
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (foundr && foundy)
                {
                    break;
                }
                FileInfo f = new FileInfo(filePath);
                if (f.Extension == ".mp4" || f.Extension == ".veg")
                {
                    raw = f;
                    foundr = true;
                }
                else if (f.Extension == ".mkv")
                {
                    yibis = f;
                    foundy = true;
                }
            }
            return foundr && foundy;
        }
        public static bool IsMatch(string input, string pattern, out Match m)
        {
            m = Regex.Match(input, pattern);
            if (Regex.IsMatch(input, pattern))
            {
                m = Regex.Match(input, pattern);
                return true;
            }
            else
            {
                m = null;
                return false;
            }
        }
    }
}
