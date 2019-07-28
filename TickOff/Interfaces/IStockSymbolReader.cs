using System.Collections.Generic;
using TickOff.Models;

namespace TickOff.Interfaces
{
	internal interface IStockSymbolReader
	{
		void Dispose();
		ICollection<StockSymbol> GetStockSymbols(bool hasHeaders = true);
	}
}