using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace TickerPlant
{
	class Program
	{
		private static IServiceProvider _serviceProvider;

		public static void Main(string[] args)
		{
			DotNetEnv.Env.Load();
			ConfigureServices();

			var plant = _serviceProvider.GetService<IPlant>();
			plant.Start();
			Task.Factory.StartNew(() => { plant.AddTicks(10); });

			do
			{
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			plant.Stop();

			Console.WriteLine("Done.");
			Thread.Sleep(2000);
			Environment.Exit(1);
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
