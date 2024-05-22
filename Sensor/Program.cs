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
            client.StartPublisher(sensor, 1,$"{location}/humidity");
            client.StartPublisher(psensor,2, $"{location}/pressure");
            client.StartPublisher(tsensor,3, $"{location}/temperature");
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
            //HumiditySensor sensor3 = new HumiditySensor();
            //PressureSensor psensor3 = new PressureSensor();
            //TempSensor tsensor3 = new TempSensor();
            MQTTClient client = new MQTTClient();
            MQTTClient client2 = new MQTTClient();
            //MQTTClient client3 = new MQTTClient();
            
            await client.ConnectAsync();
            await client2.ConnectAsync();
            //await client3.ConnectAsync();
            
            client.StartPublisher(sensor, 2,"Edison/0/humidity");
            client.StartPublisher(psensor, 3,"Edison/0/pressure");
            client.StartPublisher(tsensor, 5,"Edison/0/temperature");
            client2.StartPublisher(sensor2, 3,"Nygaard/0/humidity");
            client2.StartPublisher(psensor2, 7,"Nygaard/0/pressure");
            client2.StartPublisher(tsensor2,10, "Nygaard/0/temperature");
            //client3.StartPublisher(sensor3,15, "Tesla/humidity");
            //client3.StartPublisher(psensor3, 20,"Tesla/pressure");
            //client3.StartPublisher(tsensor3, 18, "Tesla/temperature");
            
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");

        }
    }
}