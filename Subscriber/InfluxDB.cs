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

        // Make fields for each sensor reading type: humidity, pressure, temperature.
        public async Task NewInfluxDBEntry(float value, string location, string room, string type)
        {
            DateTime time = DateTime.Now;
            DateTime newTime = time.AddHours(2);
            var point = PointData.Measurement("wdata")
                .SetTag("location", location)
                .SetTag("room", room)
                .SetField(type, value)
                .SetTimestamp(DateTime.Now);
            await this.dbClient.WritePointAsync(point: point);
        }
    }
}