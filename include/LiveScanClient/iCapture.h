//   Copyright (C) 2015  Marek Kowalski (M.Kowalski@ire.pw.edu.pl), Jacek Naruniec (J.Naruniec@ire.pw.edu.pl)
//   License: MIT Software License   See LICENSE.txt for the full license.

//   If you use this software in your research, then please use the following citation:

//    Kowalski, M.; Naruniec, J.; Daniluk, M.: "LiveScan3D: A Fast and Inexpensive 3D Data
//    Acquisition System for Multiple Kinect v2 Sensors". in 3D Vision (3DV), 2015 International Conference on, Lyon, France, 2015

//    @INPROCEEDINGS{Kowalski15,
//        author={Kowalski, M. and Naruniec, J. and Daniluk, M.},
//        booktitle={3D Vision (3DV), 2015 International Conference on},
//        title={LiveScan3D: A Fast and Inexpensive 3D Data Acquisition System for Multiple Kinect v2 Sensors},
//        year={2015},
//    }
#pragma once

#include "utils.h"
#include "Kinect.h"

struct Body
{
	Body()
	{
		bTracked = false;
		vJoints.resize(JointType_Count);
		vJointsInColorSpace.resize(JointType_Count);
	}
	bool bTracked;
	std::vector<Joint> vJoints;
	std::vector<Point2f> vJointsInColorSpace;
};

class ICapture
{
public:
	ICapture();
	~ICapture();

	virtual bool Initialize() = 0;
	virtual bool AcquireFrame() = 0;
	virtual void MapDepthFrameToCameraSpace(Point3f *pCameraSpacePoints) = 0;
	virtual void MapColorFrameToCameraSpace(Point3f *pCameraSpacePoints) = 0;
	virtual void MapDepthFrameToColorSpace(Point2f *pColorSpacePoints) = 0;
	virtual void MapColorFrameToDepthSpace(Point2f *pDepthSpacePoints) = 0;

	bool bInitialized;

	int nColorFrameHeight, nColorFrameWidth;
	int nDepthFrameHeight, nDepthFrameWidth;

	UINT16 *pDepth;
	BYTE *pBodyIndex;
	RGB *pColorRGBX;
	std::vector<Body> vBodies;
};