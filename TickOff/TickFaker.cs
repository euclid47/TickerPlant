using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickerPlant.Interfaces;
using TickOff.Models;

namespace TickerPlant
{
	internal class TickFaker : ITickFaker
	{
		public event InternalTickHandler InternalTick;

		private readonly Random _rng;
		private readonly FakeTickQueue _fakeQueue;
		private readonly ConcurrentDictionary<string, Tick> _tickDictionary;

		public TickFaker(FakeTickQueue fakeQueue)
		{
			_rng = new Random(DateTime.UtcNow.Millisecond);
			_tickDictionary = new ConcurrentDictionary<string, Tick>();
			_fakeQueue = fakeQueue;
		}

		public void Start()
		{
			Task.Factory.StartNew(ReadAddStockSymbolsQueue, _fakeQueue.CancellationToken);
			Task.Factory.StartNew(StartTicks, _fakeQueue.CancellationToken);
		}

		public int SymbolCount()
		{
			return _tickDictionary.Count;
		}

		private void ReadAddStockSymbolsQueue()
		{
			try
			{
				foreach (var initialTick in _fakeQueue.AddStockSymbols.GetConsumingEnumerable(_fakeQueue.CancellationToken))
				{
					_tickDictionary.TryAdd(initialTick.Symbol, new Tick
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
					});

					Thread.Sleep(NextInt(1, 10));
				}
			}
			catch (Exception)
			{

			}	
		}

		public void StopSymbol(string symbol)
		{
			_tickDictionary.TryRemove(symbol, out _);
		}

		private void StartTicks()
		{
			do
			{
				if (!_tickDictionary.IsEmpty)
				{
					var tick = _tickDictionary.OrderBy(x => Guid.NewGuid()).First().Value;
					tick = GetNextTick(tick);
					InternalTick(this, new InternalTickEventArgs {Tick = tick});
					_tickDictionary.TryUpdate(tick.Symbol, tick, tick);
				}

				Thread.Sleep(NextInt(DotNetEnv.Env.GetInt("min",10), DotNetEnv.Env.GetInt("max", 100)));

			} while (!_fakeQueue.CancellationToken.IsCancellationRequested);
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
