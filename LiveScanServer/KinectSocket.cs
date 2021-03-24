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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using OpenTK.Graphics;

namespace KinectServer
{
    public delegate void SocketChangedHandler();
    public delegate void SubOrdinateInitialized();
    public delegate void MasterRestarted();
    public delegate void RecievedSyncJackState();
    public delegate void StandAloneInitialized();

    public class KinectSocket
    {
        Socket oSocket;
        byte[] byteToSend = new byte[1];
        public bool bFrameCaptured = false;
        public bool bLatestFrameReceived = false;
        public bool bStoredFrameReceived = false;
        public bool bNoMoreStoredFrames = true;
        public bool bCalibrated = false;
        public bool bSubStarted = false;
        public bool bMasterStarted = false;

        public enum eTempSyncConfig { MASTER, SUBORDINATE, STANDALONE, UNKNOWN }

        //Shows how the *Device* is configured (Determined only by the Sync-Cables hooked up to the device)
        public eTempSyncConfig currentDeviceTempSyncState = eTempSyncConfig.STANDALONE; 

        //Shows how the Client-Software is configured (This is set by the server)
        public eTempSyncConfig currentClientTempSyncState = eTempSyncConfig.STANDALONE;

        //The pose of the sensor in the scene (used by the OpenGLWindow to show the sensor)
        public AffineTransform oCameraPose = new AffineTransform();

        //The transform that maps the vertices in the sensor coordinate system to the world corrdinate system.
        public AffineTransform oWorldTransform = new AffineTransform();

        public string sSocketState;

        public List<byte> lFrameRGB = new List<byte>();
        public List<Single> lFrameVerts = new List<Single>();
        public List<Body> lBodies = new List<Body>(); 

        public event SocketChangedHandler eChanged;
        public event SubOrdinateInitialized eSubInitialized;
        public event MasterRestarted eMasterRestart;
        public event RecievedSyncJackState eSyncJackstate;
        public event StandAloneInitialized eStandAloneInitialized;

        public KinectSocket(Socket clientSocket)
        {
            oSocket = clientSocket;
            sSocketState = oSocket.RemoteEndPoint.ToString() + " Calibrated = false";

            UpdateSocketState();
        }

        public void CaptureFrame()
        {
            bFrameCaptured = false;
            byteToSend[0] = 0;
            SendByte();
        }

        public void Calibrate()
        {            
            bCalibrated = false;
            sSocketState = oSocket.RemoteEndPoint.ToString() + " Calibrated = false";

            byteToSend[0] = 1;
            SendByte();

            UpdateSocketState();
        }

        public void SendSettings(KinectSettings settings)
        {
            List<byte> lData = settings.ToByteList();

            byte[] bTemp = BitConverter.GetBytes(lData.Count);
            lData.InsertRange(0, bTemp);
            lData.Insert(0, 2);

            if (SocketConnected())
                oSocket.Send(lData.ToArray());
        }

        public void RequestStoredFrame()
        {
            byteToSend[0] = 3;
            SendByte();
            bNoMoreStoredFrames = false;
            bStoredFrameReceived = false;
        }

        public void RequestLastFrame()
        {
            byteToSend[0] = 4;
            SendByte();
            bLatestFrameReceived = false;
        }

        public void SendCalibrationData()
        {
            int size = 1 + (9 + 3) * sizeof(float);
            byte[] data = new byte[size];
            int i = 0;

            data[i] = 5;
            i++;

            Buffer.BlockCopy(oWorldTransform.R, 0, data, i, 9 * sizeof(float));
            i += 9 * sizeof(float);
            Buffer.BlockCopy(oWorldTransform.t, 0, data, i, 3 * sizeof(float));
            i += 3 * sizeof(float);

            if (SocketConnected())
                oSocket.Send(data);
        }

        public void ClearStoredFrames()
        {
            byteToSend[0] = 6;
            SendByte();
        }

        /// <summary>
        /// Send the device the command to restart with Temporal Sync Enabled or Disabled
        /// </summary>
        /// <param name="tempSyncOn">Enable/Disable Temporal Sync</param>
        /// <param name="syncOffSetMultiplier">Only set when this device should be a subordinate. Should be a unique number in ascending order for each sub</param>
        public void SendTemporalSyncStatus(bool tempSyncOn, byte syncOffSetMultiplier)
        {
            bSubStarted = false;
            currentClientTempSyncState = eTempSyncConfig.UNKNOWN;

            if (tempSyncOn)
            {
                byte[] data = new byte[2];
                data[0] = 7;
                data[1] = syncOffSetMultiplier;
                if (SocketConnected())
                    oSocket.Send(data);
            }

            else
            {
                byteToSend[0] = 8;
                SendByte();
            }
        }

        /// <summary>
        /// Sends the master device the command to Start. Should only be called when all subs have started
        /// </summary>
        public void SendMasterInitialize()
        {
            byteToSend[0] = 9;
            SendByte();
        }


        /// <summary>
        /// Asks the client in which way the Sync-Cables are hooked up to the device
        /// </summary>
        public void RequestTempSyncState()
        {
            currentDeviceTempSyncState = eTempSyncConfig.UNKNOWN;
            byteToSend[0] = 10;
            SendByte();
        }

        public void RecieveDeviceSyncState()
        {
            byte tempSyncState;
            byte[] buffer = new byte[1024];

            while (oSocket.Available == 0)
            {
                if (!SocketConnected())
                    return;
            }

            oSocket.Receive(buffer, 3, SocketFlags.None);
            tempSyncState = buffer[0];

            if (tempSyncState == 0)
            {
                currentDeviceTempSyncState = eTempSyncConfig.SUBORDINATE;
            }

            else if (tempSyncState == 1)
            {
                currentDeviceTempSyncState = eTempSyncConfig.MASTER;
            }

            else if (tempSyncState == 2)
            {
                currentDeviceTempSyncState = eTempSyncConfig.STANDALONE;
            }

            eSyncJackstate.Invoke();
        }

        public void ReceiveCalibrationData()
        {
            bCalibrated = true;

            byte[] buffer = Receive(sizeof(int) * 1);
            //currently not used
            int markerId = BitConverter.ToInt32(buffer, 0);

            buffer = Receive(sizeof(float) * 9);
            Buffer.BlockCopy(buffer, 0, oWorldTransform.R, 0, sizeof(float) * 9);

            buffer = Receive(sizeof(float) * 3);
            Buffer.BlockCopy(buffer, 0, oWorldTransform.t, 0, sizeof(float) * 3);

            oCameraPose.R = oWorldTransform.R;
            for (int i = 0; i < 3; i++)
            {
                oCameraPose.t[i] = 0.0f;
                for (int j = 0; j < 3; j++)
                {
                    oCameraPose.t[i] += oWorldTransform.t[j] * oWorldTransform.R[i, j];
                }
            }

            UpdateSocketState();
        }

        //Gets the TemporalSyncStatus from the device
        public void ReceiveTemporalSyncStatus()
        {
            byte tempSyncState;
            byte[] buffer = new byte[1024];

            while (oSocket.Available == 0)
            {
                if (!SocketConnected())
                    return;
            }

            oSocket.Receive(buffer, 3, SocketFlags.None);
            tempSyncState = buffer[0];

            if (tempSyncState == 0)
            {
                currentClientTempSyncState = eTempSyncConfig.SUBORDINATE;
                bSubStarted = true;
                bMasterStarted = false;
                eSubInitialized?.Invoke();
            }

            else if(tempSyncState == 1)
            {
                currentClientTempSyncState = eTempSyncConfig.MASTER;
                bSubStarted = false;
            }

            else if (tempSyncState == 2)
            {
                currentClientTempSyncState = eTempSyncConfig.STANDALONE;
                bSubStarted = false;
                bMasterStarted = false;
                eStandAloneInitialized?.Invoke();
            }

            UpdateSocketState();

        }

        public void RecieveMasterHasRestarted()
        {
            bMasterStarted = true;
            eMasterRestart?.Invoke();
        }

        public void ReceiveFrame()
        {
            lFrameRGB.Clear();
            lFrameVerts.Clear();
            lBodies.Clear();

            int nToRead;
            byte[] buffer = new byte[1024];

            while (oSocket.Available == 0)
            {
                if (!SocketConnected())
                    return;
            }

            oSocket.Receive(buffer, 8, SocketFlags.None);
            nToRead = BitConverter.ToInt32(buffer, 0);



            int iCompressed = BitConverter.ToInt32(buffer, 4);

            if (nToRead == -1)
            {
                bNoMoreStoredFrames = true;
                return;
            }

            //Sometimes we recieve negative values when the cameras are restarting, I don't know why yet.
            //I'm just patching this up for now.s

            if (nToRead <= 0)
            {
                return;
            }

            buffer = new byte[nToRead];
            int nAlreadyRead = 0;

            while (nAlreadyRead != nToRead)
            {
                while (oSocket.Available == 0)
                {
                    if (!SocketConnected())
                        return;
                }

                nAlreadyRead += oSocket.Receive(buffer, nAlreadyRead, nToRead - nAlreadyRead, SocketFlags.None);
            }

            


            if (iCompressed == 1)
                buffer = ZSTDDecompressor.Decompress(buffer);

            //Receive depth and color data
            int startIdx = 0;

            int n_vertices = BitConverter.ToInt32(buffer, startIdx);
            startIdx += 4;

            for (int i = 0; i < n_vertices; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    lFrameRGB.Add(buffer[startIdx++]);
                }
                for (int j = 0; j < 3; j++)
                {
                    float val = BitConverter.ToInt16(buffer, startIdx);
                    //converting from milimeters to meters
                    val /= 1000.0f;
                    lFrameVerts.Add(val);
                    startIdx += 2;
                }
            }

            //Receive body data
            int nBodies = BitConverter.ToInt32(buffer, startIdx);
            startIdx += 4;
            for (int i = 0; i < nBodies; i++)
            {
                Body tempBody = new Body();
                tempBody.bTracked = BitConverter.ToBoolean(buffer, startIdx++);
                int nJoints = BitConverter.ToInt32(buffer, startIdx);
                startIdx += 4;

                tempBody.lJoints = new List<Joint>(nJoints);
                tempBody.lJointsInColorSpace = new List<Point2f>(nJoints);

                for (int j = 0; j < nJoints; j++)
                {
                    Joint tempJoint = new Joint();
                    Point2f tempPoint = new Point2f();

                    tempJoint.jointType = (JointType)BitConverter.ToInt32(buffer, startIdx);
                    startIdx += 4;
                    tempJoint.trackingState = (TrackingState)BitConverter.ToInt32(buffer, startIdx);
                    startIdx += 4;
                    tempJoint.position.X = BitConverter.ToSingle(buffer, startIdx);
                    startIdx += 4;
                    tempJoint.position.Y = BitConverter.ToSingle(buffer, startIdx);
                    startIdx += 4;
                    tempJoint.position.Z = BitConverter.ToSingle(buffer, startIdx);
                    startIdx += 4;

                    tempPoint.X = BitConverter.ToSingle(buffer, startIdx);
                    startIdx += 4;
                    tempPoint.Y = BitConverter.ToSingle(buffer, startIdx);
                    startIdx += 4;

                    tempBody.lJoints.Add(tempJoint);
                    tempBody.lJointsInColorSpace.Add(tempPoint);
                }

                lBodies.Add(tempBody);
            }
        }

        public byte[] Receive(int nBytes)
        {
            byte[] buffer;
            if (oSocket.Available != 0)
            {
                buffer = new byte[Math.Min(nBytes, oSocket.Available)];
                oSocket.Receive(buffer, nBytes, SocketFlags.None);
            }
            else
                buffer = new byte[0];

            return buffer;
        }

        public bool SocketConnected()
        {
            bool part1 = oSocket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (oSocket.Available == 0);

            if (part1 && part2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SendByte()
        {
            oSocket.Send(byteToSend);
        }

        public void UpdateSocketState()
        {
            string tempSyncMessage = "";

            switch (currentClientTempSyncState)
            {
                case eTempSyncConfig.MASTER:
                    tempSyncMessage = "[MASTER]";
                    break;
                case eTempSyncConfig.SUBORDINATE:
                    tempSyncMessage = "[SUBORDINATE]";
                    break;
                default:
                    break;
            }

            sSocketState = oSocket.RemoteEndPoint.ToString() + " Calibrated = " + bCalibrated + " " + tempSyncMessage;

            eChanged?.Invoke();
        }
    }
}
