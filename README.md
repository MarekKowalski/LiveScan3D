# LiveScan3D #
LiveScan3D is a system designed for real time 3D reconstruction using multiple Kinect v2 depth sensors simultaneously at real time speed.

The produced 3D reconstruction is in the form of a coloured point cloud, with points from all of the Kinects placed in the same coordinate system. Possible use scenarios of the system include:
  * capturing an object’s 3D structure from multiple viewpoints simultaneously,
  * capturing a “panoramic” 3D structure of a scene (extending the field of view of one sensor by using many),
  * increasing the density of a point cloud captured by a single sensor, by having multiple sensors capture the same scene.

At the moment connecting multiple Kinect v2 devices to a single computer is difficult and only possible under Linux. Also, the number of the devices or capture speed might be low, because of the limitations of the PCI-E bus.

Because of those limitations, in our system each Kinect v2 sensor is connected to a separate computer. Each of those computers is connected to a server which governs all of the sensors. The server allows the user to perform calibration, filtering, synchronized frame capture, and to visualize the acquired point cloud live.

## How to use it ##
To start working with our software you will need a Windows machine with the Kinect for Windows SDK 2.0, and at least a single Kinect v2 device. You can either build LiveScan3D from source which is easiest using Visual Studio, or you can download the binaries from: http://ztv.ire.pw.edu.pl/mkowalski/.

Both the binary and source distributions contain a manual (in the docs directory) which contains the steps necessary to start working with our software (it won't take more than a couple of minutes to set up).

## Where to get help ##
If you have any problems feel free to contact us: Marek Kowalski <m.kowalski@ire.pw.edu.pl>, Jacek Naruniec <j.naruniec@ire.pw.edu.pl>. We usually answer emails quickly (our timezone is CET).

For details regarding the methods used in LiveScan3D you can take a look at our article: "LiveScan3D: A Fast and Inexpensive 3D Data Acquisition System for Multiple Kinect v2 Sensors" once it is released.

## Licensing ##
While all of our code is licensed under the MIT license, the 3rd party libraries have different licenses:
  * nanoflann - https://github.com/jlblancoc/nanoflann, BSD license
  * OpenCV - https://github.com/Itseez/opencv, 3-clause BSD license
  * OpenTK - https://github.com/opentk/opentk, MIT/X11 license
  * SocketCS - http://www.adp-gmbh.ch/win/misc/sockets.html

## Authors ##
  * Marek Kowalski <m.kowalski@ire.pw.edu.pl>, homepage: http://ztv.ire.pw.edu.pl/mkowalski/
  * Jacek Naruniec <j.naruniec@ire.pw.edu.pl>, homepage: http://home.elka.pw.edu.pl/~jnarunie/
