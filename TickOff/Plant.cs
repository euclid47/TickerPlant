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
		private readonly ConcurrentDictionary<string, TickFaker> _fakers;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly JsonSerializer _serializer;

		public event EventHandler<TickUpdateEventArgs> TickUpdate;

		public Plant(ILogger<Plant> log)
		{
			_log = log;
			_fakers = new ConcurrentDictionary<string, TickFaker>();
			_cancellationTokenSource = new CancellationTokenSource();
			_serializer = new JsonSerializer();
			_serializer.Converters.Add(new DecimalJsonConverter());
		}

		public void Start()
		{
			
		}

		public void AddTicks(int fakes)
		{
			LoadFakers(fakes);
		}

		public void AddTicks(string symbols)
		{
			LoadFakers(symbols);
		}

		public void RemoveTick(string symbol)
		{
			if (_fakers.TryGetValue(symbol.ToUpper().Trim(), out var faker))
			{
				faker.Stop();
				_fakers.Remove(symbol.ToUpper().Trim(), out _);
			}
		}

		public void Stop()
		{
			foreach (var kvp in _fakers)
			{
				kvp.Value.Stop();
			}

			_fakers.Clear();

			_cancellationTokenSource.Cancel();
		}

		private void LoadFakers(string symbols)
		{
			var symbolsList = symbols.Split(",").ToList().Select(x => x.ToUpper().Trim());

			LoadFakers(GetStockSymbols().Where(x => symbolsList.Contains(x.Symbol) && !_fakers.Keys.Contains(x.Symbol)).ToList());
		}

		private void LoadFakers(int fakes)
		{
			LoadFakers(GetStockSymbols().Where(x => !_fakers.Keys.Contains(x.Symbol)).Take(fakes).ToList());
		}

		private void LoadFakers(ICollection<StockSymbol> stockSymbols)
		{
			foreach (var stockSymbol in stockSymbols)
			{
				var tmp = new TickFaker();
				WireEventHandlers(tmp);
				tmp.Start(stockSymbol);
				_fakers.AddOrUpdate(stockSymbol.Symbol, tmp, (k, v) => tmp);
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
				return reader.StockSymbols.OrderBy(x => Guid.NewGuid()).ToList();
			}
		}
	}
}
