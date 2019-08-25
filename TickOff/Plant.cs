using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TickOff.Models;
using TickOff.Services;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Threading;
using Newtonsoft.Json;
using TickerPlant.Interfaces;

namespace TickerPlant
{
	public class Plant : IPlant
	{
		private readonly ILogger<Plant> _log;
		private readonly ConcurrentBag<TickFaker> _fakers;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly JsonSerializer _serializer;
		private readonly FakeTickQueue _fakeQueue;
		private readonly HashSet<string> _streamingSymbols;


		public event EventHandler<TickUpdateEventArgs> TickUpdate;

		public Plant(ILogger<Plant> log)
		{
			_log = log;
			_fakers = new ConcurrentBag<TickFaker>();
			_cancellationTokenSource = new CancellationTokenSource();
			_serializer = new JsonSerializer();
			_serializer.Converters.Add(new DecimalJsonConverter());
			_fakeQueue = new FakeTickQueue(_cancellationTokenSource.Token);
			_streamingSymbols = new HashSet<string>();
		}

		public void Start()
		{
			
		}

		public void AddTicks(int fakes)
		{
			LoadFakers(GetStockSymbols().Where(x => !_streamingSymbols.Contains(x.Symbol)).Take(fakes).ToList());
		}

		public void AddTicks(string symbols)
		{
			var newSymbols = symbols.Split(",").ToList().Select(x => x.ToUpper().Trim()).Distinct().ToList();
			newSymbols = newSymbols.Where(x => !_streamingSymbols.Contains(x)).ToList();

			newSymbols.ForEach(x => _streamingSymbols.Add(x));

			var stockSymbols = GetStockSymbols();
			var newStockSymbols = stockSymbols.Where(x => newSymbols.Contains(x.Symbol)).ToList();

			LoadFakers(newStockSymbols);

			newStockSymbols = null;
			newSymbols = null;
		}

		public void RemoveTick(string symbol)
		{
			if (_streamingSymbols.Contains(symbol))
			{
				StopSymbol(symbol);
				_streamingSymbols.Remove(symbol);
			}
		}

		public void Stop()
		{
			foreach (var symbol in _streamingSymbols)
			{
				StopSymbol(symbol);
			}

			_cancellationTokenSource.Cancel();
			_fakers.Clear();
			_streamingSymbols.Clear();
		}

		private void StopSymbol(string symbol)
		{
			_fakers.AsParallel().ForAll(x => x.StopSymbol(symbol));
		}

		private void LoadFakers(List<StockSymbol> stockSymbols)
		{
			foreach (var stockSymbol in stockSymbols)
			{
				if (_fakers.Count == 0 || _fakers.Count * 4 < _streamingSymbols.Count)
				{
					var tmp = new TickFaker(_fakeQueue);
					WireEventHandlers(tmp);
					tmp.Start();
					_fakers.Add(tmp);
				}

				_fakeQueue.AddStockSymbols.Add(stockSymbol);
			}
		}

		private void WireEventHandlers(TickFaker tickFaker)
		{
			var handler = new InternalTickHandler(PlantHandler);
			tickFaker.InternalTick += handler;
		}

		public void PlantHandler(object sender, InternalTickEventArgs e)
		{
			var handler = TickUpdate;

			handler(this, new TickUpdateEventArgs { Tick = e.Tick });
		}

		private ICollection<StockSymbol> GetStockSymbols()
		{
			using (var reader = new StockSymbolReader())
			{
				return reader.StockSymbols().OrderBy(x => Guid.NewGuid()).ToList();
			}
		}
	}
}
