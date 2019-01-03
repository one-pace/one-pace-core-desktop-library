using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnePaceCore.Utils
{
    public static class AssemblyUtils
    {
        public static string GetAssemblyDirectory()
        {
            string path = Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path);
            return Path.GetDirectoryName(path);
        }
        public static DateTime GetBuildDateTime(TimeZoneInfo target = null)
        {
            return GetBuildDateTime(Assembly.GetCallingAssembly());
        }
        public static DateTime GetBuildDateTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            byte[] contents = new byte[2048];
            using (var stream = new FileStream(assembly.Location, FileMode.Open, FileAccess.Read))
            {
                stream.Read(contents, 0, contents.Length);
            }

            int offset = BitConverter.ToInt32(contents, 60);
            int secondsSince1970 = BitConverter.ToInt32(contents, offset + 8);
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);
        }
        public static string GetVersion()
        {
            return Assembly.GetCallingAssembly().GetVersion();
        }
        public static string GetVersion(this Assembly assembly)
        {
            string version = assembly.GetName().Version.ToString();
            return version.Substring(0, version.Length - 2);
        }
        public static string GetTitle(this Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
            AssemblyTitleAttribute assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;
            return assemblyTitleAttribute?.Title ?? "";
        }
        public static string GetTitle()
        {
            return Assembly.GetCallingAssembly().GetTitle();
        }
    }
}
