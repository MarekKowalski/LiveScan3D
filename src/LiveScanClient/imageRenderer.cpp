//------------------------------------------------------------------------------
// <copyright file="ImageRenderer.cpp" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#include "stdafx.h"
#include "ImageRenderer.h"

/// <summary>
/// Constructor
/// </summary>
ImageRenderer::ImageRenderer() : 
    m_hWnd(0),
    m_sourceWidth(0),
    m_sourceHeight(0),
    m_sourceStride(0),
    m_pD2DFactory(NULL), 
    m_pRenderTarget(NULL),
    m_pBitmap(0)
{
}

/// <summary>
/// Destructor
/// </summary>
ImageRenderer::~ImageRenderer()
{
    DiscardResources();
    SafeRelease(m_pD2DFactory);
}

/// <summary>
/// Ensure necessary Direct2d resources are created
/// </summary>
/// <returns>indicates success or failure</returns>
HRESULT ImageRenderer::EnsureResources()
{
    HRESULT hr = S_OK;

    if (NULL == m_pRenderTarget)
    {
        D2D1_SIZE_U size = D2D1::SizeU(m_sourceWidth, m_sourceHeight);

        D2D1_RENDER_TARGET_PROPERTIES rtProps = D2D1::RenderTargetProperties();
        rtProps.pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE);
        rtProps.usage = D2D1_RENDER_TARGET_USAGE_GDI_COMPATIBLE;

        // Create a hWnd render target, in order to render to the window set in initialize
        hr = m_pD2DFactory->CreateHwndRenderTarget(
            rtProps,
            D2D1::HwndRenderTargetProperties(m_hWnd, size),
            &m_pRenderTarget
        );

        if (FAILED(hr))
        {
            return hr;
        }

		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(0.27f, 0.75f, 0.27f), &m_pBrushJointTracked);

		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Yellow, 1.0f), &m_pBrushJointInferred);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 1.0f), &m_pBrushBoneTracked);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Gray, 1.0f), &m_pBrushBoneInferred);

		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 0.5f), &m_pBrushHandClosed);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 0.5f), &m_pBrushHandOpen);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Blue, 0.5f), &m_pBrushHandLasso);

        // Create a bitmap that we can copy image data into and then render to the target
        hr = m_pRenderTarget->CreateBitmap(
            size, 
            D2D1::BitmapProperties(D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE)),
            &m_pBitmap 
        );

        if (FAILED(hr))
        {
            SafeRelease(m_pRenderTarget);
            return hr;
        }
    }

    return hr;
}

/// <summary>
/// Dispose of Direct2d resources 
/// </summary>
void ImageRenderer::DiscardResources()
{
    SafeRelease(m_pRenderTarget);
    SafeRelease(m_pBitmap);
}

/// <summary>
/// Set the window to draw to as well as the video format
/// Implied bits per pixel is 32
/// </summary>
/// <param name="hWnd">window to draw to</param>
/// <param name="pD2DFactory">already created D2D factory object</param>
/// <param name="sourceWidth">width (in pixels) of image data to be drawn</param>
/// <param name="sourceHeight">height (in pixels) of image data to be drawn</param>
/// <param name="sourceStride">length (in bytes) of a single scanline</param>
/// <returns>indicates success or failure</returns>
HRESULT ImageRenderer::Initialize(HWND hWnd, ID2D1Factory* pD2DFactory, int sourceWidth, int sourceHeight, int sourceStride)
{
    if (NULL == pD2DFactory)
    {
        return E_INVALIDARG;
    }

    m_hWnd = hWnd;

    // One factory for the entire application so save a pointer here
    m_pD2DFactory = pD2DFactory;

    m_pD2DFactory->AddRef();

    // Get the frame size
    m_sourceWidth  = sourceWidth;
    m_sourceHeight = sourceHeight;
    m_sourceStride = sourceStride;

    return S_OK;
}

/// <summary>
/// Draws a 32 bit per pixel image of previously specified width, height, and stride to the associated hwnd
/// </summary>
/// <param name="pImage">image data in RGBX format</param>
/// <param name="cbImage">size of image data in bytes</param>
/// <param name="vBodies">vector of bodies to draw</param>
/// <returns>indicates success or failure</returns>
HRESULT ImageRenderer::Draw(BYTE* pImage, unsigned long cbImage, std::vector<Body> &vBodies)
{
    // incorrectly sized image data passed in
    if (cbImage < ((m_sourceHeight - 1) * m_sourceStride) + (m_sourceWidth * 4))
    {
        return E_INVALIDARG;
    }

    // create the resources for this draw device
    // they will be recreated if previously lost
    HRESULT hr = EnsureResources();

    if (FAILED(hr))
    {
        return hr;
    }
    
    // Copy the image that was passed in into the direct2d bitmap
    hr = m_pBitmap->CopyFromMemory(NULL, pImage, m_sourceStride);

    if (FAILED(hr))
    {
        return hr;
    }
       
    m_pRenderTarget->BeginDraw();

    // Draw the bitmap stretched to the size of the window
    m_pRenderTarget->DrawBitmap(m_pBitmap);
      
	for (unsigned int i = 0; i < vBodies.size(); i++)
	{
		if (vBodies[i].bTracked)
		{
			DrawBody(vBodies[i]);
		}
	}

    hr = m_pRenderTarget->EndDraw();

    // Device lost, need to recreate the render target
    // We'll dispose it now and retry drawing
    if (hr == D2DERR_RECREATE_TARGET)
    {
        hr = S_OK;
        DiscardResources();
    }

    return hr;
}

/// <summary>
/// Draws all the body to the associated hwnd, assumes that BeginDraw has been called
/// </summary>
/// <param name="body">body to be drawn</param>
void ImageRenderer::DrawBody(Body &body)
{
	// Draw the bones

	// Torso
	DrawBone(body, JointType_Head, JointType_Neck);
	DrawBone(body, JointType_Neck, JointType_SpineShoulder);
	DrawBone(body, JointType_SpineShoulder, JointType_SpineMid);
	DrawBone(body, JointType_SpineMid, JointType_SpineBase);
	DrawBone(body, JointType_SpineShoulder, JointType_ShoulderRight);
	DrawBone(body, JointType_SpineShoulder, JointType_ShoulderLeft);
	DrawBone(body, JointType_SpineBase, JointType_HipRight);
	DrawBone(body, JointType_SpineBase, JointType_HipLeft);

	// Right Arm    
	DrawBone(body, JointType_ShoulderRight, JointType_ElbowRight);
	DrawBone(body, JointType_ElbowRight, JointType_WristRight);
	DrawBone(body, JointType_WristRight, JointType_HandRight);
	DrawBone(body, JointType_HandRight, JointType_HandTipRight);
	DrawBone(body, JointType_WristRight, JointType_ThumbRight);

	// Left Arm
	DrawBone(body, JointType_ShoulderLeft, JointType_ElbowLeft);
	DrawBone(body, JointType_ElbowLeft, JointType_WristLeft);
	DrawBone(body, JointType_WristLeft, JointType_HandLeft);
	DrawBone(body, JointType_HandLeft, JointType_HandTipLeft);
	DrawBone(body, JointType_WristLeft, JointType_ThumbLeft);

	// Right Leg
	DrawBone(body, JointType_HipRight, JointType_KneeRight);
	DrawBone(body, JointType_KneeRight, JointType_AnkleRight);
	DrawBone(body, JointType_AnkleRight, JointType_FootRight);

	// Left Leg
	DrawBone(body, JointType_HipLeft, JointType_KneeLeft);
	DrawBone(body, JointType_KneeLeft, JointType_AnkleLeft);
	DrawBone(body, JointType_AnkleLeft, JointType_FootLeft);

	for (unsigned int i = 0; i < body.vJoints.size(); i++)
	{
		D2D1_POINT_2F tempPoint;
		tempPoint.x = body.vJointsInColorSpace[i].X;
		tempPoint.y = body.vJointsInColorSpace[i].Y;

		D2D1_ELLIPSE ellipse = D2D1::Ellipse(tempPoint, 6.0f, 6.0f);

		if (body.vJoints[i].TrackingState == TrackingState_Inferred)
		{
			m_pRenderTarget->FillEllipse(ellipse, m_pBrushJointInferred);
		}
		else if (body.vJoints[i].TrackingState == TrackingState_Tracked)
		{
			m_pRenderTarget->FillEllipse(ellipse, m_pBrushJointTracked);
		}
	}
}

/// <summary>
/// Draws one bone of a body (joint to joint)
/// </summary>
/// <param name="body">body to be drawn</param>
/// <param name="joint0">one joint of the bone to draw</param>
/// <param name="joint1">other joint of the bone to draw</param>
void ImageRenderer::DrawBone(Body &body, JointType joint0, JointType joint1)
{
	TrackingState joint0State = body.vJoints[joint0].TrackingState;
	TrackingState joint1State = body.vJoints[joint1].TrackingState;

	// If we can't find either of these joints, exit
	if ((joint0State == TrackingState_NotTracked) || (joint1State == TrackingState_NotTracked))
	{
		return;
	}

	// Don't draw if both points are inferred
	if ((joint0State == TrackingState_Inferred) && (joint1State == TrackingState_Inferred))
	{
		return;
	}

	D2D1_POINT_2F joint0Point, joint1Point;
	joint0Point.x = body.vJointsInColorSpace[joint0].X;
	joint0Point.y = body.vJointsInColorSpace[joint0].Y;
	joint1Point.x = body.vJointsInColorSpace[joint1].X;
	joint1Point.y = body.vJointsInColorSpace[joint1].Y;
	// We assume all drawn bones are inferred unless BOTH joints are tracked
	if ((joint0State == TrackingState_Tracked) && (joint1State == TrackingState_Tracked))
	{
		m_pRenderTarget->DrawLine(joint0Point, joint1Point, m_pBrushBoneTracked, 10.0f);
	}
	else
	{
		m_pRenderTarget->DrawLine(joint0Point, joint1Point, m_pBrushBoneInferred, 3);
	}
}