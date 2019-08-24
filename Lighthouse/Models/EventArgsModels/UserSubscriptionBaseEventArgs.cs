using System;
using System.Collections.Generic;

namespace Lighthouse.Models.EventArgsModels
{
	public class UserSubscriptionBaseEventArgs : EventArgs
	{
		public string Id { get; set; }
		public IEnumerable<string> Symbols { get; set; }
	}
}
