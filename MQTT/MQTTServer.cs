// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Collections.Generic;

namespace MQTT.Server
{
    public static class Server_Simple_Samples
    {
        public static async Task Force_Disconnecting_Client()
        {
            /*
             * This sample will disconnect a client.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Let the client connect.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Now disconnect the client (if connected).
                var affectedClient = (await mqttServer.GetClientsAsync()).FirstOrDefault(c => c.Id == "MyClient");
                if (affectedClient != null)
                {
                    await affectedClient.DisconnectAsync();
                }
            }
        }

        public static async Task Publish_Message_From_Broker()
        {
            /*
             * This sample will publish a message directly at the broker.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Create a new message using the builder as usual.
                var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();

                // Now inject the new message at the broker.
                await mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = "SenderClientId"
                    });
            }
        }

        public static async Task Run_Minimal_Server()
        {
            /*
             * This sample starts a simple MQTT server which will accept any TCP connection.
             */

            var mqttFactory = new MqttFactory(); 

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
            var serverOption = new MqttServerOptionsBuilder().WithDefaultEndpoint().WithDefaultEndpointPort(1883)
                .Build();
            // The port can be changed using the following API (not used in this example).
            // new MqttServerOptionsBuilder()
            //     .WithDefaultEndpoint()
            //     .WithDefaultEndpointPort(1234)
            //     .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(serverOption))
            {
                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        public static async Task Run_Server_With_Logging()
        {
            /*
             * This sample starts a simple MQTT server and prints the logs to the output.
             *
             * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
             *
             * See sample "Run_Minimal_Server" for more details.
             */

            var mqttFactory = new MqttFactory(new ConsoleLogger());

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();
                
                // Influx object which can be used to write data to the InfluxDB

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        public static async Task Validating_Connections()
        {
            /*
             * This sample starts a simple MQTT server which will check for valid credentials and client ID.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Setup connection validation before starting the server so that there is 
                // no change to connect without valid credentials.
                mqttServer.ValidatingConnectionAsync += e =>
                {
                    if (e.ClientId != "ValidClientId")
                    {
                        e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    }

                    if (e.UserName != "ValidUser")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    if (e.Password != "SecretPassword")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                };

                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }

        static async Task<MqttServer> StartMqttServer()
        {
            var mqttFactory = new MqttFactory();

            // Due to security reasons the "default" endpoint (which is unencrypted) is not enabled by default!
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder().WithDefaultEndpoint().Build();
            var server = mqttFactory.CreateMqttServer(mqttServerOptions);
            await server.StartAsync();
            return server;
        }

        class ConsoleLogger : IMqttNetLogger
        {
            readonly object _consoleSyncRoot = new();

            public bool IsEnabled => true;

            public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters,
                Exception? exception)
            {
                var foregroundColor = ConsoleColor.White;
                switch (logLevel)
                {
                    case MqttNetLogLevel.Verbose:
                        foregroundColor = ConsoleColor.White;
                        break;

                    case MqttNetLogLevel.Info:
                        foregroundColor = ConsoleColor.Green;
                        break;

                    case MqttNetLogLevel.Warning:
                        foregroundColor = ConsoleColor.DarkYellow;
                        break;

                    case MqttNetLogLevel.Error:
                        foregroundColor = ConsoleColor.Red;
                        break;
                }

                if (parameters?.Length > 0)
                {
                    message = string.Format(message, parameters);
                }

                lock (_consoleSyncRoot)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(message);

                    if (exception != null)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }

        // Temp, pressure, Humidity
        public static async Task InfluxDBWrite(string sensorID ,float temp, float press, float humidity)
        {
            string influxUrl = "https://eu-central-1-1.aws.cloud2.influxdata.com/api/v2/buckets";
            string token = "ed80GKFRH_eNI-L0DAqm_Y8JdcBDwVcw8ZBMlnpCpoRHISt5kjitNaq1PwD6Do3JP3vMKV_4QNqOukINErS7kg==";
            string org = "WeatherDay_CO";
            string bucket = "weatherdata";

            using (var client = InfluxDBClientFactory.Create(influxUrl, token.ToCharArray()))
            {
                // Define data points
                var dataPoints = new List<PointData>
                {
                    CreatePoint("sensordata", sensorID,new Dictionary<string, object> {
                        {"temp", temp },
                        {"pressure", press},
                        {"humidity", humidity}// Assuming it is in float
                    }),
                };
                // Write the point to InfluxDB
                var writeApi = client.GetWriteApi();
                writeApi.WritePoints(dataPoints, bucket,org);
            }

            Console.WriteLine("Data written successfully to InfluxDB.");
        }
        
        static PointData CreatePoint(string measurement, string location ,Dictionary<string, object> fields)
        {
            var builder = PointData.Measurement(measurement);
            builder.Tag("location", location);
            foreach (var field in fields)
            {
                if (field.Value is string)
                {
                    builder = builder.Field(field.Key, (string)field.Value);
                }
                else if (field.Value is double)
                {
                    builder = builder.Field(field.Key, (double)field.Value);
                }
                else if (field.Value is long)
                {
                    builder = builder.Field(field.Key, (long)field.Value);
                }
                else
                {
                    throw new ArgumentException($"Unsupported type for field '{field.Key}'.");
                }
            }

            return builder.Timestamp(DateTime.UtcNow, WritePrecision.Ms);
        }
    }
}