using System.Collections.Generic;
using System.Linq;

namespace Glinter.Extensions
{
	public static class StringExtensions
	{
		public static HashSet<string> ToSymbolList(this string val)
		{
			return val.Split(",").Select(x => x.ToUpper().Trim()).Distinct().ToHashSet();
		}
	}
}
