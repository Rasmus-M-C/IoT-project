namespace InfluxDB.LoadEnvironment
{
    using System;
    using System.IO;

    public static class DotEnv
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: .env file not found.");
                return;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length != 2)
                    continue;
                
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}