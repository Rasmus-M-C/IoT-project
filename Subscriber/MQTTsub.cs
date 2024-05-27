using System;
using System.Collections.Generic;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Protocol;
using RaspberryPi;
using Subscriber;



namespace Subscriber
{
    public class MQTTsub
    {
        public IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }
        private int queueCapacity;
        private Dictionary<string, StatsQueue> topicStatsQueues;

        public MQTTsub(string brokerIp = "0b5f2477a4fb4160bf4cabc96ee41a39.s1.eu.hivemq.cloud", int capacity =10)
        {
            Console.WriteLine(brokerIp);
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            queueCapacity = capacity;
            topicStatsQueues = new Dictionary<string, StatsQueue>();
        }

        // Method to calculate Median of an unsorted array
        // Always even number as it has 10 elements
        private float Median(float[] a, int n)
        {
            // Precondition is that the array has an even amount of elements
            if (n % 2 != 0)
                throw new Exception("The array must have an uneven amount of elements to calculate the Median.");
            // First we sort
            // the array
            Array.Sort(a);

            return (float)(a[(10 - 1) / 2] + a[n / 2]) / 2.0f;
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

            // Initialize a StatsQueue for each topic
            if (!topicStatsQueues.ContainsKey(topic))
            {
                topicStatsQueues[topic] = new StatsQueue(queueCapacity);
            }
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

            // Initialize a StatsQueue for each topic
            if (!topicStatsQueues.ContainsKey(topic))
            {
                topicStatsQueues[topic] = new StatsQueue(queueCapacity);
            }

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
                        //Console.WriteLine($"Received float value {receivedValue} on topic: {e.ApplicationMessage.Topic}");

                        // Adding random values to simulate sensor behaviour
                        

                        var statsQueue = topicStatsQueues[topic];
                        var adjustedValue = SimulateSensorBehaviour(topic.Split('/')[0]) + receivedValue;
                        if (statsQueue.IsQueueNotFull())
                        {
                            statsQueue.AddValue(adjustedValue);
                        }
                       else if (!statsQueue.IsValueAnOutlier(adjustedValue, 3))
                        {   
                            statsQueue.AddValue(adjustedValue);

                            Console.WriteLine($"Added value {adjustedValue} to {topic}. M:{statsQueue.Mean}, V:{statsQueue.Variance}, STD:{statsQueue.StandardDeviation}, Z-score: {statsQueue.IsValueAnOutlierDouble(receivedValue)}");
                            
                            await DB.NewInfluxDBEntry(adjustedValue,
                                topic.Split('/')[0],
                                topic.Split('/')[1],
                                topic.Split('/')[2]);
                        }
                        else
                        {
                            
                            Console.WriteLine($"Z-Score is: {statsQueue.IsValueAnOutlierDouble(receivedValue)} for {topic}");
                            Console.WriteLine($"Variance is: {statsQueue.Variance}");
                            Console.WriteLine(adjustedValue);
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("Received payload is not of expected length for a float value.");
                    }
                }
            };
        }
        private float SimulateSensorBehaviour(string type)
        {
            Random random = new Random();
            float randomFloatValue = 0;
            switch (type.ToLower())
            {
                case "edison":
                    // min = 0, max = 0
                    randomFloatValue += (float)random.NextDouble() * (0f - 0f) + 0f;
                    break;
                case "nygaard":
                    // random float value between 0.0 and 0.5
                    randomFloatValue += (float)random.NextDouble() * (0.5f - 0f) + 0f;
                    break;
                case "tesla":
                    // random float value between 0.0 and 0.5
                    randomFloatValue -= (float)random.NextDouble() * (0.5f - 0f) + 0f;
                    break;
                // Add more cases for other measurement types if needed
                default:
                    // Handle unknown measurement types, or set a default threshold
                    Console.WriteLine("Unknown measurement type. Using default randomFloatValue.");
                    break;
            }

            return randomFloatValue;
        }
    }
}
