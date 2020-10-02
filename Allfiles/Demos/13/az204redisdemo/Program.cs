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
                    services.AddTransient<Runner>();
                }).Build();

            var runner = host.Services.GetRequiredService<Runner>();
            await runner.InitializeAsync();
            await runner.SetValueAsync();
            await runner.GetValueAsync();
            await runner.PingAsync();
        }
    }

    internal sealed class Runner : IDisposable
    {
        private readonly IConfiguration _configuration;
        private ConnectionMultiplexer _cache;
        private IDatabase _database;
        public Runner(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            var connectionString = _configuration["RedisConnection"];
            _cache = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _database = _cache.GetDatabase();
        }

        public async Task SetValueAsync()
        {
            bool setValue = await _database.StringSetAsync("test:key", "100");
            Console.WriteLine($"SET: {setValue}");
        }

        public async Task GetValueAsync()
        {
            string getValue = await _database.StringGetAsync("test:key");
            Console.WriteLine($"GET: {getValue}");
        }

        public async Task PingAsync()
        {
            var result = await _database.ExecuteAsync("ping");
            Console.WriteLine($"PING: {result.Type}: {result}");

            result = await _database.ExecuteAsync("flushdb");
            Console.WriteLine($"FLUSH: {result.Type}: {result}");
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
