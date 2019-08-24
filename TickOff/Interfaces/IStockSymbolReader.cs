using System.Collections.Generic;
using TickOff.Models;

namespace TickOff.Interfaces
{
	internal interface IStockSymbolReader
	{
		void Dispose();
		ICollection<StockSymbol> StockSymbols { get; }

		ICollection<StockSymbol> GetStockSymbols(bool hasHeaders);
	}
}