namespace TickerPlant
{
	public interface IPlant
	{
		void AddTicks(int fakes);
		void AddTicks(string symbols);
		void Start();
		void Stop();
	}
}