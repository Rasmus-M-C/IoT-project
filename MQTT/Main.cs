using System.Threading.Tasks;
using MQTT.Server;

namespace YourNamespace
{
    public class Program
    {
        static async Task Main(string[] args) // Main method should be static
        {
            //await Server_Simple_Samples.Run_Server_With_Logging();
            await Server_Simple_Samples.Run_Minimal_Server();


        }
    }
}