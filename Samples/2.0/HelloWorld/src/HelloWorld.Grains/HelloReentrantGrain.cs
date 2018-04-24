using HelloWorld.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using System;

namespace HelloWorld.Grains
{
    [StatelessWorker]
    [Reentrant]
    public class HelloReentrantGrain : Orleans.Grain, IHelloReentrant
    {
        private readonly ILogger logger;

        public HelloReentrantGrain(ILogger<HelloGrain> logger)
        {
            this.logger = logger;
        }

        Task<string> IHelloReentrant.SayHello(string greeting)
        {
            logger.LogInformation($"SayHello message received: greeting = '{greeting}'");
            return Task.FromResult($"{greeting}");
        }

        Task<string> IHelloReentrant.SayEcho(string greeting)
        {
            logger.LogInformation($"SayEcho message received: greeting = '{greeting}'");
            Random rand1 = new Random();
            int delay = (int)(50 * rand1.NextDouble());
            Task.Delay(delay).Wait();
            return Task.FromResult($"{greeting}");
        }
    }
}
