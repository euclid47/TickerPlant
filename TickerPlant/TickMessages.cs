using System.Collections.Concurrent;
using TickerPlant.Models;

namespace TickerPlant
{
	public class TickMessages
	{
		public BlockingCollection<Tick> Ticks { get; set; } = new BlockingCollection<Tick>(new ConcurrentQueue<Tick>());
	}
}
