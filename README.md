# IoT Project - Weather station with RasperryPi 3.0

## Network
RaspeeriPi and host computer needs to be on the same network. This can be made using a mobile hotspot.

## Rider environment
First one need to setup the rider environment to compile the code to a linux-arm processor.

## How to compile and run the code on the RasperryPi
First you build and publish the solution.
Then you copy the files over to the RaspberryPi, and run the solution via SSH on the Pi.

First you need the publish folder.
The publish folder can be found by right-clicking in the Solution menu on the left, and choosing "Publish" -> "Local folder". Then you can copy the "Target Location".
And insert that as "publish folder from rider", remember the /* on the end.

You have to create the folder on the Pi before publishing and then insert that as "FolderOnPi".
To copy the files to the Pi we use SCP with the following command:
```
scp -r "publish folder from rider"/* admin@raspberryPiIP:~/FolderOnPi
```

After it has been published and copied over, you can run the solution on the Pi.

This is done by moving into the folder you copied to (FolderOnPi). For the sensor program you run:
```
./RaspberryPi
```
For the subscriber program you run:
```
./Subscriber
```

Then the program will run until canceled by ctrl+c
