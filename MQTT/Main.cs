using System;
using System.Threading.Tasks;
using MQTT.Server;
using InfluxDB.Test;

namespace YourNamespace
{
    public class Program
    {
        static async Task Main(string[] args) // Main method should be static
        {
            // Create an instance of Server_Simple_Samples
            //await Server_Simple_Samples.Run_Server_With_Logging();
            await InfluxDB_Samples.InfluxDBWrite("1", 25.0f, 1000.0f, 65.0f);
            //await Server_Simple_Samples.Run_Server_With_Logging();
            await Server_Simple_Samples.Run_Minimal_Server();


        }
    }
}