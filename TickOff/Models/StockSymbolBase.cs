namespace TickOff.Models
{
	public abstract class StockSymbolBase
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public string ADR { get; set; }
		public string Sector { get; set; }
		public string Industry { get; set; }
	}
}
