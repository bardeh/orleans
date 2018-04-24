using HelloWorld.Interfaces;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Orleans.Configuration;

namespace OrleansClient
{
    /// <summary>
    /// Orleans test silo client
    /// </summary>
    public class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    client = new ClientBuilder()
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "HelloWorldApp";
                        })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHello).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();

                    await client.Connect();
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            await HelloGrain(client);
            await ArchiveGrain(client);
            await DemoLoopingConcurrencyAwait(client);
            await DemoReentrant(client);
            await DemoReentrantSuperHello(client);
        }

        private static async Task HelloGrain(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            var response = await friend.SayHello("Good morning, my friend!");
        }
        private static async Task ArchiveGrain(IClusterClient client)
        {
            var archiveFriend = client.GetGrain<IHelloArchive>(0);
            var response = await archiveFriend.SayHello("Hi, Friend!");
            response = await archiveFriend.SayHello("Hello, THere!");
            response = await archiveFriend.SayHello("Hi, for the third time!");
            var listResponse = await archiveFriend.GetGreetings();
        }
      
        private static async Task DemoLoopingConcurrencyAwait(IClusterClient client)
        {
            var friend = client.GetGrain<IHello>(0);
            var responses = new List<Task<string>>();
            for (int i = 1; i <= 500; i++)
            {
                responses.Add(friend.SayEcho($"{i}"));
            }

            var responsesFromGrain = await Task.WhenAll(responses);
        }

        private static async Task DemoReentrant(IClusterClient client)
        {
            var reentrantGrain = client.GetGrain<IHelloReentrant>(0);
            var reentrantResponses = new List<Task<string>>();
            for (int i = 1; i <= 500; i++)
            {
                reentrantResponses.Add(reentrantGrain.SayEcho($"{i}"));
            }
            
            var responsesFromReentrantGrain = await Task.WhenAll(reentrantResponses);
        }

        private static async Task DemoReentrantSuperHello(IClusterClient client)
        {
            var reentrantGrain = client.GetGrain<IHelloReentrant>(0);
            var reentrantResponses = new List<Task<string>>();
            for (int i = 1; i <= 5_000; i++)
            {
                reentrantResponses.Add(reentrantGrain.SayHello($"{i}"));
            }
            var responsesFromReentrantGrain = await Task.WhenAll(reentrantResponses);
        }
    }
}
