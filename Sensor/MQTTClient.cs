using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public MQTTClient(string brokerIp = "0b5f2477a4fb4160bf4cabc96ee41a39.s1.eu.hivemq.cloud")
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
                .WithTcpServer("0b5f2477a4fb4160bf4cabc96ee41a39.s1.eu.hivemq.cloud", 8883)
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

        // Method to start a publisher on a separate thread
        public void StartPublisher(ISensor sensor, int interval, string topic)
        {
            Task.Run(async () => await Publisher(sensor, interval, topic));
        }

        // The publisher method, now suitable for concurrent execution
        public async Task Publisher(ISensor sensor, int interval, string topic, float? prevval = null)
        {
            if (interval < 1)
                throw new Exception("Interval to broadcast messages must be greater than 0.");
            await Task.Delay(interval * 100); // Delay the first message

            while (true)
            {
                float value = sensor.Measure();
                await PublishMessageAsync(value, topic);
                await Task.Delay(interval * 1000);
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

            Client.ApplicationMessageReceivedAsync += async e =>
            {
                // Offload the processing to a separate task
                await Task.Run(() =>
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
                });
            };
        }
        static double CalculateStandardDeviation(double[] values)
        {
            if (values.Length == 0) return 0.0;
            double mean = values.Average();
            double variance = values.Select(value => (value - mean) * (value - mean)).Average();
            return Math.Sqrt(variance);
        }
    }
    
}
