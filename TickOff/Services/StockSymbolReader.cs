using System;
using System.Collections.Generic;
using TickOff.Models;
using System.IO;
using System.Linq;

namespace TickOff.Services
{
	public class StockSymbolReader : IDisposable
	{
		public ICollection<StockSymbol> GetStockSymbols(bool hasHeaders = true)
		{
			var result = new HashSet<StockSymbol>();

			var filename = DotNetEnv.Env.GetString("filename", "");

			if (filename.IsEmpty())
				new Exception("filename not found in environment variables.");

			using (var sr = new StreamReader(filename.ToApplicationPath()))
			{
				var line = "";
				var lineList = new List<string>();

				if (hasHeaders)
					line = sr.ReadLine();

				while (!sr.EndOfStream)
				{
					line = sr.ReadLine();

					if (!line.IsEmpty())
					{
						var tmp = new StockSymbol();
						lineList = line.Split(new[] {'\t'}).ToList();

						for (var i = 0; i < lineList.Count; i++)
						{
							switch (i)
							{
								case 0:
									tmp.Symbol = lineList[i];
									break;
								case 1:
									tmp.Name = lineList[i];
									break;
								case 2:
									if (double.TryParse(lineList[i], out var lastSale))
										tmp.LastSale = lastSale;
									break;
								case 3:
									if(double.TryParse(lineList[i], out var marketCap))
										tmp.MarketCap = marketCap;
									break;
								case 4:
									tmp.ADR = lineList[i];
									break;
								case 5:
									tmp.Sector = lineList[i];
									break;
								case 6:
									tmp.Industry = lineList[i];
									break;
							}
						}

						if (!tmp.Symbol.IsEmpty())
							result.Add(tmp);
					}
				}
			}

			return result;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~StockSymbolReader() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
