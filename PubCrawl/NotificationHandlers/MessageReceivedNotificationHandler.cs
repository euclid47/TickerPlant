using MediatR;
using Microsoft.Extensions.Logging;
using PubCrawl.Notifications;
using ServiceStack;
using System.Threading;
using System.Threading.Tasks;

namespace PubCrawl.NotificationHandlers
{
	public class MessageReceivedNotificationHandler : INotificationHandler<MessageReceivedNotification>
	{
		private readonly ILogger<MessageReceivedNotificationHandler> _log;

		public MessageReceivedNotificationHandler(ILogger<MessageReceivedNotificationHandler> log)
		{
			_log = log;
		}

		public Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
		{
			_log.LogInformation(notification.Message.ToJson());

			return Task.CompletedTask;
		}
	}
}
