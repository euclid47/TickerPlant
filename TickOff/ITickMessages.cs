using System.Collections.Concurrent;
using TickOff.Models;

namespace TickerPlant
{
	public interface ITickMessages
	{
		BlockingCollection<Tick> Ticks { get; set; }
	}
}