using System;
using System.Threading;
using System.Threading.Tasks;
using TickOff.Models;

namespace TickerPlant
{
	public class TickFaker
	{
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly TickMessages _messages;

		private Tick Tick;

		public TickFaker(StockSymbol stockSymbol, TickMessages messages)
		{
			Tick = new Tick
			{
				Ask = stockSymbol.LastSale,
				AskSize = NextInt(0, 10) * 100,
				ADR = stockSymbol.ADR,
				Bid = stockSymbol.LastSale,
				BidSize = NextInt(0, 10) * 100,
				High = stockSymbol.LastSale,
				Industry = stockSymbol.Industry,
				LastPrice = stockSymbol.LastSale,
				LastSale = stockSymbol.LastSale,
				Low = stockSymbol.LastSale,
				Name = stockSymbol.Name,
				Open = stockSymbol.LastSale,
				PercentChange = 0,
				Sector = stockSymbol.Sector,
				Symbol = stockSymbol.Symbol,
				Volume = 0
			};
			
			_cancellationTokenSource = new CancellationTokenSource();
			_messages = messages;
		}

		public void Start()
		{
			Task.Factory.StartNew(() => { StartTicks(); });
		}

		public void Stop()
		{
			_cancellationTokenSource.Cancel();
		}

		private void StartTicks()
		{
			do
			{
				Tick = GetNextTick();
				_messages.Ticks.Add(Tick);

				Thread.Sleep(NextInt(10, 5000));
			} while (!_cancellationTokenSource.IsCancellationRequested);
		}

		private Tick GetNextTick()
		{
			var result = Tick;

			result.LastPrice = ((Tick.Ask + Tick.Bid) / 2) * PriceMultiplier();

			result.Ask = Tick.Ask * PriceMultiplier();
			result.AskSize = NextInt(0, 10) * 100;
			result.Bid = Tick.Bid * PriceMultiplier();
			result.BidSize = NextInt(0, 10) * 100;

			result.High = Tick.High > result.LastPrice ? Tick.High : result.LastPrice;

			result.Low = Tick.Low < result.LastPrice ? Tick.Low : result.LastPrice;
			result.PercentChange = result.LastPrice / Tick.Open;
			result.Volume += NextInt(Tick.AskSize <= Tick.BidSize ? Tick.AskSize / 100 : Tick.BidSize / 100, Tick.AskSize >= Tick.BidSize ? Tick.AskSize / 100 : Tick.BidSize / 100) * 100;

			return result;
		}

		private double PriceMultiplier()
		{
			var next = NextDouble(0, .1);
			var posneg = NextInt(0, 100) % 2 == 0 ? -1 : 1;
			
			return posneg == -1 ? 1.0 - next : 1.0 + next;
		}

		private int NextInt(int min, int max)
		{
			var rng = new Random();
			return rng.Next(min, max);
		}

		private double NextDouble(double min, double max)
		{
			var rng = new Random();
			return rng.NextDouble() * (max - min) + min;
		}
	}
}
