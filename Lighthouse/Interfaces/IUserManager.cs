using System.Net.WebSockets;

namespace Lighthouse.Interfaces
{
	public interface IUserManager
	{
		void AddSocket(string id, WebSocket webSocket);
		void Dispose();
	}
}