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
        private static async Task SetupAndStartSubscribers(MQTTsub client, string location, Subscriber.InfluxDB DB)
        {
            await client.Subscribe($"{location}/humidity", DB);
            await client.Subscribe($"{location}/pressure", DB);
            await client.Subscribe($"{location}/temperature", DB);
        }
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
            */
            HumiditySensor sensor = new HumiditySensor();
            PressureSensor psensor = new PressureSensor();
            TempSensor tsensor = new TempSensor();
            InfluxDB dbClient = new InfluxDB();
            
            //await dbClient.NewInfluxDBEntry(18f, "Mikkel", "test3");
            //await dbClient.NewInfluxDBEntry(18f, "Mikkel", "test");
            
            MQTTsub client = new MQTTsub();
            await client.ConnectAsync();
            //await SetupAndStartSubscribers(client, "Edison", dbClient);
            await client.Subscribe("Edison/humidity", dbClient);
            await client.Subscribe("Edison/pressure", dbClient);
            await client.Subscribe("Edison/temperature", dbClient);
            await client.Subscribe("Nygaard/humidity", dbClient);
            await client.Subscribe("Nygaard/pressure", dbClient);
            await client.Subscribe("Nygaard/temperature", dbClient);
            
            /*client.StartPublisher(sensor, 5, "rasmus_room/humidity");
            client.StartPublisher(psensor, 5, "rasmus_room/pressure");
            client.StartPublisher(tsensor, 5, "rasmus_room/temperature");
            */
            //await client.Subscribe("rasmus_room/humidity");
            //await client.Subscribe("rasmus_room/pressure");
            //await client.Subscribe("rasmus_room/temperature");
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");
            // Continuously receive messages from the "home" topic
            
        }
    }
}