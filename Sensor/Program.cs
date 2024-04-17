using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTService;

namespace RaspberryPi
{
    public class Program
    {
        static async Task Main()
        {
            float reading = 20.5f;
            MQTTClient client = new MQTTClient();
            await client.ConnectAsync();// Subscribe to the topic
            
            await client.Subscribe("home");
            await client.PublishMessageAsync(reading, "home");

            // Keep the application running to listen for incoming messages
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        
    }
}