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
            
            HumiditySensor sensor = new HumiditySensor();
            MQTTClient client = new MQTTClient();
            await client.ConnectAsync();
            
            await client.Publisher(sensor, 5, "home");
            
        }
        
    }
}