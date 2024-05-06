using System;
using MQTTnet.Server;
using MQTTService;
using System.Threading.Tasks;
using InfluxDB.Client;
using Subscriber;
using MQTTService;
using RaspberryPi;

namespace Subscriber
{
    class Program
    {
        // Function to start a location of subscribers
        private static async Task SetupAndStartSubscribers(MQTTsub client, string location)
        {
            await client.Subscribe($"{location}/humidity");
            await client.Subscribe($"{location}/pressure");
            await client.Subscribe($"{location}/temperature");
            InfluxDB dbClient = new InfluxDB();
            await dbClient.NewInfluxDBEntry(18f, location, "test");
        }
        static async Task Main(string[] args)
        {
            
            MQTTsub client = new MQTTsub();
            await SetupAndStartSubscribers(client, "Edison");
            await client.ConnectAsync();
            
           //await client.Subscribe("rasmus_room/humidity");
            //await client.Subscribe("rasmus_room/pressure");
            //await client.Subscribe("rasmus_room/temperature");
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");
            // Continuously receive messages from the "home" topic
            
        }
    }
}