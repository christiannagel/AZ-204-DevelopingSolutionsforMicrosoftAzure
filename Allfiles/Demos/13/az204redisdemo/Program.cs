using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace az204redisdemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddUserSecrets("2827e18d-dd3c-4a92-b8e4-fed4ef9ec6c0");
                }).ConfigureServices(services =>
                {

                }).Build();

            var config = host.Services.GetRequiredService<IConfiguration>();
            var connectionString = config["RedisConnection"];
            using var cache = ConnectionMultiplexer.Connect(connectionString);
            IDatabase db = cache.GetDatabase();
            bool setValue = await db.StringSetAsync("test:key", "100");
            Console.WriteLine($"SET: {setValue}");

            string getValue = await db.StringGetAsync("test:key");
            Console.WriteLine($"GET: {getValue}");

            var result = await db.ExecuteAsync("ping");
            Console.WriteLine($"PING: {result.Type}: {result}");

            result = await db.ExecuteAsync("flushdb");
            Console.WriteLine($"PING: {result.Type}: {result}");
        }
    }
}
