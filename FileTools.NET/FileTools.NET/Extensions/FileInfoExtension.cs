using FileTools.NET.Cryptography;
using System.IO;
using System.Text.RegularExpressions;

namespace FileTools.NET.Extensions
{
    public static class FileInfoExtension
    {
        public static string GetCRC32(this FileInfo instance)
        {
            return CRC32.GetHex(instance.ToString());
        }
        public static string GetFileNameWithoutExtension(this FileInfo instance)
        {
            return Path.GetFileNameWithoutExtension(instance.ToString());
        }
        public static void ChangeExtension(this FileInfo instance, string newExtension)
        {
            string name = instance.GetFileNameWithoutExtension();
            instance.MoveTo(instance.Directory + "\\" + name + newExtension);
        }
        public static string GetFirstDirectory(this FileInfo instance)
        {
            string path = instance.ToString();
            string[] a = path.Split('\\');
            return a[a.Length - 1];
        }
        public static void Rename(this FileInfo instance, string newName)
        {
            string directory = instance.DirectoryName;
            instance.MoveTo(directory + "\\" + newName);
        }
        public static void Rename(this FileInfo instance, string regexPattern, string newName)
        {
            string name = instance.GetFileNameWithoutExtension();
            Match match = Regex.Match(name, regexPattern);
            if (match != null)
            {
                MatchCollection groupParameters = Regex.Matches(newName, @"\$\d");
                MatchCollection crc32Parameters = Regex.Matches(newName, @"\$crc32");
                if (crc32Parameters.Count > 0)
                {
                    newName = Regex.Replace(newName, @"\$crc32", instance.GetCRC32());
                }
                foreach (Match groupParameter in groupParameters)
                {
                    int number = int.Parse(Regex.Match(groupParameter.Value, @"\d").Value);
                    string replacement = match.Groups[number + 1].Value;
                    newName = Regex.Replace(newName, $@"\${number}", replacement);
                }
                instance.Rename(newName);
            }
        }
        public static bool ValidateCRC32(this FileInfo instance, string regexPattern, int offsetStart, int offsetEnd)
        {
            Match m = Regex.Match(instance.GetFileNameWithoutExtension(), regexPattern, RegexOptions.IgnoreCase);
            if (m != null)
            {
                string crc32 = m.Value.Substring(offsetStart, m.Value.Length - offsetStart - offsetEnd);
                if (crc32 != instance.GetCRC32())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
