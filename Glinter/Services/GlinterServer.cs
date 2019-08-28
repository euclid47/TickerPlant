using Fleck;
using Glinter.Extensions;
using Microsoft.Extensions.Logging;
using ServiceStack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickerPlant.Interfaces;
using TickOff.Models;

namespace Glinter.Services
{
	public class GlinterServer : IGlinterServer
	{
		public readonly ILogger<GlinterServer> _logger;
		public readonly IPlant _plant;
		private CancellationToken _cancellationToken;
		private readonly WebSocketServer _server;
		private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _userConnections;
		private readonly ConcurrentDictionary<string, HashSet<Guid>> _symbolUsers;
		private readonly ConcurrentDictionary<Guid, HashSet<string>> _usersSymbols;
		private readonly ConcurrentDictionary<string, Tick> _ticks;

		public GlinterServer(ILogger<GlinterServer> logger, IPlant plant)
		{
			_logger = logger;
			_server = new WebSocketServer("ws://127.0.0.1:5000/ws");
			_userConnections = new ConcurrentDictionary<Guid, IWebSocketConnection>();
			_symbolUsers = new ConcurrentDictionary<string, HashSet<Guid>>();
			_usersSymbols = new ConcurrentDictionary<Guid, HashSet<string>>();
			_plant = plant;
			_plant.TickUpdate += _plant_TickUpdate;

			var _timer = new System.Timers.Timer {Enabled = true, Interval = 10000};
			_timer.Elapsed += _timer_Elapsed;

			var _tickTimer = new System.Timers.Timer { Enabled = true, Interval = 1000 };
			_tickTimer.Elapsed += _tickTimer_Elapsed;

			_ticks = new ConcurrentDictionary<string, Tick>();
		}

		private void _plant_TickUpdate(object sender, TickUpdateEventArgs e)
		{
			_ticks.AddOrUpdate(e.Tick.Symbol, e.Tick, (k, v) => e.Tick);
		}

		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Task.Factory.StartNew(() =>
			{
				foreach (var symbol in _symbolUsers.Where(x => !x.Value.Any()))
				{
					_plant.RemoveTick(symbol.Key);
					_symbolUsers.Remove(symbol.Key, out _);
					_ticks.TryRemove(symbol.Key, out _);

					_logger.LogInformation($"Removed Symbol: {symbol.Key}");
					Thread.Sleep(1);
				}
			}, _cancellationToken);
		}

		private void _tickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Task.Factory.StartNew(() =>
			{
				_ticks.Where(x => _symbolUsers.Keys.Contains(x.Key) && x.Value.TimeStamp >= DateTime.UtcNow.AddMilliseconds(-1000)).AsParallel().ForAll(x =>
				{
					if (_symbolUsers.TryGetValue(x.Value.Symbol, out var connectionIds) && connectionIds.Any())
					{
						var payload = x.Value.ToJson();

						connectionIds.AsParallel().ForAll(connectionId =>
						{
							if (_userConnections.TryGetValue(connectionId, out var connection))
							{
								connection.Send(payload);
							}
						});
					}
				});
			}, _cancellationToken);
		}

		public Task Start(CancellationToken token)
		{
			_cancellationToken = token;

			Task.Factory.StartNew(() =>
			{
				_server.ListenerSocket.NoDelay = true;
				_server.RestartAfterListenError = true;

				_server.Start(socket =>
				{
					socket.OnOpen = () => OnOpenHandler(socket);
					socket.OnClose = () => OnCloseHandler(socket);
					socket.OnMessage = message => OnMessageHandler(socket, message);
				});
			});

			return Task.CompletedTask;
		}

		private void OnOpenHandler(IWebSocketConnection connection)
		{
			if (!_userConnections.ContainsKey(connection.ConnectionInfo.Id))
			{
				_userConnections.TryAdd(connection.ConnectionInfo.Id, connection);
				_usersSymbols.TryAdd(connection.ConnectionInfo.Id, new HashSet<string>());

				connection.Send(connection.ConnectionInfo.Id.ToString());
			}
		}

		private void OnCloseHandler(IWebSocketConnection connection)
		{
			if (_usersSymbols.TryGetValue(connection.ConnectionInfo.Id, out var symbols))
			{
				symbols.AsParallel().ForAll(symbol => { RemoveSymbolUser(symbol, connection.ConnectionInfo.Id); });
				_userConnections.TryRemove(connection.ConnectionInfo.Id, out _);
			}
		}

		private void OnMessageHandler(IWebSocketConnection connection, string message)
		{
			if (_userConnections.ContainsKey(connection.ConnectionInfo.Id))
			{
				var splitMessage = message.Split("|");

				if (splitMessage.Count() == 2)
				{
					switch (splitMessage[0].ToUpper())
					{
						case "S":
							if (_usersSymbols.TryGetValue(connection.ConnectionInfo.Id, out var addSymbols))
							{
								HashSet<string> symbols = splitMessage[1].ToSymbolList();

								foreach (var symbol in symbols)
								{
									if (_plant.ValidSymbol(symbol))
									{
										if (!addSymbols.Contains(symbol))
										{
											addSymbols.Add(symbol);
										}

										if (!_symbolUsers.TryGetValue(symbol, out var addUsers))
											addUsers = new HashSet<Guid>();

										if (!addUsers.Contains(connection.ConnectionInfo.Id))
										{
											addUsers.Add(connection.ConnectionInfo.Id);
											_symbolUsers.AddOrUpdate(symbol, addUsers, (k, v) => addUsers);
											_plant.AddTicks(symbol);
										}
									}
									else
									{
										connection.Send(CreateMessage(400, $"{symbol} is not valid."));
									}
									
								}

								_usersSymbols.TryUpdate(connection.ConnectionInfo.Id, symbols, symbols);
							}
							
							break;
						case "U":
							if (_usersSymbols.TryGetValue(connection.ConnectionInfo.Id, out var userSymbols))
							{
								List<string> symbols = splitMessage[1].Contains("*")
									? userSymbols.ToList()
									: splitMessage[1].ToSymbolList().ToList();

								foreach (var symbol in symbols)
								{
									if (userSymbols.Contains(symbol))
									{
										userSymbols.Remove(symbol);

										connection.Send(CreateMessage(200, $"Unsubscribed from {symbol}."));
									}
									else
									{
										connection.Send(CreateMessage(400, $"Not subscribed to {symbol}."));
									}

									if (_symbolUsers.TryGetValue(symbol, out var symbolUsers))
									{
										symbolUsers.Remove(connection.ConnectionInfo.Id);
										_symbolUsers.TryUpdate(symbol, symbolUsers, symbolUsers);
									}
								}

								_usersSymbols.TryUpdate(connection.ConnectionInfo.Id, symbols.ToHashSet(), symbols.ToHashSet());
							}
							break;
						default:
							connection.Send(CreateMessage(400, "Invalid message."));
							break;
					}
				}
				else
				{
					connection.Send(CreateMessage(400, "Invalid message."));
				}
			}
		}

		private void RemoveSymbolUser(string symbol, Guid connectionId)
		{
			if (_symbolUsers.TryGetValue(symbol, out var users) && users.Contains(connectionId))
			{
				users.Remove(connectionId);
				_symbolUsers.TryUpdate(symbol, users, users);

				_logger.LogInformation($"Removed Connection Id: {connectionId.ToString()}");
				_logger.LogInformation($"Removed Symbol: {symbol}");
			}
		}

		private string CreateMessage(int messageCode, string message)
		{
			return new KeyValuePair<int, string>(messageCode, message).ToJson();
		}
	}
}
