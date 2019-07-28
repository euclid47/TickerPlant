namespace TickerPlant
{
	public interface IPlant
	{
		void AddTicks(string symbols);
		void Start();
		void Stop();
		void RemoveTick(string symbol);
	}
}