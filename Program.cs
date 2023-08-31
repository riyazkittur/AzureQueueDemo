// See https://aka.ms/new-console-template for more information
using Azure.Storage;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using AzureQueueDemo;
using System.Text.Json;

Console.WriteLine("Hello, World!");

/// Use storage Account name and Account Key
var storageAccountName = "riyazkittur";
var accountKey = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY");
var queueName = "demoqueueriyaz";

var credential = new StorageSharedKeyCredential(storageAccountName, accountKey);
var queueClient = new QueueClient(
    new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"), credential
    );

/// create queue
await queueClient.CreateIfNotExistsAsync();

/// send string
#region stringMessage
SendReceipt receipt = await queueClient.SendMessageAsync("Message one", timeToLive: TimeSpan.FromDays(1));
/// Use receipt to delete or update the message
//await queueClient.DeleteMessageAsync(receipt.MessageId, receipt.PopReceipt);
#endregion


#region Send Objects

/// send object serialized using System.Text.Json.JsonSerializer

foreach (var id in Enumerable.Range(1, 10))
{
    var addEmployee = new AddEmployeeDto(id.ToString(), "Riyaz", "Kittur");
    var message = JsonSerializer.Serialize(addEmployee);
    await queueClient.SendMessageAsync(message);
}
#endregion

#region Peek

PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10, default);

foreach (var message in peekedMessages)
{
    Console.WriteLine(message.MessageText);
}
#endregion

#region Receive Messages
QueueMessage[] messages1 = await queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(10));
Console.WriteLine("Received Message count {0}", messages1.Length);
foreach (var message in messages1)
{
    try
    {
        var dto = JsonSerializer.Deserialize<AddEmployeeDto>(message.MessageText);
        Console.WriteLine($" Receieved Dto with Id : {dto.Id}");
    }
    catch(JsonException)
    {
        Console.WriteLine("Message is not in expected format");
    }
    
}
QueueMessage[] messages2 = await queueClient.ReceiveMessagesAsync(maxMessages: 10);
Console.WriteLine("Received Message count {0} for second call", messages2.Length);
await Task.Delay(TimeSpan.FromSeconds(10));
QueueMessage[] messages3 = await queueClient.ReceiveMessagesAsync(maxMessages: 10);
Console.WriteLine("Received Message count {0} after 10 sec delay", messages3.Length);

Console.WriteLine("Check the order of messages");
foreach (var message in messages3)
{
    Console.WriteLine(message.MessageText);
}
#endregion