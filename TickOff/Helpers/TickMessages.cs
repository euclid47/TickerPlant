using System.Collections.Concurrent;
using TickerPlant.Interfaces;
using TickOff.Models;

namespace TickerPlant
{
	internal class TickMessages : ITickMessages
	{
		public BlockingCollection<Tick> Ticks { get; set; } = new BlockingCollection<Tick>(new ConcurrentQueue<Tick>());
	}
}
