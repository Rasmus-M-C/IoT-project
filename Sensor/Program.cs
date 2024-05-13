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
        // Function to start a location of sensors
        private static async Task SetupAndStartPublishers(MQTTClient client, string location)
        {
            // location where sensors are physically.
            HumiditySensor sensor = new HumiditySensor();
            PressureSensor psensor = new PressureSensor();
            TempSensor tsensor = new TempSensor();
            client.StartPublisher(sensor, 5, $"{location}/humidity");
            client.StartPublisher(psensor, 5, $"{location}/pressure");
            client.StartPublisher(tsensor, 5, $"{location}/temperature");
        }
        static async Task Main()
        {
            //MQTTClient client = new MQTTClient();
            //await client.ConnectAsync();
            HumiditySensor sensor = new HumiditySensor();
            PressureSensor psensor = new PressureSensor();
            TempSensor tsensor = new TempSensor();
            HumiditySensor sensor2 = new HumiditySensor();
            PressureSensor psensor2 = new PressureSensor();
            TempSensor tsensor2 = new TempSensor();
            MQTTClient client = new MQTTClient();
            MQTTClient client2 = new MQTTClient();
            
            await client.ConnectAsync();
            
            client.StartPublisher(sensor, 2, "Edison/humidity");
            client.StartPublisher(psensor, 3, "Edison/pressure");
            client.StartPublisher(tsensor, 5, "Edison/temperature");
            client2.StartPublisher(sensor2, 3, "Nygaard/humidity");
            client2.StartPublisher(psensor2, 7, "Nygaard/pressure");
            client2.StartPublisher(tsensor2, 10, "Nygaard/temperature");
            /*
            await client.Subscribe("rasmus_room/humidity");
            await client.Subscribe("rasmus_room/pressure");
            await client.Subscribe("rasmus_room/temperature");
            */
            //await SetupAndStartPublishers(client, "Edison");
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");





        }
    }
}