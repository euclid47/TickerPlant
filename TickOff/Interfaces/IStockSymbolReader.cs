using System.Collections.Generic;
using TickOff.Models;

namespace TickOff.Interfaces
{
	internal interface IStockSymbolReader
	{
		void Dispose();
		ICollection<StockSymbol> StockSymbols();

		ICollection<StockSymbol> GetStockSymbols(bool hasHeaders);
	}
}