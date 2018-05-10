using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTools.NET.Utils
{
    public static class MKVToolNixUtils
    {
        public static void Multiplex(IList<FileInfo> sourceFiles, IList<string> languages, IList<FileInfo> attachments, FileInfo chapterFile, string output)
        {
            if (sourceFiles == null || sourceFiles.Count == 0)
            {
                throw new ArgumentException("No source files specified.");
            }
            if (languages == null || languages.Count == 0)
            {
                throw new ArgumentException("No languages specified.");
            }
            if (sourceFiles.Count != languages.Count)
            {
                throw new ArgumentException("Not as many source files as languages specified.");
            }
            if (output == null)
            {
                throw new ArgumentException("No output file specified.");
            }
            if (output.EndsWith(".mkv"))
            {
                throw new ArgumentException("Output file must be an .mkv.");
            }
            if (chapterFile != null && chapterFile.Extension != ".xml")
            {
                throw new ArgumentException("Chapter file must be an .xml.");
            }
            var options = new List<string>();
            for (int i = 0; i < sourceFiles.Count; i++)
            {
                FileInfo sourceFile = sourceFiles[i];
                string language = languages[i];
                options.Add("\"" + sourceFile + "\" -a -d --language 0:" + language);
            }
            string attachFiles = string.Join(" ", attachments.Select(i => "--attach-file \"" + i + "\""));
            string chapters = chapterFile != null ? "--chapters \"" + chapterFile + "\"" : "";
            string arguments = $"-o {output} " + string.Join(" ", options) + " " + attachFiles + " " + chapters;
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
