using System;
using System.Threading.Tasks;
using MQTT.Server;

namespace YourNamespace
{
    public class Program
    {
        static async Task Main(string[] args) // Main method should be static
        {
            // Create an instance of Server_Simple_Samples
            await Server_Simple_Samples.Run_Server_With_Logging();
            

            
        }
    }
}