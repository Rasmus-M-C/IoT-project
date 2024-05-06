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
            int capacity = 10;
            if (interval < 1)
                throw new Exception("Interval to broadcast messages must be greater than 0.");
            await Task.Delay(interval * 1000); // Delay the first message
            Queue<double> prevValues = new Queue<double>(capacity);
            Random random = new Random();
            string sensorType = topic.Split('/')[1];
            /*
            if (sensorType == "temperature")
            {
                for (int i = 0; i < capacity; i++)
                {   
                    double noise = 0.5 * (2 * random.NextDouble() - 1); // Noise between -0.5 and 0.5
                    prevValues.Enqueue((double)sensor.Measure()+(float)noise);
                    Thread.Sleep(interval*500);
                }
            }
            else if (sensorType == "humidity")
            {
                for (int i = 0; i < capacity; i++)
                {
                    double noise = 1 * (2 * random.NextDouble() - 1); // Noise between -1 and 1
                    prevValues.Enqueue((double)sensor.Measure()+(float)noise);
                    Thread.Sleep(interval*500);
                }
            }
            else // pressure case
            {
                for (int i = 0; i < capacity; i++)
                {
                    double noise = 2 * (2 * random.NextDouble() - 1); // Noise between -2 and 2
                    prevValues.Enqueue((double)sensor.Measure()+(float)noise);
                    Thread.Sleep(interval*500);
                }
            }

            */
            for (int i = 0; i < capacity; i++)
            {
                prevValues.Enqueue((double)sensor.Measure());
                Thread.Sleep(interval * 500);
            }

            while (true)
            {
                double avg = prevValues.Average();
                double stdDev = CalculateStandardDeviation(prevValues.ToArray());

                var value = (double)sensor.Measure(); // Cast to double since Measure returns float
                if (value > avg + 2 * stdDev || value < avg - 2 * stdDev)
                {
                    // Anomaly detected
                    Console.WriteLine("Anomaly detected, skipping value...");
                }
                else
                {
                    // No anomaly detected, update queue and publish message
                    if (prevValues.Count >= capacity)
                        prevValues.Dequeue(); // Remove the oldest value

                    prevValues.Enqueue(value); // Add new value
                    await PublishMessageAsync((float)value, topic);
                    Console.WriteLine($"Value {value} added and message published.");
                }

                await Task.Delay((int)(interval * 1000)); // Delay for the interval in milliseconds
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
