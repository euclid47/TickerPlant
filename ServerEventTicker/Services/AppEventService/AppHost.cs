using Funq;
using ServiceStack;
using System.Reflection;

namespace ServerEventTicker.Services.AppEventService
{
	public class AppHost : AppHostBase
	{
		public AppHost() : base("TickerPlant", typeof(ServerEventsServices).GetTypeInfo().Assembly) { }

		public override void Configure(Container container)
		{
			Plugins.Add(new ServerEventsFeature { });
			SetConfig(new HostConfig { });
		}
	}
}
