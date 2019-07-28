using System.Collections.Concurrent;
using TickOff.Models;

namespace TickerPlant.Interfaces
{
	internal interface ITickMessages
	{
		BlockingCollection<Tick> Ticks { get; set; }
	}
}