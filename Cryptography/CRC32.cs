using DamienG.Security.Cryptography;
using System.IO;
using System.Text;

namespace OnePaceCore.Cryptography
{
    public static class CRC32
    {
        public static string GetHex(byte[] contents)
        {
            StringBuilder hash = new StringBuilder();
            foreach (byte b in ComputeHash(contents))
            {
                hash.Append(b.ToString("x2"));
            }
            return hash.ToString().ToUpper();
        }
        public static string GetHex(string path)
        {
            byte[] contents = GetContents(path);
            return GetHex(contents);
        }
        public static byte[] ComputeHash(string path)
        {
            byte[] contents = GetContents(path);
            return ComputeHash(contents);
        }
        public static byte[] ComputeHash(byte[] contents)
        {
            return new Crc32().ComputeHash(contents);
        }
        public static byte[] GetContents(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                byte[] contents = new byte[stream.Length];
                stream.Read(contents, 0, contents.Length);
                return contents;
            }
        }
    }
}
