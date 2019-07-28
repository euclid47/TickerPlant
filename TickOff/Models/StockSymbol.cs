using Newtonsoft.Json;

namespace TickOff.Models
{
	public class StockSymbol : IStockSymbol
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		[JsonIgnore]
		public double LastSale { get; set; }
		[JsonIgnore]
		public double MarketCap { get; set; }
		public string ADR { get; set; }
		public string Sector { get; set; }
		public string Industry { get; set; }
	}
}
