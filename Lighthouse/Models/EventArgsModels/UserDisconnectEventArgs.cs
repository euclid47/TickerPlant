using System;

namespace Lighthouse.Models.EventArgsModels
{
	public class UserDisconnectEventArgs : EventArgs
	{
		public string Id { get; set; }
	}
}
