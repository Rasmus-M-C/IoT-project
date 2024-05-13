# IoT Project - Weather station with RasperryPi 3.0

## Network
RaspberryPi and host computer needs to be on the same network. This can be done using a mobile hotspot.

## Rider environment
First one need to setup the rider environment to compile the code to a linux-arm processor.

## How to compile and run the code on the RasperryPi
First you build and publish the solution.
Then you copy the files over to the RaspberryPi, and run the solution via SSH on the Pi.

### Publish folder
First you need the publish folder.  
The publish folder can be found by right-clicking in the Solution menu on the left,  
and choosing "Publish" -> "Local folder". Then you can copy the "Target Location". 
When this is copied you insert that as "publish folder from rider", remember the /* on the end.  
An example of a target location could be 
```
/Users/username/RiderProjects/Project/Sensor/bin/Release/net5.0/publish
```
### Create folder on Pi
You have to create the folder on the Pi before publishing and then insert that as "FolderOnPi".  

### RaspberryPi IP
Remember to get the RaspberryPi IP and insert that as "raspberryPiIP".  

Finally you can insert it in the following command to copy the files to the Pi with SCP:
```
scp -r "publish folder from rider"/* admin@raspberryPiIP:~/FolderOnPi
```
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
