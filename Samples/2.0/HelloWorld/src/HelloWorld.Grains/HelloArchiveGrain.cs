using HelloWorld.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelloWorld.Grains
{
    public class HelloArchiveGrain : Orleans.Grain<GreetingArchive>, IHelloArchive
    {
        private readonly ILogger logger;

        public HelloArchiveGrain(ILogger<HelloArchiveGrain> logger)
        {
            this.logger = logger;
        }

        public async Task<string> SayHello(string greeting)
        {
            logger.LogInformation($"SayHello message received: greeting = '{greeting}'");

            State.Greetings.Add(greeting);

            await WriteStateAsync();

            return $"You said: '{greeting}', I say: Hello!";
        }

        public Task<IEnumerable<string>> GetGreetings()
        {
            logger.LogInformation($"GetGreetings message received.");
            return Task.FromResult<IEnumerable<string>>(State.Greetings);
        }
    }

    public class GreetingArchive
    {
        public List<string> Greetings { get; } = new List<string>();
    }
}
