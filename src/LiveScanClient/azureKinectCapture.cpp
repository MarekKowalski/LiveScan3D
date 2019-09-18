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
	config.color_resolution = K4A_COLOR_RESOLUTION_1080P;
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
	k4a_image_release(depthImage);


	colorImage = k4a_capture_get_color_image(capture);
	depthImage = k4a_capture_get_depth_image(capture);
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