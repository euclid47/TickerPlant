using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TickerPlant.Models;
using TickerPlant.Services;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TickerPlant
{
	public class Plant : IPlant
	{
		private readonly ILogger<Plant> _log;
		private readonly TickMessages _messages;
		private readonly ConcurrentDictionary<string, TickFaker> _fakers;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly JsonSerializer _serializer;

		public Plant(ILogger<Plant> log)
		{
			_log = log;
			_messages = new TickMessages();
			_fakers = new ConcurrentDictionary<string, TickFaker>();
			_cancellationTokenSource = new CancellationTokenSource();
			_serializer = new JsonSerializer();
			_serializer.Converters.Add(new DecimalJsonConverter());
		}

		public void Start()
		{
			Task.Factory.StartNew(() => { OutputTicks(); });
		}

		public void AddTicks(int fakes)
		{
			Task.Factory.StartNew(() => { LoadFakers(fakes); });
		}

		public void AddTicks(string symbols)
		{
			Task.Factory.StartNew(() => { LoadFakers(symbols); });
		}

		public void Stop()
		{
			foreach (var kvp in _fakers)
			{
				kvp.Value.Stop();
			}

			_cancellationTokenSource.Cancel();
		}

		private void OutputTicks()
		{
			foreach (var tick in _messages.Ticks.GetConsumingEnumerable())
			{
				Console.WriteLine(JsonConvert.SerializeObject(tick, new JsonConverter[] { new DecimalJsonConverter() }));
			}
		}

		private void LoadFakers(string symbols)
		{
			var symbolsList = symbols.Split(",").ToList().Select(x => x.ToUpper().Trim());

			foreach (var symbol in GetStockSymbols().Where(x => symbolsList.Contains(x.Symbol)))
			{
				var tmp = new TickFaker(symbol, _messages);
				tmp.Start();
				_fakers.AddOrUpdate(symbol.Symbol, tmp, (k, v) => tmp);
			}

			do
			{
			} while (!_cancellationTokenSource.IsCancellationRequested);
		}

		private void LoadFakers(int fakes)
		{
			foreach (var symbol in GetStockSymbols().Take(fakes))
			{
				var tmp = new TickFaker(symbol, _messages);
				tmp.Start();
				_fakers.AddOrUpdate(symbol.Symbol, tmp, (k, v) => tmp);
			}

			do
			{
			} while (!_cancellationTokenSource.IsCancellationRequested);
		}
		
		private ICollection<StockSymbol> GetStockSymbols()
		{
			using (var reader = new StockSymbolReader())
			{
				return reader.GetStockSymbols().OrderBy(x => Guid.NewGuid()).ToList();
			}
		}
	}
}
