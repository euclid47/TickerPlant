using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using TickerPlant.Interfaces;
using TickOff.Models;
using ServiceStack;

namespace TickerPlant
{
	class Program
	{
		private static IServiceProvider _serviceProvider;
		private static ILogger<Program> _log;
		//private static TickMessages tickMessages;

		public static Task Main(string[] args)
		{
			DotNetEnv.Env.Load();

			ConfigureServices();
			SetLogger();

			var plant = _serviceProvider.GetService<IPlant>();
			var symbolCount = DotNetEnv.Env.GetInt("symbols");
			plant.TickUpdate += Plant_TickUpdate;
			plant.Start();
			plant.AddTicks(symbolCount);

			do { } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			plant.Stop();

			_log.LogInformation("Done.");
			Thread.Sleep(2000);
			Environment.Exit(1);

			return Task.CompletedTask;
		}

		private static void Plant_TickUpdate(object sender, TickUpdateEventArgs e)
		{
			_log.LogInformation(e.Tick.ToJson());
		}

		private static void SetLogger()
		{
			var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
			_log = loggerFactory.CreateLogger<Program>();
		}

		private static void ConfigureServices()
		{
			_serviceProvider = new ServiceCollection()
				.AddLogging(configure => { configure.AddConsole(); })
				.AddScoped<IPlant, Plant>()
				.BuildServiceProvider();
		}
	}
}
