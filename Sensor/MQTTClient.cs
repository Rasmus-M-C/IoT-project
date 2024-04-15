using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Protocol;

namespace MQTTService
{
    public class MQTTClient
    {
        private IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }

        public MQTTClient(string brokerIp = "192.168.230.192")
        {
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
        }

        public async Task ConnectAsync()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(BrokerIP, 1883)
                .WithCleanSession()
                .Build();

            await Client.ConnectAsync(options, CancellationToken.None);
        }

        public async Task PublishMessageAsync(float payload, string topic)
        {
            if (!Client.IsConnected)
            {
                await ConnectAsync(); // Ensure connection is established
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload.ToString())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.PublishAsync(message);
        }
    }
}