﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Collections.Generic;
using System.Threading;

namespace InfluxDB.Test
{
    public static class InfluxDB_Samples
    {
        // Temp, pressure, Humidity
        public static async Task InfluxDBWrite(string sensorID ,float temp, float press, float humidity)
        {
            string influxUrl = "https://eu-central-1-1.aws.cloud2.influxdata.com";
            string token = Environment.GetEnvironmentVariable("API_TOKEN");
            Console.WriteLine(token);
            const string org = "WeatherDay Co";
            const string bucket = "weatherdata";
            
            using (var client = new InfluxDBClient(influxUrl, token))
            {
                // Define data points
                var dataPoints = new List<PointData>
                {
                    CreatePoint("home", sensorID,new Dictionary<string, object> {
                        {"temp", temp },
                        {"pressure", press},
                        {"humidity", humidity}// Assuming it is in float
                    }),
                };
                
                foreach (var point in dataPoints)
                {
                    await client.GetWriteApiAsync().WritePointAsync(point: point, bucket: bucket, org: org);
                    //await client.WritePointAsync(point: point, database: bucket);
    
                    Thread.Sleep(1000); // separate points by 1 second
                }
                /*
                // Write the point to InfluxDB
                var writeApi = client.GetWriteApi();
                writeApi.WritePoints(dataPoints, bucket,org);
                */
            }

            Console.WriteLine("Data written successfully to InfluxDB.");
            
        }
        
        static PointData CreatePoint(string measurement, string sensorId,Dictionary<string, object> fields)
        {
            // https://eu-central-1-1.aws.cloud2.influxdata.com/orgs/d119a21a75bd117f/new-user-setup/csharpe
            var builder = PointData.Measurement(measurement);
            builder.Tag("sensorId", sensorId);
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
                else if (field.Value is float)
                {
                    builder = builder.Field(field.Key, (float)field.Value);
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