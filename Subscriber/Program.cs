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
            HumiditySensor sensor = new HumiditySensor();
            PressureSensor psensor = new PressureSensor();
            TempSensor tsensor = new TempSensor();
            InfluxDB dbClient = new InfluxDB();
            
            MQTTsub client = new MQTTsub();
            await client.ConnectAsync();
            await client.Subscribe("Edison/0/humidity", dbClient);
            await client.Subscribe("Edison/0/pressure", dbClient);
            await client.Subscribe("Edison/0/temperature", dbClient);
            await client.Subscribe("Nygaard/0/humidity", dbClient);
            await client.Subscribe("Nygaard/0/pressure", dbClient);
            await client.Subscribe("Nygaard/0/temperature", dbClient);
            //await client.Subscribe("Tesla/humidity", dbClient);
            //await client.Subscribe("Tesla/pressure", dbClient);
            //await client.Subscribe("Tesla/temperature", dbClient);
            
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");
        }
    }
}