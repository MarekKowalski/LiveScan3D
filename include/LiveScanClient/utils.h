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
#include <stdio.h>
#include <string>
#include <vector>

enum INCOMING_MESSAGE_TYPE
{
	MSG_CAPTURE_FRAME,
	MSG_CALIBRATE,
	MSG_RECEIVE_SETTINGS,
	MSG_REQUEST_STORED_FRAME,
	MSG_REQUEST_LAST_FRAME,
	MSG_RECEIVE_CALIBRATION,
	MSG_CLEAR_STORED_FRAMES
};

enum OUTGOING_MESSAGE_TYPE
{
	MSG_CONFIRM_CAPTURED,
	MSG_CONFIRM_CALIBRATED,
	MSG_STORED_FRAME,
	MSG_LAST_FRAME
};

typedef struct Point3f
{
	Point3f()
	{
		this->X = 0;
		this->Y = 0;
		this->Z = 0;
	}
	Point3f(float X, float Y, float Z)
	{
		this->X = X;
		this->Y = Y;
		this->Z = Z;
	}
	float X;
	float Y;
	float Z;
} Point3f;

typedef struct Point3s
{
	Point3s()
	{
		this->X = 0;
		this->Y = 0;
		this->Z = 0;
	}
	Point3s(short X, short Y, short Z)
	{
		this->X = X;
		this->Y = Y;
		this->Z = Z;
	}
	//meters to milimeters
	Point3s(Point3f &other)
	{
		this->X = static_cast<short>(1000 * other.X);
		this->Y = static_cast<short>(1000 * other.Y);
		this->Z = static_cast<short>(1000 * other.Z);
	}
	short X;
	short Y;
	short Z;
} Point3s;

typedef struct Point2f
{
	Point2f()
	{
		this->X = 0;
		this->Y = 0;
	}
	Point2f(float X, float Y)
	{
		this->X = X;
		this->Y = Y;
	}
	float X;
	float Y;
} Point2f;

typedef struct RGB
{
	BYTE    rgbBlue;
	BYTE    rgbGreen;
	BYTE    rgbRed;
	BYTE    rgbReserved;
} RGB;

Point3f RotatePoint(Point3f &point, std::vector<std::vector<float>> &R);
Point3f InverseRotatePoint(Point3f &point, std::vector<std::vector<float>> &R);
