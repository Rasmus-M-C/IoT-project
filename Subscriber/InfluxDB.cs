using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;

namespace Subscriber
{
    public class InfluxDB
    {
        private string host { get; set; }

        private string token { get; set; }

        private string bucket { get; set; }

        private InfluxDBClient dbClient = null;

        public InfluxDB(string bucket = "weatherdata")
        {
            host = "https://eu-central-1-1.aws.cloud2.influxdata.com";
            token = Environment.GetEnvironmentVariable("API_TOKEN");
            this.bucket = bucket;
            dbClient = new InfluxDBClient(host: host, token: token, database: this.bucket);
        }

        // Make fields for each sensor reading type: humidity, pressure, temperatur.
        public async Task NewInfluxDBEntry(float value, string location, string type)
        {
            var point = PointData.Measurement("measurement")
                .SetTag("location", location)
                .SetTag("type", type)
                .SetField("value", value)
                .SetTimestamp(DateTime.Now.AddHours(+2));
            await this.dbClient.WritePointAsync(point: point);
        }
        /*
        public static void InfluxDBWrite(string sensorID ,float temp, float press, float humidity)
        {
            const string host = "";
            string authToken = Environment.GetEnvironmentVariable("API_TOKEN");
            string bucket = "weatherdata";

            using var client = new InfluxDBClient(host, token: authToken, database: bucket);
            string org = "WeatherDay_CO";


            var point = PointData.Measurement("temperature")
                .SetTag("location", "west")
                .SetField("value", 55.15)
                .SetTimestamp(DateTime.UtcNow.AddSeconds(-10));
            await client.WritePointAsync(point: point);

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
        } */
    }
}