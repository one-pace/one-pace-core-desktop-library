namespace OnePaceCore.Utils
{
	public static class PathEscaper
	{
		/// <summary>
		/// Escapes a path wrapped inside an argument string. Replaces <c>\</c> with <c>\\</c> and <c>:</c> with <c>\:</c>.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string EscapeWrappedPath(string path)
		{
			return path.Replace(@"\", @"\\").Replace(":", @"\:");
		}
	}
}
