namespace TickerPlant.Models
{
	public class Tick : StockSymbol
	{
		public double Ask { get; set; }
		public double Bid { get; set; }
		public double LastPrice { get; set; }
		public int AskSize { get; set; }
		public int BidSize { get; set; }
		public long Volume { get; set; }
		public double High { get; set; }
		public double Low { get; set; }
		public double Open { get; set; }
		public double PercentChange { get; set; }
	}
}
