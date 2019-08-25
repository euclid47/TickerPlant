using Lighthouse.Interfaces;
using Lighthouse.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TickerPlant.Interfaces;

namespace Lighthouse.Managers
{
	public class UserManager : IDisposable, IUserManager
	{
		private readonly ConcurrentDictionary<string, UserConnection> _users;
		private readonly ConcurrentDictionary<string, List<string>> _symbolUsers;
		private readonly ConcurrentDictionary<string, HashSet<string>> _usersSymbols;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly IPlant _plant;

		public UserManager(IPlant plant)
		{
			_plant = plant;
			_plant.TickUpdate += _plant_TickUpdate;
			_users = new ConcurrentDictionary<string, UserConnection>();
			_symbolUsers = new ConcurrentDictionary<string, List<string>>();
			_usersSymbols = new ConcurrentDictionary<string, HashSet<string>>();
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public void AddSocket(string id, WebSocket webSocket)
		{
			var userConnection = new UserConnection(id, webSocket, _cancellationTokenSource.Token);

			userConnection.OnDisconnect += UserConnection_OnDisconnect;
			userConnection.OnSubscribe += UserConnection_OnSubscribe;
			userConnection.OnUnsubscribe += UserConnection_OnUnsubscribe;

			userConnection.Start();

			_users.TryAdd(id, userConnection);
		}

		private void _plant_TickUpdate(object sender, TickOff.Models.TickUpdateEventArgs e)
		{
			if (_symbolUsers.TryGetValue(e.Tick.Symbol, out var subscribers))
			{
				var message = JsonConvert.SerializeObject(e.Tick);

				subscribers.AsParallel().ForAll(subscriber =>
				{
					if (_users.TryGetValue(subscriber, out var user))
					{
						user.Send(message);
					}
				});
			}
		}

		private void UserConnection_OnUnsubscribe(object sender, Models.EventArgsModels.UnsubscribeEventArgs e)
		{
			e.Symbols.AsParallel().ForAll(symbol => { UnsubscribeSymbol(e.Id, symbol); });
		}

		private void UserConnection_OnSubscribe(object sender, Models.EventArgsModels.SubscribeEventArgs e)
		{
			e.Symbols.AsParallel().ForAll(symbol => { SubscribeSymbol(e.Id, symbol); });
		}

		private void UserConnection_OnDisconnect(object sender, Models.EventArgsModels.UserDisconnectEventArgs e)
		{
			if (_users.ContainsKey(e.Id) && _usersSymbols.TryGetValue(e.Id, out var symbols))
			{
				symbols.AsParallel().ForAll(symbol =>
				{
					UnsubscribeSymbol(e.Id, symbol);
					_users.TryRemove(e.Id, out _);
				});
			}
		}

		private void SubscribeSymbol(string id, string symbol)
		{
			if (!_symbolUsers.TryGetValue(symbol, out var symbolUsers))
				symbolUsers = new List<string>();

			if (!symbolUsers.Any(x => x == id))
				symbolUsers.Add(id);

			_symbolUsers.AddOrUpdate(symbol, symbolUsers, (k, v) => symbolUsers);

			if (!_usersSymbols.TryGetValue(id, out var userSymbols))
				userSymbols = new HashSet<string>();

			if (!userSymbols.Any(x => x == symbol))
				userSymbols.Add(symbol);

			_usersSymbols.AddOrUpdate(id, userSymbols, (k, v) => userSymbols);

			_plant.AddTicks(symbol);
		}

		private void UnsubscribeSymbol(string id, string symbol)
		{
			if (_symbolUsers.TryGetValue(symbol, out var users) && users.Contains(id))
			{
				users.Remove(id);

				if (users.Any())
				{
					_symbolUsers.TryUpdate(symbol, users, users);
				}
				else
				{
					_plant.RemoveTick(symbol);
					_symbolUsers.TryRemove(symbol, out _);
				}
			}

			if (_usersSymbols.TryGetValue(id, out var symbols) && symbols.Any(x => x == symbol))
			{
				symbols.Remove(symbol);
				_usersSymbols.TryUpdate(id, symbols, symbols);
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_cancellationTokenSource.Cancel();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~UserManager() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

	}
}
