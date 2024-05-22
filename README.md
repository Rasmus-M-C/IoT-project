# IoT Project - Weather station with RasperryPi 3.0

## Network
RaspberryPi and host computer needs to be on the same network. This can be done using a mobile hotspot.
The following settings should be used for the hotspot  
Name: RCDA_2G  
Pass: DANIELOGRASMUSNR6814

## Rider environment
First one need to setup the rider environment to compile the code to a linux-arm processor.

## How to compile and run the code on the RasperryPi
First you build and publish the solution.
Then you copy the files over to the RaspberryPi, and run the solution via SSH on the Pi.  
This is done with scp using the following command:
```
scp -r "publish folder from rider"/* admin@raspberrypi:~/FolderOnPi
```

### Publish folder
First you need the publish folder.  
The publish folder can be found by opening the Project in rider and right-clicking in the Solution menu on the left by  
choosing "Publish" -> "Local folder" and copying the "Target Location". 
When this is copied you insert that as "publish folder from rider", remember the /* on the end.  
An example of a target location could be 
```
/Users/username/RiderProjects/Project/Sensor/bin/Release/net5.0/publish
```
### Create folder on Pi
You have to create the folder on the Pi before publishing and then insert that as "FolderOnPi".  
An example name could be ProjectDir.

### RaspberryPi IP

Finally you can insert it in the command to copy the files to the Pi with SCP. With the examples in this guide the final command would look like this:
```
scp -r /Users/username/RiderProjects/Project/Sensor/bin/Release/net5.0/publish/* admin@raspberrypi:~/ProjectDir
```
If this does not work you can replace raspberrypi with its IP adresse. An example could be 192.168.129.299
### Run the programs

You have to run the Sensor program on the RaspberryPi and a Subscriber on your local machine.  

To run the Subscriber on your local machine, select the "Subscriber" configuration at the top right in rider and then clicking the green arrow to run the configuration.

When the files have been copied to the Pi, you can run the solution on the Pi.  
This is done by moving into the folder you copied to (FolderOnPi) using cd.  

When you are in the right folder, you can run the sensor solution with the following command:
```
./RaspberryPi
```

Then the program will run until canceled by ctrl+c

If it fails you might have to make the program executable with the following command inside the program folder:
```
chmod +x RaspberryPi
```
