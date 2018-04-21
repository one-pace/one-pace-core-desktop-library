using FileTools.NET.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Tests
{
    [TestClass]
    public class CRC32Tester
    {
        [TestMethod]
        public void TestCRC32()
        {
            byte[] contents = Encoding.UTF8.GetBytes("sdas5f4das65f4a4g94g98afd 4g9fd8 ag049fd8a 0g498a 4g9a8 g409ad8 g9a 08g49");
            string crc32 = CRC32.GetHex(contents);
            Assert.AreEqual("5440F28D", crc32);
        }
    }
}
