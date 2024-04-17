using System;
using System.IO;
using System.Threading.Tasks;
using InfluxDB.LoadEnvironment;
using MQTT.Server;
using InfluxDB.Test;

namespace YourNamespace
{
    public class Program
    {
        static async Task Main(string[] args) // Main method should be static
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);
            
            // Create an instance of Server_Simple_Samples
            //await Server_Simple_Samples.Run_Server_With_Logging();
            await InfluxDB_Samples.InfluxDBWrite("1", 25.0f, 1000.0f, 65.0f);
        }
    }
}