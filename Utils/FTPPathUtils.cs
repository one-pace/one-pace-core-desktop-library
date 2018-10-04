namespace OnePaceCore.Utils
{
    public static class FTPPathUtils
    {
        public static string Combine(params string[] paths)
        {
            return string.Join("/", paths).Replace("//", "/").Replace("ftp:/", "ftp://");
        }
    }
}
