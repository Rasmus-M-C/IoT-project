using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using RaspberryPi;

namespace MQTTService
{
    public class MQTTClient
    {
        public IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }

        public MQTTClient(string brokerIp = "834699b1c09d459582225a5b5ebf9fdc.s1.eu.hivemq.cloud")
        {
            Console.WriteLine(brokerIp);
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            
        }

        public async Task ConnectAsync()
        {
            var options = new MqttClientOptionsBuilder()
                .WithCredentials("admin", "Admin123")
                .WithTls()
                .WithTcpServer(BrokerIP, 8883)
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

        public async Task Publisher(ISensor sensor, int interval, string topic)
        {
            if (interval < 1)
                throw new Exception("Interval to broadcast messages must be greater than 0.");
            else
            {
                var value = sensor.Measure();
                while (true)
                {
                    await PublishMessageAsync(value, topic);
                    await Task.Delay(interval * 1000);
                }
            }
        }
        
        public async Task PublishMessageAsync(float payload, string topic)
        {
            byte[] payloadBytes = BitConverter.GetBytes(payload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payloadBytes)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            /* Uncomment the following block to publish messages every 2 seconds for test.
            while (true) {
                Thread.Sleep(2000);
                await Client.PublishAsync(message);
                Console.WriteLine($"Published message to {topic}: {payload}");
            }*/
            await Client.PublishAsync(message);
            Console.WriteLine($"Published message to {topic}: {payload}");
        }

        public async Task Subscribe(string topic)
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.SubscribeAsync(topicFilter);

            Console.WriteLine($"Subscribed to {topic}");
            Client.ApplicationMessageReceivedAsync += e =>
            {
                var payloadBytes = e.ApplicationMessage.PayloadSegment.ToArray();

                // Ensure the payload is exactly 4 bytes, the size of a single-precision float
                if (payloadBytes.Length == 4)
                {
                    float receivedValue = BitConverter.ToSingle(payloadBytes, 0);
                    Console.WriteLine($"Received float value {receivedValue} on topic: {e.ApplicationMessage.Topic}");
                }
                else
                {
                    // Handle other cases differently or log an error
                    Console.WriteLine("Received payload is not of expected length for a float value.");
                }
                return Task.CompletedTask;
            };
        }
    }
}
