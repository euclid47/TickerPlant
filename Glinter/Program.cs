using Fleck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TickerPlant;
using TickerPlant.Interfaces;

namespace Glinter
{
	class Program
	{
		private static IServiceProvider _serviceProvider;
		private static ILogger<Program> _log;

		static void Main(string[] args)
		{
			var server = new WebSocketServer("ws://0.0.0.0:8181");
			server.Start(socket =>
			{
				socket.OnOpen = () => Console.WriteLine("Open!");
				socket.OnClose = () => Console.WriteLine("Close!");
				socket.OnMessage = message => socket.Send(message);
			});
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
				.AddSingleton<IPlant, Plant>()
				.BuildServiceProvider();
		}
	}
}
