using Fleck;
using Glinter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using TickerPlant;
using TickerPlant.Interfaces;

namespace Glinter
{
	class Program
	{
		private static IServiceProvider _serviceProvider;
		private static ILogger<Program> _log;
		private static IGlinterServer _glinterServer;

		static void Main(string[] args)
		{
			ConfigureServices();

			try
			{
				_glinterServer = _serviceProvider.GetRequiredService<IGlinterServer>();
				_glinterServer.Start();
			}
			catch (Exception e)
			{
				_log.LogError("Error in main", e);
			}

			do
			{
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			_glinterServer.Stop();
			Console.WriteLine("Exiting in 2 seconds.");
			Thread.Sleep(2000);

		}

		private static void ConfigureServices()
		{
			_serviceProvider = new ServiceCollection()
				.AddLogging(configure => { configure.AddConsole(); })
				.AddSingleton<IPlant, Plant>()
				.AddSingleton<IGlinterServer, GlinterServer>()
				.BuildServiceProvider();

			var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
			_log = loggerFactory.CreateLogger<Program>();
		}
	}
}
