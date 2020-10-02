using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace QueueClientSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddUserSecrets("9affe013-51e7-4439-a41b-4639010bc61b");
                })
                .ConfigureServices(services =>
                {

                }).Build();
            var config = host.Services.GetRequiredService<IConfiguration>();
            var connectionString = config["QueueConnection"];
            var queueName = config["QueueName"];

            // create queue client
            var client = new QueueClient(connectionString, queueName);

            // Create the queue if it doesn't already exist
            var response = await client.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.

            var sendReceipt = await client.SendMessageAsync("a message");
            var sendReceipt2 = await client.SendMessageAsync("a second message");

            // Peek at the next message
            var peekedMessageResponse = await client.PeekMessagesAsync();
            var peekedMessages = peekedMessageResponse.Value;

            // Fetch the queue attributes.
            var queueProperties = await client.GetPropertiesAsync();

            // Retrieve the cached approximate message count.
            Console.WriteLine(queueProperties.Value.ApproximateMessagesCount);

            // Get the next message
            var messageResponse = await client.ReceiveMessagesAsync(maxMessages: 1);

            //Process the message in less than 30 seconds, and then delete the message
            var message = messageResponse.Value[0];
            Console.WriteLine(message.MessageText);
            response = await client.DeleteMessageAsync(message.MessageId, message.PopReceipt);

            // Get the message from the queue and update the message contents.
            var messageResponse2 = await client.ReceiveMessagesAsync(maxMessages: 1);
            var message2 = messageResponse2.Value[0];
            await client.UpdateMessageAsync(message2.MessageId, message2.PopReceipt, "updated message text",
                TimeSpan.FromSeconds(60));
        }
    }
}
