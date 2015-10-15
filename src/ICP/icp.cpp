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
#include "icp.h"
#include "opencv\cv.h"

void FindClosestPointForEach(PointCloud &sourceCloud, cv::Mat &destPoints, vector<float> &distances, vector<size_t> &indices)
{
	int nVerts2 = destPoints.rows;
	typedef nanoflann::KDTreeSingleIndexAdaptor<nanoflann::L2_Simple_Adaptor<float, PointCloud>, PointCloud, 3> kdTree;
	kdTree tree(3, sourceCloud);
	tree.buildIndex();

#pragma omp parallel for
	for (int i = 0; i < nVerts2; i++)
	{
		nanoflann::KNNResultSet<float> resultSet(1);
		resultSet.init(&indices[i], &distances[i]);
		tree.findNeighbors(resultSet, (float*)destPoints.row(i).data, nanoflann::SearchParams());
	}
}

float GetStandardDeviation(vector<float> &data)
{
	float mean = 0;
	for (size_t i = 0; i < data.size(); i++)
	{
		mean += data[i];
	}
	mean /= data.size();

	float std = 0;

	for (size_t i = 0; i < data.size(); i++)
	{
		std += pow(data[i] - mean, 2);
	}

	std /= data.size();
	std = sqrt(std);

	return std;
}

void RejectOutlierMatches(vector<Point3f> &matches1, vector<Point3f> &matches2, vector<float> &matchDistances, float maxStdDev)
{
	float distanceStandardDev = GetStandardDeviation(matchDistances);

	vector<Point3f> filteredMatches1;
	vector<Point3f> filteredMatches2;
	for (size_t i = 0; i < matches1.size(); i++)
	{
		if (matchDistances[i] > maxStdDev * distanceStandardDev)
			continue;

		filteredMatches1.push_back(matches1[i]);
		filteredMatches2.push_back(matches2[i]);
	}

	matches1 = filteredMatches1;
	matches2 = filteredMatches2;
}

ICP_API float __stdcall ICP(Point3f *verts1, Point3f *verts2, int nVerts1, int nVerts2, float *R, float *t, int maxIter)
{
	PointCloud cloud1;
	cloud1.pts = vector<Point3f>(verts1, verts1 + nVerts1);

	cv::Mat matR(3, 3, CV_32F, R);
	cv::Mat matT(1, 3, CV_32F, t);

	cv::Mat verts2Mat(nVerts2, 3, CV_32F, (float*)verts2);

	float error = 1;

	for (int iter = 0; iter < maxIter; iter++)
	{
		vector<Point3f> matched1, matched2;

		vector<float> distances(nVerts2);
		vector<size_t> indices(nVerts2);
		FindClosestPointForEach(cloud1, verts2Mat, distances, indices);

		vector<float> matchDistances;
		vector<int> matchIdxs(nVerts1, -1);
		for (int i = 0; i < nVerts2; i++)
		{
			int pos = matchIdxs[indices[i]];

			if (pos != -1)
			{
				if (matchDistances[pos] < distances[i])
					continue;
			}

			Point3f temp;
			temp.X = verts2Mat.at<float>(i, 0);
			temp.Y = verts2Mat.at<float>(i, 1);
			temp.Z = verts2Mat.at<float>(i, 2);

			if (pos == -1)
			{
				matched1.push_back(verts1[indices[i]]);
				matched2.push_back(temp);

				matchDistances.push_back(distances[i]);

				matchIdxs[indices[i]] = matched1.size() - 1;
			}
			else
			{
				matched2[pos] = temp;
				matchDistances[pos] = distances[i];
			}
		}

		RejectOutlierMatches(matched1, matched2, matchDistances, 2.5);

		//error = 0;
		//for (int i = 0; i < matchDistances.size(); i++)
		//{
		//	error += sqrt(matchDistances[i]);
		//}
		//error /= matchDistances.size();
		//cout << error << endl;

		cv::Mat matched1MatCv(matched1.size(), 3, CV_32F, matched1.data());
		cv::Mat matched2MatCv(matched2.size(), 3, CV_32F, matched2.data());
		cv::Mat tempT;
		cv::reduce(matched1MatCv - matched2MatCv, tempT, 0, CV_REDUCE_AVG);

		for (int i = 0; i < verts2Mat.rows; i++)
		{
			verts2Mat.row(i) += tempT;
		}
		for (int i = 0; i < matched2MatCv.rows; i++)
		{
			matched2MatCv.row(i) += tempT;
		}

		cv::Mat M = matched2MatCv.t() * matched1MatCv;
		cv::SVD svd;
		svd(M);
		cv::Mat tempR = svd.u * svd.vt;

		double det = cv::determinant(tempR);
		if (det < 0)
		{
			cv::Mat temp = cv::Mat::eye(3, 3, CV_32F);
			temp.at<float>(2, 2) = -1;
			tempR = svd.u * temp * svd.vt;
		}

		verts2Mat = verts2Mat * tempR;

		matT += tempT * matR.t();
		matR = matR * tempR;
	}

	memcpy(verts2, verts2Mat.data, verts2Mat.rows * sizeof(float) * 3);

	memcpy(R, matR.data, 9 * sizeof(float));
	memcpy(t, matT.data, 3 * sizeof(float));

	return error;
}
