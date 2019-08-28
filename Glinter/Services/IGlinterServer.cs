using System.Threading;
using System.Threading.Tasks;

namespace Glinter.Services
{
	public interface IGlinterServer
	{
		Task Start(CancellationToken cancellationToken);
	}
}