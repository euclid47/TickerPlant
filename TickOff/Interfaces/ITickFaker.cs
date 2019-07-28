using TickOff.Models;

namespace TickerPlant.Interfaces
{
	internal interface ITickFaker
	{
		void Start(StockSymbol initialTick);
		void Stop();
	}
}