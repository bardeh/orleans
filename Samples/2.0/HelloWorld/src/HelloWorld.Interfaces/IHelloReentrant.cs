using Orleans.Concurrency;
using System.Threading.Tasks;

namespace HelloWorld.Interfaces
{
    public interface IHelloReentrant : Orleans.IGrainWithIntegerKey
    {        
        Task<string> SayHello(string greeting);

        Task<string> SayEcho(string greeting);
    }
}
