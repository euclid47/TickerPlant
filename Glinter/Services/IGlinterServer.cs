using System.Threading.Tasks;

namespace Glinter.Services
{
	public interface IGlinterServer
	{
		Task Start();
		void Stop();
	}
}