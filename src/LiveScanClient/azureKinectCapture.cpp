#include "azureKinectCapture.h"
#include <chrono>

AzureKinectCapture::AzureKinectCapture()
{

}

AzureKinectCapture::~AzureKinectCapture()
{
	k4a_image_release(colorImage);
	k4a_image_release(depthImage);
	k4a_image_release(depthPointCloudImage);
	k4a_image_release(colorImageInDepth);
	k4a_image_release(depthImageInColor);
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
	config.color_resolution = K4A_COLOR_RESOLUTION_1080P;
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
	k4a_image_release(depthImage);


	colorImage = k4a_capture_get_color_image(capture);
	depthImage = k4a_capture_get_depth_image(capture);

	currentTimeStamp = k4a_image_get_device_timestamp_usec(colorImage);

	if (colorImage == NULL || depthImage == NULL)
	{
		k4a_capture_release(capture);
		return false;
	}

	if (pColorRGBX == NULL)
	{
		nColorFrameHeight = k4a_image_get_height_pixels(colorImage);
		nColorFrameWidth = k4a_image_get_width_pixels(colorImage);
		pColorRGBX = new RGB[nColorFrameWidth * nColorFrameHeight];
	}

	if (pDepth == NULL)
	{
		nDepthFrameHeight = k4a_image_get_height_pixels(depthImage);
		nDepthFrameWidth = k4a_image_get_width_pixels(depthImage);
		pDepth = new UINT16[nDepthFrameHeight * nDepthFrameWidth];
	}

	memcpy(pColorRGBX, k4a_image_get_buffer(colorImage), nColorFrameWidth * nColorFrameHeight * sizeof(RGB));
	memcpy(pDepth, k4a_image_get_buffer(depthImage), nDepthFrameHeight * nDepthFrameWidth * sizeof(UINT16));


	k4a_capture_release(capture);

	return true;
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

void AzureKinectCapture::UpdateDepthPointCloud()
{
	if (depthPointCloudImage == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nDepthFrameWidth, nDepthFrameHeight,
			nDepthFrameWidth * 3 * (int)sizeof(int16_t),
			&depthPointCloudImage);
	}

	k4a_transformation_depth_image_to_point_cloud(transformation, depthImage, K4A_CALIBRATION_TYPE_DEPTH, depthPointCloudImage);
}

void AzureKinectCapture::MapDepthFrameToCameraSpace(Point3f *pCameraSpacePoints)
{
	UpdateDepthPointCloud();

	int16_t* pointCloudData = (int16_t*)k4a_image_get_buffer(depthPointCloudImage);

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

// This mapping is much slower then the other ones, use with caution
void AzureKinectCapture::MapColorFrameToCameraSpace(Point3f *pCameraSpacePoints)
{
	UpdateDepthPointCloud();

	// Initializing temporary images
	k4a_image_t colorPointCloudImageX, colorPointCloudImageY, colorPointCloudImageZ, dummy_output_image;
	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nColorFrameWidth, nColorFrameHeight,
		nColorFrameWidth * (int)sizeof(int16_t),
		&colorPointCloudImageX);
	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nColorFrameWidth, nColorFrameHeight,
		nColorFrameWidth * (int)sizeof(int16_t),
		&colorPointCloudImageY);
	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nColorFrameWidth, nColorFrameHeight,
		nColorFrameWidth * (int)sizeof(int16_t),
		&colorPointCloudImageZ);
	k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16, nColorFrameWidth, nColorFrameHeight,
		nColorFrameWidth * (int)sizeof(int16_t),
		&dummy_output_image);

	k4a_image_t depthPointCloudX, depthPointCloudY, depthPointCloudZ;

	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nDepthFrameWidth, nDepthFrameHeight,
		nDepthFrameWidth * (int)sizeof(int16_t),
		&depthPointCloudX);
	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nDepthFrameWidth, nDepthFrameHeight,
		nDepthFrameWidth * (int)sizeof(int16_t),
		&depthPointCloudY);
	k4a_image_create(K4A_IMAGE_FORMAT_CUSTOM16, nDepthFrameWidth, nDepthFrameHeight,
		nDepthFrameWidth * (int)sizeof(int16_t),
		&depthPointCloudZ);

	int16_t* depthPointCloudImage_buffer = (int16_t*)k4a_image_get_buffer(depthPointCloudImage);
	int16_t* depthPointCloudImageX_buffer = (int16_t*)k4a_image_get_buffer(depthPointCloudX);
	int16_t* depthPointCloudImageY_buffer = (int16_t*)k4a_image_get_buffer(depthPointCloudY);
	int16_t* depthPointCloudImageZ_buffer = (int16_t*)k4a_image_get_buffer(depthPointCloudZ);
	for (int i = 0; i < nDepthFrameWidth * nDepthFrameHeight; i++)
	{
		depthPointCloudImageX_buffer[i] = depthPointCloudImage_buffer[3 * i + 0];
		depthPointCloudImageY_buffer[i] = depthPointCloudImage_buffer[3 * i + 1];
		depthPointCloudImageZ_buffer[i] = depthPointCloudImage_buffer[3 * i + 2];
	}

	int test = 1 << 16;

	// Transforming per-depth-pixel point cloud to per-color-pixel point cloud
	k4a_transformation_depth_image_to_color_camera_custom(transformation, depthImage, depthPointCloudX,
		dummy_output_image, colorPointCloudImageX,
		K4A_TRANSFORMATION_INTERPOLATION_TYPE_LINEAR, 0);
	k4a_transformation_depth_image_to_color_camera_custom(transformation, depthImage, depthPointCloudY,
		dummy_output_image, colorPointCloudImageY,
		K4A_TRANSFORMATION_INTERPOLATION_TYPE_LINEAR, 0);
	k4a_transformation_depth_image_to_color_camera_custom(transformation, depthImage, depthPointCloudZ,
		dummy_output_image, colorPointCloudImageZ,
		K4A_TRANSFORMATION_INTERPOLATION_TYPE_LINEAR, 0);


	int16_t* pointCloudDataX = (int16_t*)k4a_image_get_buffer(colorPointCloudImageX);
	int16_t* pointCloudDataY = (int16_t*)k4a_image_get_buffer(colorPointCloudImageY);
	int16_t* pointCloudDataZ = (int16_t*)k4a_image_get_buffer(colorPointCloudImageZ);
	for (int i = 0; i < nColorFrameHeight; i++)
	{
		for (int j = 0; j < nColorFrameWidth; j++)
		{
			pCameraSpacePoints[j + i * nColorFrameWidth].X = pointCloudDataX[j + i * nColorFrameWidth + 0] / 1000.0f;
			pCameraSpacePoints[j + i * nColorFrameWidth].Y = pointCloudDataY[j + i * nColorFrameWidth + 1] / 1000.0f;
			pCameraSpacePoints[j + i * nColorFrameWidth].Z = pointCloudDataZ[j + i * nColorFrameWidth + 2] / 1000.0f;
		}
	}

	k4a_image_release(colorPointCloudImageX);
	k4a_image_release(colorPointCloudImageY);
	k4a_image_release(colorPointCloudImageZ);

	k4a_image_release(dummy_output_image);
	k4a_image_release(depthPointCloudX);
	k4a_image_release(depthPointCloudY);
	k4a_image_release(depthPointCloudZ);
}


void AzureKinectCapture::MapDepthFrameToColorSpace(UINT16 *pDepthInColorSpace)
{
	if (depthImageInColor == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16, nColorFrameWidth, nColorFrameHeight,
			nColorFrameWidth * (int)sizeof(uint16_t),
			&depthImageInColor);
	}

	k4a_transformation_depth_image_to_color_camera(transformation, depthImage, depthImageInColor);

	memcpy(pDepthInColorSpace, k4a_image_get_buffer(depthImageInColor), nColorFrameHeight * nColorFrameWidth * (int)sizeof(uint16_t));
}

void AzureKinectCapture::MapColorFrameToDepthSpace(RGB *pColorInDepthSpace)
{
	if (colorImageInDepth == NULL)
	{
		k4a_image_create(K4A_IMAGE_FORMAT_COLOR_BGRA32, nDepthFrameWidth, nDepthFrameHeight,
			nDepthFrameWidth * 4 * (int)sizeof(uint8_t),
			&colorImageInDepth);
	}

	k4a_transformation_color_image_to_depth_camera(transformation, depthImage, colorImage, colorImageInDepth);

	memcpy(pColorInDepthSpace, k4a_image_get_buffer(colorImageInDepth), nDepthFrameHeight * nDepthFrameWidth * 4 * (int)sizeof(uint8_t));
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

