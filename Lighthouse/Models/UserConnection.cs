using Lighthouse.Extensions;
using Lighthouse.Models.EventArgsModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Models
{
	public class UserConnection
	{
		public event EventHandler<UserDisconnectEventArgs> OnDisconnect;
		public event EventHandler<SubscribeEventArgs> OnSubscribe;
		public event EventHandler<UnsubscribeEventArgs> OnUnsubscribe;

		private readonly string _id;
		private readonly WebSocket _webSocket;
		private readonly CancellationToken _cancellationToken;
		private BlockingCollection<string> _inboundMessages;

		public UserConnection(string id, WebSocket webSocket, CancellationToken cancellationToken)
		{
			_id = id;
			_webSocket = webSocket;
			_cancellationToken = cancellationToken;
			_inboundMessages = new BlockingCollection<string>(new ConcurrentQueue<string>());
		}

		public Task Start()
		{
			Send("Connected");
			Task.Factory.StartNew(ParseMessage, _cancellationToken);
			Task.Factory.StartNew(ReadSocket, _cancellationToken);

			return Task.CompletedTask;
		}

		public void Send(string message)
		{
			SendMessage(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message)));
		}

		private async Task ReadSocket()
		{
			var buffer = new ArraySegment<byte>(new byte[1024 * 4]);

			WebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _cancellationToken);

			if (result.Count != 0 || result.CloseStatus == WebSocketCloseStatus.Empty)
			{
				_inboundMessages.Add(Encoding.ASCII.GetString(buffer.Array, buffer.Offset, result.Count));
				await ReadSocket();
			}
		}

		private void ParseMessage()
		{
			foreach (var inboundMessage in _inboundMessages.GetConsumingEnumerable(_cancellationToken))
			{
				var message = string.Join("", inboundMessage);
				var splitMessage = message.Split("|");

				if (splitMessage.Count() == 2)
				{
					switch (splitMessage[0].ToUpper())
					{
						case "S":
							SendSubscribe(splitMessage[1].ToSymbolList());
							break;
						case "U":
							SendUnsubscribe(splitMessage[1].ToSymbolList());
							break;
						default:
							//Send bad request
							break;
					}
				}
				else
				{
					// Send bad request
				}
			}
		}

		private async void SendMessage(ArraySegment<byte> message)
		{
			if (!_webSocket.CloseStatus.HasValue)
			{
				await _webSocket.SendAsync(new ArraySegment<byte>(message.Array, 0, message.Array.Length), WebSocketMessageType.Text, true, _cancellationToken);
			}
		}

		private void SendDisconnect()
		{
			var handler = OnDisconnect;
			handler?.Invoke(this, new UserDisconnectEventArgs {Id = _id});
		}

		private void SendSubscribe(IEnumerable<string> symbols)
		{
			var handler = OnSubscribe;
			handler?.Invoke(this, new SubscribeEventArgs {Id = _id, Symbols = symbols});
		}

		private void SendUnsubscribe(IEnumerable<string> symbols)
		{
			var handler = OnUnsubscribe;
			handler?.Invoke(this, new UnsubscribeEventArgs { Id = _id, Symbols = symbols });
		}
	}
}
