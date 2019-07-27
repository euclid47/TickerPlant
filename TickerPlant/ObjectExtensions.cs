using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TickerPlant
{
	public static class ObjectExtensions
	{
		public static string ToApplicationPath(this string fileName)
		{
			var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
			var appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
			var appRoot = appPathMatcher.Match(exePath).Value;
			return Path.Combine(appRoot, fileName);
		}

		public static bool IsEmpty(this string val)
		{
			return string.IsNullOrWhiteSpace(val);
		}
	}
}
