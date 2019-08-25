namespace TickerPlant.Interfaces
{
	internal interface ITickFaker
	{
		void Start();
		void StopSymbol(string symbol);
		int SymbolCount();
	}
}