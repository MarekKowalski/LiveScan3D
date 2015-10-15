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
#include "filter.h"

using namespace std;

vector<KNNeighborsResult> KNNeighbors(PointCloud &cloud, kdTree &tree, int k)
{
	vector<KNNeighborsResult> result(cloud.pts.size());
	int nCloudPts = static_cast<int>(cloud.pts.size());

#pragma omp parallel for
	for (int i = 0; i < nCloudPts; i++)
	{
		result[i].neighbors.resize(k);
		result[i].distances.resize(k);
		tree.knnSearch((float*)(cloud.pts.data() + i), k, (size_t*)result[i].neighbors.data(), result[i].distances.data());
		result[i].kDistance = result[i].distances[k - 1];
	}

	return result;
}

void filter(std::vector<Point3f> &vertices, std::vector<RGB> &colors, int k, float maxDist)
{
	if (k <= 0 || maxDist <= 0)
		return;

	PointCloud cloud;
	cloud.pts = vertices;

	kdTree tree(3, cloud);
	tree.buildIndex();

	vector<KNNeighborsResult> knn = KNNeighbors(cloud, tree, k);
	
	vector<int> indicesToRemove;

	float distThreshold = pow(maxDist, 2);
	for (unsigned int i = 0; i < cloud.pts.size(); i++)
	{
		if (knn[i].kDistance > distThreshold)
			indicesToRemove.push_back(i);
	}

	int lastElemIdx = 0;
	unsigned int idxToCheck = 0;
	for (unsigned int i = 0; i < vertices.size(); i++)
	{
		if (idxToCheck < indicesToRemove.size() && i == indicesToRemove[idxToCheck])
		{
			idxToCheck++;
			continue;
		}
		vertices[lastElemIdx] = vertices[i];
		colors[lastElemIdx] = colors[i];

		lastElemIdx++;
	}

	vertices.resize(lastElemIdx);
	colors.resize(lastElemIdx);
}