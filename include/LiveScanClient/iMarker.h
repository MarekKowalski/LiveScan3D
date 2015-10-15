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

//struktura przechowywuj¹ca wszystkie dane markera
typedef struct MarkerStruct
{
	int id;
	//po³o¿enie naro¿ników markera w obrazie
	std::vector<Point2f> corners;
	//te same punkty w uk³adzie wspó³rzêdnych markera
	std::vector<Point3f> points;

	MarkerStruct()
	{
		id = -1;
	}

	MarkerStruct(int id, std::vector<Point2f> corners, std::vector<Point3f> points)
	{
		this->id = id;

		this->corners = corners;
		this->points = points;
	}
} MarkerInfo;

class IMarker
{
public:
	IMarker() {};

	//znajduje wszystkie markery w obrazie i zapisuje je w zmiennej markers
	virtual bool GetMarker(RGB *img, int height, int width, MarkerInfo &marker) = 0;
};