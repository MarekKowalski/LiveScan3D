#pragma once

#include "stdafx.h"
#include "ICapture.h"
#include <k4a/k4a.h>
#include <opencv2/opencv.hpp>
#include "utils.h"
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

class AzureKinectCapture : public ICapture
{
public:
	AzureKinectCapture();
	~AzureKinectCapture();

	bool Initialize(SYNC_STATE state, int syncOffset);
	bool AcquireFrame();
	bool Close();
	void MapDepthFrameToCameraSpace(Point3f *pCameraSpacePoints);
	void MapColorFrameToCameraSpace(Point3f *pCameraSpacePoints);
	void MapDepthFrameToColorSpace(UINT16 *pDepthInColorSpace);
	void MapColorFrameToDepthSpace(RGB *pColorInDepthSpace);
	int GetSyncJackState();
	uint64_t GetTimeStamp();
	int GetDeviceIndex();
	void SetExposureState(bool enableAutoExposure, int exposureStep);

private:
	k4a_device_t kinectSensor = NULL;
	int32_t captureTimeoutMs = 1000;
	k4a_image_t colorImage = NULL;
	k4a_image_t depthImage = NULL;
	k4a_image_t pointCloudImage = NULL;
	k4a_image_t transformedDepthImage = NULL;
	k4a_image_t colorImageInDepth = NULL;
	k4a_image_t depthImageInColor = NULL;
	k4a_image_t colorImageDownscaled = NULL;
  k4a_transformation_t transformationColorDownscaled = NULL;
  k4a_transformation_t transformation = NULL;

	int colorImageDownscaledWidth;
	int colorImageDownscaledHeight;
  
	bool syncInConnected = false;
	bool syncOutConnected = false;
	uint64_t currentTimeStamp = 0;
	SYNC_STATE syncState = Standalone;
	int deviceIDForRestart = -1;
	int restartAttempts = 0;
	bool autoExposureEnabled = true;
	int exposureTimeStep = 0;
  
	void UpdateDepthPointCloud();
	void UpdateDepthPointCloudForColorFrame();
};