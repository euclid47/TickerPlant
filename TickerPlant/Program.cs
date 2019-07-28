using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO.Pipelines;
using System.Text;
using System.Buffers;

namespace TickerPlant
{
	class Program
	{
		private static IServiceProvider _serviceProvider;
		private static Socket listenSocket;
		private static ILogger<Program> _log;
		private static TickMessages tickMessages;

		public async Task Main(string[] args)
		{
			DotNetEnv.Env.Load();

			ConfigureServices();
			SetLogger();

			tickMessages = new TickMessages();

			var plant = _serviceProvider.GetService<IPlant>();
			plant.Start(tickMessages);

			listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 5500));

			_log.LogInformation("Started socket on 5500");

			listenSocket.Listen(100);

			do
			{
				var socket = await listenSocket.AcceptAsync();

			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			plant.Stop();

			_log.LogInformation("Done.");
			Thread.Sleep(2000);
			Environment.Exit(1);
		}

		private static async Task ProcessLinesAsync(Socket socket)
		{
			_log.LogInformation($"[{socket.RemoteEndPoint}]: connected");

			var pipe = new Pipe();
			Task writing = FillPipeAsync(socket, pipe.Writer);
			Task reading = ReadPipeAsync(socket, pipe.Reader);

			await Task.WhenAll(reading, writing);

			_log.LogInformation($"[{socket.RemoteEndPoint}]: disconnected");
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

		private static async Task FillPipeAsync(Socket socket, PipeWriter writer)
		{
			const int minimumBufferSize = 512;

			while (true)
			{
				try
				{
					// Request a minimum of 512 bytes from the PipeWriter
					Memory<byte> memory = writer.GetMemory(minimumBufferSize);

					int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
					if (bytesRead == 0)
					{
						break;
					}

					// Tell the PipeWriter how much was read
					writer.Advance(bytesRead);
				}
				catch
				{
					break;
				}

				// Make the data available to the PipeReader
				FlushResult result = await writer.FlushAsync();

				if (result.IsCompleted)
				{
					break;
				}
			}

			// Signal to the reader that we're done writing
			writer.Complete();
		}

		private static async Task ReadPipeAsync(Socket socket, PipeReader reader)
		{
			while (true)
			{
				ReadResult result = await reader.ReadAsync();

				ReadOnlySequence<byte> buffer = result.Buffer;
				SequencePosition? position = null;

				do
				{
					// Find the EOL
					position = buffer.PositionOf((byte)'\n');

					if (position != null)
					{
						var line = buffer.Slice(0, position.Value);
						ProcessLine(socket, line);

						// This is equivalent to position + 1
						var next = buffer.GetPosition(1, position.Value);

						// Skip what we've already processed including \n
						buffer = buffer.Slice(next);
					}
				}
				while (position != null);

				// We sliced the buffer until no more data could be processed
				// Tell the PipeReader how much we consumed and how much we left to process
				reader.AdvanceTo(buffer.Start, buffer.End);

				if (result.IsCompleted)
				{
					break;
				}
			}

			reader.Complete();
		}

		private static void ProcessLine(Socket socket, in ReadOnlySequence<byte> buffer)
		{
			
		}
	}
}
