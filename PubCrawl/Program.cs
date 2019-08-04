using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PubCrawl.Services;
using System;
using System.Threading;

namespace PubCrawl
{
	class Program
	{
		private static IServiceProvider _serviceProvider;
		private static ILogger<Program> _log;

		static void Main(string[] args)
		{
			DotNetEnv.Env.Load();
			ConfigureServices();
			SetLogger();

			_log.LogInformation("Press Esc to quit.");

			var server = _serviceProvider.GetService<ITickerServer>();
			server.Start();

			do
			{
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			server.Stop();

			_log.LogInformation("Exiting in 2 seconds.");
			Thread.Sleep(2000);
			Environment.Exit(1);
		}

		private static void ConfigureServices()
		{
			_serviceProvider = new ServiceCollection()
				.AddLogging(configure => { configure.AddConsole(); })
				.AddMediatR()
				.AddScoped<ITickerServer, TickerServer>()
				.BuildServiceProvider();
		}

		private static void SetLogger()
		{
			var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
			_log = loggerFactory.CreateLogger<Program>();
		}
	}
}
