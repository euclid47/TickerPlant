using System;
using System.Threading;
using System.Threading.Tasks;
using TickerPlant.Interfaces;
using TickOff.Models;

namespace TickerPlant
{
	internal class TickFaker : ITickFaker
	{
		public event InternalTickHandler InternalTick;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Random _rng;
		private Tick Tick;

		public TickFaker()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_rng = new Random(DateTime.UtcNow.Millisecond);	
		}

		public void Start(StockSymbol initialTick)
		{
			Tick = new Tick
			{
				Ask = initialTick.LastSale,
				AskSize = NextInt(0, 10) * 100,
				ADR = initialTick.ADR,
				Bid = initialTick.LastSale,
				BidSize = NextInt(0, 10) * 100,
				High = initialTick.LastSale,
				Industry = initialTick.Industry,
				LastPrice = initialTick.LastSale,
				Low = initialTick.LastSale,
				Name = initialTick.Name,
				Open = initialTick.LastSale,
				PercentChange = 0,
				Sector = initialTick.Sector,
				Symbol = initialTick.Symbol,
				Volume = 0,
				TimeStamp = DateTime.UtcNow
			};

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
				Tick = GetNextTick(Tick);
				InternalTick(this, new InternalTickEventArgs {Tick = Tick});
				Thread.Sleep(NextInt(DotNetEnv.Env.GetInt("min",100), DotNetEnv.Env.GetInt("max", 5000)));

			} while (!_cancellationTokenSource.IsCancellationRequested);
		}

		private Tick GetNextTick(Tick lastTick)
		{
			var result = lastTick;

			result.LastPrice = ((lastTick.Ask + lastTick.Bid) / 2) * PriceMultiplier();

			result.Ask = lastTick.Ask * PriceMultiplier();
			result.AskSize = NextInt(0, 10) * 100;
			result.Bid = lastTick.Bid * PriceMultiplier();
			result.BidSize = NextInt(0, 10) * 100;

			result.High = lastTick.High > result.LastPrice ? lastTick.High : result.LastPrice;

			result.Low = lastTick.Low < result.LastPrice ? lastTick.Low : result.LastPrice;
			result.PercentChange = (result.LastPrice - lastTick.Open) / lastTick.Open;
			result.Volume += (NextInt(lastTick.AskSize <= lastTick.BidSize ? lastTick.AskSize / 100 : lastTick.BidSize / 100, lastTick.AskSize >= lastTick.BidSize ? lastTick.AskSize / 100 : lastTick.BidSize / 100) * 100) + NextInt(1, 99);

			result.TimeStamp = DateTime.UtcNow;

			return result;
		}

		private decimal PriceMultiplier()
		{
			var next = NextDecimal(0, .1);
			var posneg = NextInt(1, 4) % 2 == 0 ? -1 : 1;

			return posneg == -1 ? 1.0M - next : 1.0M + next;
		}

		private int NextInt(int min, int max)
		{
			return _rng.Next(min, max);
		}

		private decimal NextDecimal(double min, double max)
		{
			return (decimal)(_rng.NextDouble() * (max - min) + min);
		}
	}
}
