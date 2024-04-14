#!/bin/bash

# Build the .NET project
dotnet build

# Publish the project
dotnet publish -o 'C:\Users\rasmu\RiderProjects\Sensor\Sensor\bin\Release\net5.0\linux-arm\publish\'

# Copy the published files to Raspberry Pi
scp -r C:\Users\rasmu\RiderProjects\Sensor\Sensor\bin\Release\net5.0\linux-arm\publish\* admin@192.168.50.7:~/Project
