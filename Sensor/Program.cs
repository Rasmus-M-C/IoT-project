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
            string reading = "20.0";
            MQTTClient client = new MQTTClient();
            await client.ConnectAsync();
            await client.PublishMessageAsync(float.Parse(reading), "home");
        }
        
    }
}