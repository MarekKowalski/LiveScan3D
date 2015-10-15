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
#include <stdio.h>
#include <vector>

#include "icp.h"
#include "opencv\cv.h"

using namespace std;

struct RGB
{
	unsigned char R, G, B;
};

void savePLY(std::string filename, std::vector<Point3f> vertices, std::vector<RGB> colors)
{

	unsigned int numVertices = vertices.size();
	unsigned int numColors = colors.size();

	// Open File
	FILE *meshFile = NULL;
	errno_t err = fopen_s(&meshFile, filename.c_str(), "wt");


	// Write the header line
	std::string header = "ply\nformat ascii 1.0\n";
	fwrite(header.c_str(), sizeof(char), header.length(), meshFile);

	const unsigned int bufSize = 1000 * 3;
	char outStr[bufSize];
	int written = 0;

	// Elements are: x,y,z, r,g,b
	written = sprintf_s(outStr, bufSize, "element vertex %u\nproperty float x\nproperty float y\nproperty float z\nproperty uchar red\nproperty uchar green\nproperty uchar blue\n", numVertices);
	fwrite(outStr, sizeof(char), written, meshFile);

	written = sprintf_s(outStr, bufSize, "end_header\n");
	fwrite(outStr, sizeof(char), written, meshFile);


	// Sequentially write the 3 vertices of the triangle, for each triangle
	for (unsigned int vertexIndex = 0; vertexIndex < numVertices; vertexIndex++)
	{
		unsigned int color0 = colors[vertexIndex].R;
		unsigned int color1 = colors[vertexIndex].G;
		unsigned int color2 = colors[vertexIndex].B;

		written = sprintf_s(outStr, bufSize, "%f %f %f %u %u %u\n",
			vertices[vertexIndex].X, vertices[vertexIndex].Y, vertices[vertexIndex].Z,
			((color0)& 255), ((color1)& 255), (color2 & 255));

		fwrite(outStr, sizeof(char), written, meshFile);
	}

	fflush(meshFile);
	fclose(meshFile);
}

void loadPLY(string filename, vector<Point3f> &verts, vector<RGB> &colors)
{
	FILE *f;
	int nVerts;

	fopen_s(&f, filename.c_str(), "r");

	char buffer[100];
	fgets(buffer, 100, f);
	fgets(buffer, 100, f);
	fgets(buffer, 100, f);
	sscanf_s(buffer, "element vertex %d\n", &nVerts);

	for (int i = 0; i < 7; i++)
		fgets(buffer, 100, f);

	for (int i = 0; i < nVerts; i++)
	{
		fgets(buffer, 100, f);

		Point3f point;
		RGB rgb;
		int R, G, B;
		sscanf_s(buffer, "%f %f %f %d %d %d\n", &point.X, &point.Y, &point.Z, &R, &G, &B);
		
		rgb.R = R;
		rgb.G = G;
		rgb.B = B;

		verts.push_back(point);
		colors.push_back(rgb);
	}

	fclose(f);
}


//This function here can be used to test the ICP functionality, it aligns the points clouds in "test1.ply" and "test2.ply"
int main()
{
	vector<Point3f> verts1, verts2;
	vector<RGB> colors1, colors2;

	loadPLY("../test1.ply", verts1, colors1);
	loadPLY("../test2.ply", verts2, colors2);

	cv::Mat R = cv::Mat::eye(3, 3, CV_32F);
	cv::Mat t(1, 3, CV_32F, cv::Scalar(0));
	cv::Mat verts2Mat(verts2.size(), 3, CV_32F, (float*)verts2.data());

	ICP(verts1.data(), verts2.data(), verts1.size(), verts2.size(), (float*)R.data, (float*)t.data, 1);

	savePLY("../testResult.ply", verts2, colors2);

	return 0;
}
