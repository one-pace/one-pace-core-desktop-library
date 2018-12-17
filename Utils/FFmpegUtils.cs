using FileTools.NET.Utils;

namespace OnePaceCore.Utils
{
    public static class FFmpegUtils
    {
        public static void Encode(string input, string output, string arguments)
        {
            string ffmpegArguments = string.Format("-i \"{0}\" {2} \"{1}\"", input, output, arguments);
            ProcessUtils.Start("ffmpeg", ffmpegArguments);
        }
    }
}
