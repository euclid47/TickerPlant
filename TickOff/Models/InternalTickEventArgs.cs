namespace TickOff.Models
{
	public delegate void InternalTickHandler(object sender, InternalTickEventArgs e);

	public class InternalTickEventArgs : TickEventArgsBase
	{
	}
}
