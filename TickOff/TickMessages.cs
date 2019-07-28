using System.Collections.Concurrent;
using TickOff.Models;

namespace TickerPlant
{
	public class TickMessages : ITickMessages
	{
		public BlockingCollection<Tick> Ticks { get; set; } = new BlockingCollection<Tick>(new ConcurrentQueue<Tick>());
	}
}
