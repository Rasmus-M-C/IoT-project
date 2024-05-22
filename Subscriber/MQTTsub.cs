using System;
using System.Collections.Generic;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using RaspberryPi;

using InfluxDB3.Client;
using Subscriber;

namespace MQTTService
{
    public class MQTTsub
    {
        public IMqttClient Client { get; set; }
        private string BrokerIP { get; set; }
        public StatsQueue StatsQueue { get; set; }
        public MQTTsub(string brokerIp = "0b5f2477a4fb4160bf4cabc96ee41a39.s1.eu.hivemq.cloud", int capacity = 10)
        {
            Console.WriteLine(brokerIp);
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            StatsQueue = new StatsQueue(capacity);
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
        
        private float Mean(float[] a, int n)
        {
            float sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += a[i];
            }
            return (float)(sum / n);
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
        
        
        // Setting the 10 first elements coming from the sensor as the initial window
        private float InitialMedian(float[] window)
        {
            float Median = 0;
            for (int i = 0; i < 10; i++)
            {
                window[i] = i;
            }
            Median = this.Median(window, 10);
            return Median;
        }
        
        public async Task Subscribe(string topic, Subscriber.InfluxDB DB)
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await Client.SubscribeAsync(topicFilter);
            Console.WriteLine($"Subscribed to {topic}");

            int counter = 0;
            float[] window = new float[10];
            
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
                        // Adding random values to simulate sensor behaviour
                        receivedValue += SimulateSensorBehaviour(topic.Split('/')[0]);
                        //Console.WriteLine("Received value after adding random value: " + receivedValue);
                        
                        if
                        {
                            Console.WriteLine(receivedValue);
                            await DB.NewInfluxDBEntry(receivedValue, 
                                topic.Split('/')[0], 
                                topic.Split('/')[1], topic.Split('/')[2]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Received payload is not of expected length for a float value.");
                    }
                }
            };
        }
        
        // Initialize the moving window with the first 10 values or upload to InfluxDB if it is not an outlier

        private bool OutlierDetection(Func<float[], int, float> func,float value, float[] window, string type, int counter)
        {
            // Define threshold for outlier detection based on the measurement type
            float threshold = 2f; // Default threshold
            switch (type.ToLower())
            {
                case "temperature":
                    threshold = 2f; // Set threshold for temperature
                    break;
                case "humidity":
                    threshold = 5f; // Set threshold for humidity
                    break;
                case "pressure":
                    threshold = 500f; // Set threshold for pressure
                    break;
                // Add more cases for other measurement types if needed
                default:
                    // Handle unknown measurement types, or set a default threshold
                    Console.WriteLine("Unknown measurement type. Using default threshold.");
                    break;
            }
            float medianOrmean = 0;
            // If counter is equal to 10
            if (counter == 10)
            {
                medianOrmean = this.Median(window, 10);
            }
            else
            {
                // Calculate the Median or Mean of the window
                medianOrmean = func(window, 10);
            }
            // Check if the value is an outlier
            if (medianOrmean - threshold < value && value < medianOrmean + threshold)
            {
                // Update the moving window with the new value by removing the oldest element and adding the element
                for (int i = 0; i < 9; i++)
                {
                    window[i] = window[i + 1];
                }

                window[9] = value;
                return false;
            }

            // If the value is an outlier, return true
            return true;
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
