using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;


namespace RaspberryPi
{
    public class Program
    {
        static async Task Main()
        {
            string reading = "20.0";
            var mqttFactory= new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.50.93", 1883)
                .WithCleanSession()
                .Build();

            await client.ConnectAsync(options, CancellationToken.None);
            await PublishMessageAsync(client);
            

        }

        private static async Task PublishMessageAsync(IMqttClient client)
        {
            string payload = "Hello!";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("home")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            if (client.IsConnected)
            {
                await client.PublishAsync(message);
            }
            
        }
        
    }
}