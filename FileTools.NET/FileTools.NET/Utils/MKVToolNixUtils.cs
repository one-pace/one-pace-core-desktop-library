using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTools.NET.Utils
{
    public static class MKVToolNixUtils
    {
        public static void Multiplex(FileInfo videoFile, FileInfo subtitleFile, IList<string> languages, IList<FileInfo> attachments, FileInfo chapterFile, string output)
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
            string attachFiles = string.Join(" ", attachments.Select(i => "--attach-file \"" + i + "\""));
            string chapters = chapterFile != null ? "--chapters \"" + chapterFile + "\"" : "";
            string arguments = $"-o \"{output}\" --language 0:" + languages[0] + " --language 1:" + languages[1] + " \"" + videoFile + "\" --language 0:" + languages[2] + " \"" + subtitleFile + "\" " + attachFiles + " " + chapters;
            try
            {
                ProcessUtils.Start("mkvmerge", arguments, true);
            }
            catch (Exception)
            {
                throw new Exception("mkvmerge not found. Make sure that it's installed and added to the system PATH.");
            }
        }
    }
}
