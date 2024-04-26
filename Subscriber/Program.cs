using System;
using MQTTnet.Server;
using MQTTService;
using System.Threading.Tasks;
using InfluxDB.Client;
using Subscriber;
using MQTTService;

namespace Subscriber
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /*
            MQTTsub client = new MQTTsub();
            try
            {
                await client.ConnectAsync();
            }
            catch
            {
                Console.WriteLine("Failed to connect to MQTT broker");
            }

            await client.SubscribeAsync("home");

            InfluxDB dbClient = new InfluxDB();
            await dbClient.NewInfluxDBEntry(18f, "Mikkel", "test");
            */
            MQTTsub client = new MQTTsub();
            await client.ConnectAsync();
            
            await client.Subscribe("rasmus_room/humidity");
            await client.Subscribe("rasmus_room/pressure");
            await client.Subscribe("rasmus_room/temperature");
            
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");
            // Continuously receive messages from the "home" topic
            
        }
    }
}