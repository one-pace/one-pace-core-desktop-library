using System.Text.RegularExpressions;

namespace OnePaceCore.Utils
{
    public static class UnixPathUtils
    {
        public static string Combine(params object[] paths)
        {
            string joinedPaths = string.Join("/", paths);
            string replacedSlashes = SanitizePath(joinedPaths);
            return replacedSlashes;
        }
        public static string SanitizePath(string path)
        {
            Regex regex = new Regex(@"(?<!:)(\/{2,})");
            return regex.Replace(path, "/");
        }
    }
}
