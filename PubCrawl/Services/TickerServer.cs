using MediatR;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using PubCrawl.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace PubCrawl.Services
{
	internal class TickerServer : ITickerServer
	{
		private readonly ILogger<TickerServer> _log;
		private readonly IMediator _mediator;
		private readonly RouterSocket _routerSocket;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public TickerServer(ILogger<TickerServer> log, IMediator mediator)
		{
			_log = log;
			_cancellationTokenSource = new CancellationTokenSource();
			_routerSocket = new RouterSocket();
		}

		public void Start()
		{
			Task.Factory.StartNew(() =>
			{
				_log.LogInformation($"Starting server on tcp://{DotNetEnv.Env.GetString("serverip")}:{DotNetEnv.Env.GetString("serverport")}");
				_routerSocket.Bind($"ws://{DotNetEnv.Env.GetString("serverip")}:{DotNetEnv.Env.GetString("serverport")}");

				using (var poller = new NetMQPoller())
				{
					poller.RunAsync();

					_log.LogInformation("Listening.");

					do
					{
						NetMQMessage message = _routerSocket.ReceiveMultipartMessage();

						if (!message.IsEmpty)
						{
							_mediator.Publish(new MessageReceivedNotification {Message = message}, _cancellationTokenSource.Token);
						}

					} while (!_cancellationTokenSource.IsCancellationRequested);
				}
			});
		}

		public void Stop()
		{
			_log.LogInformation("Sent close signal.");
			_cancellationTokenSource.Cancel();
		}
	}
}
