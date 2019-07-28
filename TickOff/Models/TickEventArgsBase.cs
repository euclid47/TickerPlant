using System;

namespace TickOff.Models
{
	public class TickEventArgsBase : EventArgs
	{
		public Tick Tick { get; set; }
	}
}
