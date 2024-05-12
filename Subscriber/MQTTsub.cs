using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using RaspberryPi;
using InfluxDB3.Client;
namespace MQTTService
{
    public class MQTTsub
    {
        public IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }

        public MQTTsub(string brokerIp = "0b5f2477a4fb4160bf4cabc96ee41a39.s1.eu.hivemq.cloud")
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
        public async Task Publisher(ISensor sensor, int interval, string topic)
        {
            if (interval < 1)
                throw new Exception("Interval to broadcast messages must be greater than 0.");
            await Task.Delay(interval * 1000); // Delay the first message

            while (true)
            {
                var value = sensor.Measure(); // Move measurement inside the loop
                await PublishMessageAsync(value, topic);
                await Task.Delay(interval * 5000);
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

        public async Task Subscribe(string topic, Subscriber.InfluxDB DB)
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.SubscribeAsync(topicFilter);
            Console.WriteLine($"Subscribed to {topic}");

            Client.ApplicationMessageReceivedAsync += async e =>
            {
                // Check if the received message's topic matches the subscribed topic
                if (e.ApplicationMessage.Topic == topic)
                {
                    var payloadBytes = e.ApplicationMessage.PayloadSegment.ToArray();

                    // Assuming payload is expected to be a single-precision float and exactly 4 bytes long
                    if (payloadBytes.Length == 4)
                    {
                        float receivedValue = BitConverter.ToSingle(payloadBytes, 0);
                        Console.WriteLine($"Received float value {receivedValue} on topic: {e.ApplicationMessage.Topic}");

                        // Call to a method to handle data insertion to InfluxDB
                        await DB.NewInfluxDBEntry(receivedValue, 
                                            topic.Split('/')[0], 
                                            topic.Split('/')[1]);
                    }
                    else
                    {
                        Console.WriteLine("Received payload is not of expected length for a float value.");
                    }
                }
            };
        }
    }
}
