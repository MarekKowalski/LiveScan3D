#include "azureKinectCapture.h"
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/opencv.hpp>
#include <chrono>

AzureKinectCapture::AzureKinectCapture()
{

}

AzureKinectCapture::~AzureKinectCapture()
{
	k4a_image_release(colorImage);
	k4a_image_release(depthImage);
	k4a_image_release(pointCloudImage);
	k4a_image_release(colorImageInDepth);
	k4a_image_release(depthImageInColor);
	k4a_image_release(transformedDepthImage);
	k4a_image_release(colorImageDownscaled);
	k4a_transformation_destroy(transformation);
	k4a_device_close(kinectSensor);
}

bool AzureKinectCapture::Initialize()
{
	uint32_t count = k4a_device_get_installed_count();
	int deviceIdx = 0;

	kinectSensor = NULL;
	while (K4A_FAILED(k4a_device_open(deviceIdx, &kinectSensor)))
	{
		deviceIdx++;
		if (deviceIdx >= count)
		{
			bInitialized = false;
			return bInitialized;
		}
	}

	k4a_device_configuration_t config = K4A_DEVICE_CONFIG_INIT_DISABLE_ALL;
	config.camera_fps = K4A_FRAMES_PER_SECOND_30;
	config.color_format = K4A_IMAGE_FORMAT_COLOR_BGRA32;
	config.color_resolution = K4A_COLOR_RESOLUTION_720P;
	config.depth_mode = K4A_DEPTH_MODE_NFOV_UNBINNED;
	config.synchronized_images_only = true;

	// Start the camera with the given configuration
	bInitialized = K4A_SUCCEEDED(k4a_device_start_cameras(kinectSensor, &config));

	k4a_calibration_t calibration;
	if (K4A_FAILED(k4a_device_get_calibration(kinectSensor, config.depth_mode, config.color_resolution, &calibration)))
	{
		bInitialized = false;
		return bInitialized;
	}
	transformation = k4a_transformation_create(&calibration);


	//No way to get the depth pixel values from the SDK at the moment, so this is hardcoded
	int depth_camera_width;
	int depth_camera_height;

	switch (config.depth_mode)
	{
	case K4A_DEPTH_MODE_NFOV_UNBINNED:	
		depth_camera_width = 640;
		depth_camera_height = 576;
		break;
	case K4A_DEPTH_MODE_NFOV_2X2BINNED:
		depth_camera_width = 320;
		depth_camera_height = 288;
		break;
	case K4A_DEPTH_MODE_WFOV_UNBINNED:
		depth_camera_width = 1024;
		depth_camera_height = 1024;
	case K4A_DEPTH_MODE_WFOV_2X2BINNED:
		depth_camera_width = 512;
		depth_camera_height = 512;
		break;
	default:
		break;
	}
	
	//It's crucial for this program to output accurately mapped Pointclouds. The highest accuracy mapping is achieved
	//by using the k4a_transformation_depth_image_to_color_camera function. However this converts a small depth image 
	//to a larger size, equivalent to the the color image size. This means more points to process and higher processing costs
	//We can however scale the color image to the depth images size beforehand, to reduce proccesing power. 

	//We calculate the minimum size that the color Image can be, while preserving its aspect ration
	float rescaleRatio = (float)calibration.color_camera_calibration.resolution_height / (float)depth_camera_height;
	colorImageDownscaledHeight = depth_camera_height;
	colorImageDownscaledWidth = calibration.color_camera_calibration.resolution_width / rescaleRatio;

	//We don't only need the size in pixels of the downscaled color image, but also a new k4a_calibration_t which fits the new 
	//sizes
	k4a_calibration_t calibrationColorDownscaled;
	memcpy(&calibrationColorDownscaled, &calibration, sizeof(k4a_calibration_t));
	calibrationColorDownscaled.color_camera_calibration.resolution_width /= rescaleRatio;
	calibrationColorDownscaled.color_camera_calibration.resolution_height /= rescaleRatio;
	calibrationColorDownscaled.color_camera_calibration.intrinsics.parameters.param.cx /= rescaleRatio;
	calibrationColorDownscaled.color_camera_calibration.intrinsics.parameters.param.cy /= rescaleRatio;
	calibrationColorDownscaled.color_camera_calibration.intrinsics.parameters.param.fx /= rescaleRatio;
	calibrationColorDownscaled.color_camera_calibration.intrinsics.parameters.param.fy /= rescaleRatio;
	transformationColorDownscaled = k4a_transformation_create(&calibrationColorDownscaled);

	std::chrono::time_point<std::chrono::system_clock> start = std::chrono::system_clock::now();
	bool bTemp;
	do
	{
		bTemp = AcquireFrame();

		std::chrono::duration<double> elapsedSeconds = std::chrono::system_clock::now() - start;
		if (elapsedSeconds.count() > 5.0)
		{
			bInitialized = false;
			break;
		}
	} while (!bTemp);

	size_t serialNoSize;
	k4a_device_get_serialnum(kinectSensor, NULL, &serialNoSize);
	serialNumber = std::string(serialNoSize, '\0');
	k4a_device_get_serialnum(kinectSensor, (char*)serialNumber.c_str(), &serialNoSize);


	return bInitialized;
}

bool AzureKinectCapture::AcquireFrame()
{
	if (!bInitialized)
	{
		return false;
	}

	k4a_capture_t capture = NULL;

	k4a_wait_result_t captureResult = k4a_device_get_capture(kinectSensor, &capture, captureTimeoutMs);
	if (captureResult != K4A_WAIT_RESULT_SUCCEEDED)
	{
		k4a_capture_release(capture);
		return false;
	}

	k4a_image_release(colorImage);
	k4a_image_release(colorImageDownscaled);
	k4a_image_release(depthImage);

	colorImage = k4a_capture_get_color_image(capture);
	depthImage = k4a_capture_get_depth_image(capture);
	if (colorImage == NULL || depthImage == NULL)
	{
		k4a_capture_release(capture);
		return false;
	}

	//We need to resize the color image, so that it's height fits the depth camera height, while preserving the aspect ratio of the color camera:

	//Convert the k4a_image to an OpenCV Mat
	cv::Mat cImg = cv::Mat(k4a_image_get_height_pixels(colorImage), k4a_image_get_width_pixels(colorImage), CV_8UC4, k4a_image_get_buffer(colorImage));

	//Resize the k4a_image to the precalculated size. Takes quite along time, maybe there is a faster algorithm?
	cv::resize(cImg, cImg, cv::Size(colorImageDownscaledWidth, colorImageDownscaledHeight), cv::INTER_LINEAR);

	//Create a k4a_image from the resized OpenCV Mat. Code taken from here: https://github.com/microsoft/Azure-Kinect-Sensor-SDK/issues/978#issuecomment-566002061
	k4a_image_create(K4A_IMAGE_FORMAT_COLOR_BGRA32, cImg.cols, cImg.rows, cImg.cols * 4 * (int)sizeof(uint8_t), &colorImageDownscaled);
	memcpy(k4a_image_get_buffer(colorImageDownscaled), &cImg.ptr<cv::Vec4b>(0)[0], cImg.rows * cImg.cols * sizeof(cv::Vec4b));


	if (pColorRGBX == NULL)
	{
		nColorFrameHeight = k4a_image_get_height_pixels(colorImageDownscaled);
		nColorFrameWidth = k4a_image_get_width_pixels(colorImageDownscaled);
		pColorRGBX = new RGB[nColorFrameWidth * nColorFrameHeight];
	}

	if (pDepth == NULL)
	{
		nDepthFrameHeight = k4a_image_get_height_pixels(depthImage);
		nDepthFrameWidth = k4a_image_get_width_pixels(depthImage);
		pDepth = new UINT16[nDepthFrameHeight * nDepthFrameWidth];
	}

	

	memcpy(pColorRGBX, k4a_image_get_buffer(colorImageDownscaled), nColorFrameWidth * nColorFrameHeight * sizeof(RGB));
	memcpy(pDepth, k4a_image_get_buffer(depthImage), nDepthFrameHeight * nDepthFrameWidth * sizeof(UINT16));

	k4a_capture_release(capture);

	return true;
}

void AzureKinectCapture::UpdateDepthPointCloudForColorFrame()
{
	if (transformedDepthImage == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16, nColorFrameWidth, nColorFrameHeight, nColorFrameWidth * (int)sizeof(uint16_t), &transformedDepthImage);
	}

	if (pointCloudImage == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM, nColorFrameWidth, nColorFrameHeight, nColorFrameWidth * 3 * (int)sizeof(int16_t), &pointCloudImage);
	}

	k4a_transformation_depth_image_to_color_camera(transformationColorDownscaled, depthImage, transformedDepthImage);

	k4a_transformation_depth_image_to_point_cloud(transformationColorDownscaled, transformedDepthImage, K4A_CALIBRATION_TYPE_COLOR, pointCloudImage);
}

void AzureKinectCapture::UpdateDepthPointCloud()
{
	if (pointCloudImage == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nDepthFrameWidth, nDepthFrameHeight,
			nDepthFrameWidth * 3 * (int)sizeof(int16_t),
			&pointCloudImage);
	}

	k4a_transformation_depth_image_to_point_cloud(transformation, depthImage, K4A_CALIBRATION_TYPE_DEPTH, pointCloudImage);
}

void AzureKinectCapture::MapDepthFrameToCameraSpace(Point3f *pCameraSpacePoints)
{
	UpdateDepthPointCloud();

	int16_t* pointCloudData = (int16_t*)k4a_image_get_buffer(pointCloudImage);

	for (int i = 0; i < nDepthFrameHeight; i++)
	{
		for (int j = 0; j < nDepthFrameWidth; j++)
		{
			pCameraSpacePoints[j + i * nDepthFrameWidth].X = pointCloudData[3 * (j + i * nDepthFrameWidth) + 0] / 1000.0f;
			pCameraSpacePoints[j + i * nDepthFrameWidth].Y = pointCloudData[3 * (j + i * nDepthFrameWidth) + 1] / 1000.0f;
			pCameraSpacePoints[j + i * nDepthFrameWidth].Z = pointCloudData[3 * (j + i * nDepthFrameWidth) + 2] / 1000.0f;
		}
	}
}

void AzureKinectCapture::MapColorFrameToCameraSpace(Point3f* pCameraSpacePoints)
{
	UpdateDepthPointCloudForColorFrame();

	int16_t* pointCloudData = (int16_t*)k4a_image_get_buffer(pointCloudImage);

	for (int i = 0; i < nColorFrameHeight; i++)
	{
		for (int j = 0; j < nColorFrameWidth; j++)
		{
			pCameraSpacePoints[j + i * nColorFrameWidth].X = pointCloudData[3 * (j + i * nColorFrameWidth) + 0] / 1000.0f;
			pCameraSpacePoints[j + i * nColorFrameWidth].Y = pointCloudData[3 * (j + i * nColorFrameWidth) + 1] / 1000.0f;
			pCameraSpacePoints[j + i * nColorFrameWidth].Z = pointCloudData[3 * (j + i * nColorFrameWidth) + 2] / 1000.0f;
		}
	}
}

void AzureKinectCapture::MapDepthFrameToColorSpace(UINT16 *pDepthInColorSpace)
{
	if (depthImageInColor == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16, nColorFrameWidth, nColorFrameHeight,
			nColorFrameWidth * (int)sizeof(uint16_t),
			&depthImageInColor);
	}

	k4a_transformation_depth_image_to_color_camera(transformationColorDownscaled, depthImage, depthImageInColor);

	memcpy(pDepthInColorSpace, k4a_image_get_buffer(depthImageInColor), nColorFrameHeight * nColorFrameWidth * (int)sizeof(uint16_t));
}

void AzureKinectCapture::MapColorFrameToDepthSpace(RGB* pColorInDepthSpace)
{
	if (colorImageInDepth == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_COLOR_BGRA32, nDepthFrameWidth, nDepthFrameHeight,
			nDepthFrameWidth * 4 * (int)sizeof(uint8_t),
			&colorImageInDepth);
	}

	k4a_transformation_color_image_to_depth_camera(transformationColorDownscaled, depthImage, colorImage, colorImageInDepth);

	memcpy(pColorInDepthSpace, k4a_image_get_buffer(colorImageInDepth), nDepthFrameHeight * nDepthFrameWidth * 4 * (int)sizeof(uint8_t));
}


