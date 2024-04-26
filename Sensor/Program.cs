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
            PressureSensor psensor = new PressureSensor();
            TempSensor tsensor = new TempSensor();
            HumiditySensor kitchen_sensorh = new HumiditySensor();
            PressureSensor kitchen_sensorp = new PressureSensor();
            TempSensor kitchen_sensort = new TempSensor();
            MQTTClient client = new MQTTClient();
            await client.ConnectAsync();


            client.StartPublisher(sensor, 5, "rasmus_room/humidity");
            client.StartPublisher(psensor, 5, "rasmus_room/pressure");
            client.StartPublisher(tsensor, 5, "rasmus_room/temperature");
            
            await client.Subscribe("rasmus_room/humidity");
            await client.Subscribe("rasmus_room/pressure");
            await client.Subscribe("rasmus_room/temperature");
            Console.ReadKey();
            Console.WriteLine("Press any key to exit");





        }
    }
}