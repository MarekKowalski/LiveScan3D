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

/// <summary>
/// Initialize the device. If you want to initialize it as Standalone, set "asMaster" and "asSubordinate" to false
/// </summary>
/// <param name="asMaster">Initializes this device as master</param>
/// <param name="asSubordinate">Initalizes this devices as subordinate</param>
/// <param name="syncOffsetMultiplier">Should only be set when initializing as Subordinate. Each subordinate should have a unique, ascending value</param>
/// <returns></returns>
bool AzureKinectCapture::Initialize(SYNC_STATE state, int syncOffsetMultiplier)
{
	uint32_t count = k4a_device_get_installed_count();
	int deviceIdx = 0;

	//We save the deviceId of this Client.
	//When the cameras are reinitialized during runtime, we can then gurantee
	//that each LiveScan instance uses the same device as before (In case two or more Kinects are connected to the same PC)
	//A device ID of -1 means that no Kinects has been successfully initalized yet (only happens when the Client starts)
	if (deviceIDForRestart != -1) {
		deviceIdx = deviceIDForRestart;
	}

	kinectSensor = NULL;
	while (K4A_FAILED(k4a_device_open(deviceIdx, &kinectSensor)))
	{
		
		if (deviceIDForRestart == -1) 
		{
			deviceIdx++;
			if (deviceIdx >= count)
			{
				bInitialized = false;
				return bInitialized;
			}
		}

		
		else {

			//Sometimes the cameras fail to reinitialize, so we try to initialize them three times with a slight delay before failing
			if (restartAttempts > 2) 
			{
				bInitialized = false;
				return bInitialized;
			}

			else {

				restartAttempts++;
				Sleep(200);
			}
			
		}
		
	}

	k4a_device_configuration_t config = K4A_DEVICE_CONFIG_INIT_DISABLE_ALL;
	config.camera_fps = K4A_FRAMES_PER_SECOND_30;
	config.color_format = K4A_IMAGE_FORMAT_COLOR_BGRA32;
	config.color_resolution = K4A_COLOR_RESOLUTION_720P;
	config.depth_mode = K4A_DEPTH_MODE_NFOV_UNBINNED;
	config.synchronized_images_only = true;

	if (state == Master) 
	{
		config.wired_sync_mode = K4A_WIRED_SYNC_MODE_MASTER;
	}

	else if (state == Subordinate) 
	{
		config.wired_sync_mode = K4A_WIRED_SYNC_MODE_SUBORDINATE;
		//Sets the offset on subordinate devices. Should be a multiple of 160, each subordinate having a different multiplier in ascending order.
		//This is very important, as it avoids firing the Kinects lasers at the same time.		
		config.subordinate_delay_off_master_usec = 160 * syncOffsetMultiplier;
	}

	else
	{
		config.wired_sync_mode = K4A_WIRED_SYNC_MODE_STANDALONE;
	}

	// Start the camera with the given configuration
	bInitialized = K4A_SUCCEEDED(k4a_device_start_cameras(kinectSensor, &config));

	k4a_calibration_t calibration;
	if (K4A_FAILED(k4a_device_get_calibration(kinectSensor, config.depth_mode, config.color_resolution, &calibration)))
	{
		bInitialized = false;
		return bInitialized;
	}

	//Workaround for a bug. When the camera starts in manual Exposure mode, the brightness of the RBG image
	//is much lower than in auto exposure mode. To prevent this, we first set the camera to auto exposure mode and 
	//then switch to manual mode again if it has been enabled before

	if (autoExposureEnabled == false) {

		k4a_device_set_color_control(kinectSensor, K4A_COLOR_CONTROL_EXPOSURE_TIME_ABSOLUTE, K4A_COLOR_CONTROL_MODE_AUTO, 0);

		//Give it a second to adjust
		Sleep(1000);

		SetExposureState(false, exposureTimeStep);
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
  
//If this device is a subordinate, it is expected to start capturing at a later time (When the master has started), so we skip this check  
if (state != Subordinate) 
{
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
	}	

	size_t serialNoSize;
	k4a_device_get_serialnum(kinectSensor, NULL, &serialNoSize);
	serialNumber = std::string(serialNoSize, '\0');
	k4a_device_get_serialnum(kinectSensor, (char*)serialNumber.c_str(), &serialNoSize);

	deviceIDForRestart = deviceIdx;
	restartAttempts = 0;

	return bInitialized;
}

bool AzureKinectCapture::Close() 
{
	if (!bInitialized) 
	{
		return false;
	}

	k4a_device_stop_cameras(kinectSensor);
	k4a_device_close(kinectSensor);

	bInitialized = false;


	return true;
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

	currentTimeStamp = k4a_image_get_device_timestamp_usec(colorImage);

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

/// <summary>
/// Enables/Disables Auto Exposure and/or sets the exposure to a step value between -11 and -5 
/// The kinect supports exposure values up to 1, but these are only available in lower FPS modes (5 or 15 FPS)
/// For further information on how this value translates to ms-Values, please take a look
/// at this table: https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/src/color/color_priv.h
/// </summary>
/// <param name="exposureStep">The Exposure Step between -11 and 1</param>
void AzureKinectCapture::SetExposureState(bool enableAutoExposure, int exposureStep)
{
	if (bInitialized) 
	{
		if (enableAutoExposure)
		{
			k4a_device_set_color_control(kinectSensor, K4A_COLOR_CONTROL_EXPOSURE_TIME_ABSOLUTE, K4A_COLOR_CONTROL_MODE_AUTO, 0);

			autoExposureEnabled = true;
		}

		else
		{
			//Formula copied from here: https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/7cd8683a1a71b8baebef4a3537e6edd8639d1e95/examples/k4arecorder/main.cpp#L333
			float absExposure = (exp2f((float)exposureStep) * 1000000.0f);
			//We add 0.5 because C++ always truncates when converting to an integer. 
			//This ensures that values will always be rounded correctly
			float absExposureRoundingMargin = absExposure + 0.5;
			int32_t absoluteExposureInt = (int32_t)absExposureRoundingMargin;

			k4a_device_set_color_control(kinectSensor, K4A_COLOR_CONTROL_EXPOSURE_TIME_ABSOLUTE, K4A_COLOR_CONTROL_MODE_MANUAL, absoluteExposureInt);

			autoExposureEnabled = false;
			exposureTimeStep = exposureStep;
		}
	}
}


/// <summary>
/// Determines if this camera is configured as a Master, Subordinate or Standalone. 
/// This is achieved by looking at how the sync out and sync in ports of the device are connected
/// </summary>
/// <returns>Returns int -1 for Subordinate, int 0 for Master and int 1 for Standalone</returns>
int AzureKinectCapture::GetSyncJackState() 
{
	if (K4A_RESULT_SUCCEEDED == k4a_device_get_sync_jack(kinectSensor, &syncInConnected, &syncOutConnected))
	{
		if (syncInConnected) 
		{
			return -1; //Device is Subordinate, as it recieves a signal via its "Sync In" Port
		}

		else if (!syncInConnected && syncOutConnected)
		{
			return 0; //Device is Master, as it doens't recieve a signal from its "Sync In" Port, but sends one through its "Sync Out" Port
		}

		else
		{
			return 1; //Device is Standalone, as it doesn't have a valid cabel configuration on its Sync Ports
		}
	}

	else 
	{
		return 1; //Probably failed because there are no cabels connected, this means the device should be set as standalone
	}
}

uint64_t AzureKinectCapture::GetTimeStamp() 
{
	return currentTimeStamp;
}

int AzureKinectCapture::GetDeviceIndex() 
{
	return deviceIDForRestart;
}
