using MediatR;
using NetMQ;

namespace PubCrawl.Notifications
{
	public class MessageReceivedNotification : INotification
	{
		public NetMQMessage Message { get; set; }
	}
}
