namespace TickerPlant.Models
{
	public interface IStockSymbol
	{
		string ADR { get; set; }
		string Industry { get; set; }
		double LastSale { get; set; }
		double MarketCap { get; set; }
		string Name { get; set; }
		string Sector { get; set; }
		string Symbol { get; set; }
	}
}