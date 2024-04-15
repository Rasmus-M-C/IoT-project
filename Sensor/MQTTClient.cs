using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTService
{
    public class MQTTClient
    {
        private IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }

        public MQTTClient(string brokerIp = "192.168.230.192")
        {
            Console.WriteLine(brokerIp);
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            Client.ApplicationMessageReceivedAsync += delegate(MqttApplicationMessageReceivedEventArgs args)
            {
                // Do some work with the message...
                Console.WriteLine($"Received message on topic: {args.ApplicationMessage.Topic}");
                Console.WriteLine($"Payload: {args.ApplicationMessage.ConvertPayloadToString()}");

                // Now respond to the broker with a reason code other than success.
                args.ReasonCode = MqttApplicationMessageReceivedReasonCode.Success;

                return Task.CompletedTask;
            };
        }

        public async Task ConnectAsync()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(BrokerIP, 1883)
                .WithCleanSession()
                .Build();

            await Client.ConnectAsync(options, CancellationToken.None);
        }

        public async Task SubscribeAsync(string topic)
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.SubscribeAsync(topicFilter);
            Console.WriteLine($"Subscribed to {topic}");
        }

        public async Task PublishMessageAsync(float payload, string topic)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload.ToString())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.PublishAsync(message);
            Console.WriteLine($"Published message to {topic}: {payload}");
        }
    }
}
