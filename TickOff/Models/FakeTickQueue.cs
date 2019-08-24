using System.Collections.Concurrent;
using System.Threading;

namespace TickOff.Models
{
	internal class FakeTickQueue
	{
		public BlockingCollection<StockSymbol> AddStockSymbols { get; set; }
		public BlockingCollection<string> RemoveStockSymbols { get; set; }
		public CancellationToken CancellationToken { get; private set; }

		public FakeTickQueue(CancellationToken cancellationToken)
		{
			AddStockSymbols = new BlockingCollection<StockSymbol>(new ConcurrentBag<StockSymbol>());
			RemoveStockSymbols = new BlockingCollection<string>(new ConcurrentBag<string>());
			CancellationToken = cancellationToken;
		}

	}
}
