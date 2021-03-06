﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnePaceCore.Utils
{
    public static class MKVToolNixUtils
    {
        public static void Multiplex(FileInfo videoFile, FileInfo audioFile, FileInfo subtitleFile, IList<string> languages, IList<FileInfo> attachments, FileInfo chapterFile, string[] subtitleAppendices, string output)
        {
            languages = languages.Select(i => i.ToLower()).ToList();
            string videoExtension = (videoFile?.Extension ?? "").ToLower();
            if (videoExtension != ".mp4" && videoExtension != ".mkv" && videoExtension != ".m4v" && videoExtension != ".wmv")
            {
                throw new ArgumentException("No video file specified.");
            }
            if (languages == null || languages.Count == 0)
            {
                throw new ArgumentException("No languages specified.");
            }
            if (languages.Count != 3)
            {
                throw new ArgumentException("Exactly three languages must be specified.");
            }
            if (output == null)
            {
                throw new ArgumentException("No output file specified.");
            }
            if (!output.EndsWith(".mkv"))
            {
                throw new ArgumentException("Output file must be an .mkv.");
            }
            if (chapterFile != null && chapterFile.Extension != ".xml")
            {
                throw new ArgumentException("Chapter file must be an .xml.");
            }
            string arguments = $"--output {EscapePath(output)}";
            arguments += $" --language 0:{languages[0]} --language 1:{languages[1]} {EscapePath(videoFile)}";
            if (audioFile != null)
            {
                arguments += $" --language 0:{languages[0]} {EscapePath(audioFile)}";
            }
            arguments += $" --language 0:{languages[2]} ( {EscapePath(subtitleFile)}";
            if (subtitleAppendices != null && subtitleAppendices.Length > 0)
            {
                arguments += " " + string.Join(" ", subtitleAppendices.Select(i => EscapePath(i)));
            }
            arguments += " )";
            if (chapterFile != null)
            {
                arguments += $" --chapter-language und --chapters {EscapePath(chapterFile)}";
            }
            if (attachments?.Count > 0)
            {
                arguments += " " + string.Join(" ", attachments.Select(i => "--attach-file " + EscapePath(i)));
            }

            ProcessUtils.Start("mkvmerge", arguments);
        }

        private static string EscapePath(FileInfo path)
        {
            return EscapePath(path.FullName);
        }

        private static string EscapePath(string path)
        {
            return $"\"{path}\"";
        }
    }
}
