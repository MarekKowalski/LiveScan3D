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

#include "stdafx.h"
#include "ICapture.h"
#include "Kinect.h"
#include "utils.h"

class KinectCapture : public ICapture
{
public:
	KinectCapture();
	~KinectCapture();

	bool Initialize();
	bool AcquireFrame();
	void MapDepthFrameToCameraSpace(Point3f *pCameraSpacePoints);
	void MapColorFrameToCameraSpace(Point3f *pCameraSpacePoints);
	void MapDepthFrameToColorSpace(Point2f *pColorSpacePoints);
	void MapColorFrameToDepthSpace(Point2f *pDepthSpacePoints);
private:	
	ICoordinateMapper* pCoordinateMapper;
	IKinectSensor* pKinectSensor;
	IMultiSourceFrameReader* pMultiSourceFrameReader;

	void GetDepthFrame(IMultiSourceFrame* pMultiFrame);
	void GetColorFrame(IMultiSourceFrame* pMultiFrame);
	void GetBodyFrame(IMultiSourceFrame* pMultiFrame);
	void GetBodyIndexFrame(IMultiSourceFrame* pMultiFrame);
};
