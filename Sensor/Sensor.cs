using System;
using MQTTnet;
using MQTTnet.Channel;
using Sense.RTIMU;

namespace RaspberryPi
{

    public interface ISensor
    {
        float Measure();
    }

    public class HumiditySensor : ISensor
    {
        public float Measure()
        {
            var settings = RTIMUSettings.CreateDefault();
            using (var imu = settings.CreateIMU())
            using (var sensorType = settings.CreateHumidity())
            {
                var measurement = sensorType.Read();

                return measurement.Humidity;

            }
        }
    }

    public class PressureSensor : ISensor
    {
        public float Measure()
        {
            var settings = RTIMUSettings.CreateDefault();
            using (var imu = settings.CreateIMU())
            using (var sensorType = settings.CreatePressure())
            {
                var measurement = sensorType.Read();

                return measurement.Pressure;

            }
        }
    }

    public class Sensor
    {
        public string HatSensor { get; set; }

        public ISensor CreateSensor()
        {
            switch (HatSensor)
            {
                case "Humidity":
                    return new HumiditySensor();
                case "Pressure":
                    return new PressureSensor();
                default:
                    throw new ArgumentException("Invalid sensor type");
            }

        } 
    }
}