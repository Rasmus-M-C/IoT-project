using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Subscriber
{
    public class InfluxDB
    {
        
        public static void InfluxDBWrite(string sensorID ,float temp, float press, float humidity)
        {
            string influxUrl = "https://eu-central-1-1.aws.cloud2.influxdata.com/api/v2/buckets";
            string token = Environment.GetEnvironmentVariable("API_TOKEN");
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