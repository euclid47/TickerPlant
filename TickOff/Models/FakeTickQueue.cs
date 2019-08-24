using System.Collections.Concurrent;
using System.Threading;

namespace TickOff.Models
{
	internal class FakeTickQueue
	{
		public BlockingCollection<StockSymbol> AddStockSymbols { get; set; }
		public CancellationToken CancellationToken { get; private set; }

		public FakeTickQueue(CancellationToken cancellationToken)
		{
			AddStockSymbols = new BlockingCollection<StockSymbol>(new ConcurrentQueue<StockSymbol>());
			CancellationToken = cancellationToken;
		}

	}
}
