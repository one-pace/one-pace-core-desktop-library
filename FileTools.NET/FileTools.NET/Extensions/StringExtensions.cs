namespace FileTools.NET.Extensions
{
    public static class StringExtensions
    {
        public static string EndWith(this string s, string endWith)
        {
            if (!s.EndsWith(endWith))
            {
                s += endWith;
            }
            return s;
        }
    }
}
