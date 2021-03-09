using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

const string ServiceBusConnectionString = "your_connection_string";
const string QueueName = "az204-queue";

const int numberOfMessages = 10;
IQueueClient _queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
Console.WriteLine("======================================================");
Console.WriteLine("Press ENTER key to exit after sending all the messages.");
Console.WriteLine("======================================================");
// Send messages.
await SendMessagesAsync(numberOfMessages);
Console.ReadKey();
await _queueClient.CloseAsync();

async Task SendMessagesAsync(int numberOfMessagesToSend)
{
    try
    {
        for (var i = 0; i < numberOfMessagesToSend; i++)
        {
            // Create a new message to send to the queue.
            string messageBody = $"Message {i}";
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            // Write the body of the message to the console.
            Console.WriteLine($"Sending message: {messageBody}");
            // Send the message to the queue.
            await _queueClient.SendAsync(message);
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
    }
}