using MQTTnet;
using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Protocol;


namespace RaspberryPi

{
    public class MQTTClient
    {
        private IMqttClient Client { get; set; }
        
        private string BrokerIP { get; set; }
        
        public MQTTClient(string brokerIp = "192.168.50.93")
        {
            BrokerIP = brokerIp;
            var mqttFactory = new MqttFactory();
            Client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(BrokerIP, 1883)
                .WithCleanSession()
                .Build();
            Client.ConnectAsync(options);
        }

        public async Task PublishMessageAsync(double payload, string topic)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload.ToString())
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        if (Client.IsConnected)
        {
            await Client.PublishAsync(message);
        }
    }
        
    }
    
    

}