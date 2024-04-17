using System;
using MQTTnet.Server;
using MQTTService;
using System.Threading.Tasks;


namespace Subscriber
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MQTTClient client = new MQTTClient();
            try
            {
                await client.ConnectAsync();

            }
            catch
            {
                Console.WriteLine("Failed to connect to MQTT broker");
            }
            await client.SubscribeAsync("home");
        }
    }
}