using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Extensions
{
	public static class StringExtensions
	{
		public static ICollection<string> ToSymbolList(this string val)
		{
			return val.Split(",").Select(x => x.ToUpper().Trim()).Distinct().ToList();
		}
	}
}
