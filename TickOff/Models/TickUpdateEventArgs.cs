using System;

namespace TickOff.Models
{
	public class TickUpdateEventArgs : EventArgs
	{
		public Tick Tick { get; set; }
	}
}
