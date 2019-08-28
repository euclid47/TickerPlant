using System;
using TickOff.Models;

namespace TickerPlant.Interfaces
{
	public interface IPlant
	{
		event EventHandler<TickUpdateEventArgs> TickUpdate;

		void AddTicks(int fakes);
		void AddTicks(string symbols);
		void RemoveTick(string symbol);
		void Start();
		void Stop();
		bool ValidSymbol(string symbol);
	}
}