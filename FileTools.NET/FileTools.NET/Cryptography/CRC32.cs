using DamienG.Security.Cryptography;
using System.IO;
using System.Text;

namespace FileTools.NET.Cryptography
{
    public static class CRC32
    {
        public static string GetHex(string path)
        {
            StringBuilder hash = new StringBuilder();
            foreach (byte b in ComputeHash(path))
            {
                hash.Append(b.ToString("x2"));
            }
            return hash.ToString().ToUpper();
        }
        public static byte[] ComputeHash(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return new Crc32().ComputeHash(stream);
            }
        }
    }
}
