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
using System.Threading;

using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Windows.Forms;

namespace KinectServer
{
    public delegate void SocketListChangedHandler(List<KinectSocket> list);
    public class KinectServer
    {
        Socket oServerSocket;

        bool bServerRunning = false;
        bool bWaitForSubToStart = false;

        //This lock prevents the user from enabeling/disabling the Temp Sync State while the cameras are in transition to another state.
        //When starting the server, all devices are already initialized, as the LiveScanClient can only connect with an initialized device
        bool allDevicesInitialized = true; 

        KinectSettings oSettings;
        SettingsForm fSettingsForm;
        MainWindowForm fMainWindowForm;
        object oClientSocketLock = new object();
        object oFrameRequestLock = new object();

        List<KinectSocket> lClientSockets = new List<KinectSocket>();

        public event SocketListChangedHandler eSocketListChanged;

        public int nClientCount
        {
            get
            {
                int nClients;
                lock (oClientSocketLock)
                {
                    nClients = lClientSockets.Count;
                }
                return nClients;
            }
        }

        public List<AffineTransform> lCameraPoses
        {
            get 
            {
                List<AffineTransform> cameraPoses = new List<AffineTransform>();
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        cameraPoses.Add(lClientSockets[i].oCameraPose);
                    }                    
                }
                return cameraPoses;
            }
            set
            {
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        lClientSockets[i].oCameraPose = value[i];
                    }
                }
            }
        }

        public List<AffineTransform> lWorldTransforms
        {
            get
            {
                List<AffineTransform> worldTransforms = new List<AffineTransform>();
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        worldTransforms.Add(lClientSockets[i].oWorldTransform);
                    }
                }
                return worldTransforms;
            }

            set
            {
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        lClientSockets[i].oWorldTransform = value[i];
                    }
                }
            }
        }

        public bool bAllCalibrated
        {
            get
            {
                bool allCalibrated = true;
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        if (!lClientSockets[i].bCalibrated)
                        {
                            allCalibrated = false;
                            break;
                        }
                    }
                    
                }
                return allCalibrated;
            }
        }

        public KinectServer(KinectSettings settings)
        {
            this.oSettings = settings;
        }

        public void SetSettingsForm(SettingsForm settings)
        {
            fSettingsForm = settings;
        }

        public void SetMainWindowForm(MainWindowForm main)
        {
            fMainWindowForm = main;
        }

        public SettingsForm GetSettingsForm()
        {
            return fSettingsForm;
        }

        private void SocketListChanged()
        {
            if (eSocketListChanged != null)
            {
                eSocketListChanged(lClientSockets);
            }
        }

        public void StartServer()
        {
            if (!bServerRunning)
            {
                oServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                oServerSocket.Blocking = false;

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 48001);
                oServerSocket.Bind(endPoint);
                oServerSocket.Listen(10);

                bServerRunning = true;
                Thread listeningThread = new Thread(this.ListeningWorker);
                listeningThread.Start();
                Thread receivingThread = new Thread(this.ReceivingWorker);
                receivingThread.Start();
            }
        }

        public void StopServer()
        {
            if (bServerRunning)
            {
                bServerRunning = false;

                oServerSocket.Close();
                lock (oClientSocketLock)
                    lClientSockets.Clear();
            }
        }

        public void CaptureSynchronizedFrame()
        {
            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                    lClientSockets[i].CaptureFrame();
            }

            //Wait till frames captured
            bool allGathered = false;
            while (!allGathered)
            {
                allGathered = true;

                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        if (!lClientSockets[i].bFrameCaptured)
                        {
                            allGathered = false;
                            break;
                        }
                    }
                }
            }
        }

        public void Calibrate()
        {
            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].Calibrate();
                }
            }
        }

        public void SendSettings()
        {
            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].SendSettings(oSettings);
                }
            }
        }

        public void SendCalibrationData()
        {
            lock (oClientSocketLock)
            {

                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].SendCalibrationData();
                }
            }
        }

        /// <summary>
        /// Request the device Sync state, so that we know which Device is Master and which are Subordinates before starting them
        /// </summary>
        public void RequestDeviceSyncState()
        {
            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].RequestTempSyncState();
                }
            }
        }

        /// <summary>
        /// When a client has send its Device Sync State, we try to Set the Client Sync State
        /// </summary>
        public void SendTemporalSyncData()
        {
            lock (oClientSocketLock)
            {
                int masterCount = 0;
                int subordinateCount = 0;

                //First we check if we have recieved the Device Sync State of all Devices
                //If not, we return and the next client who confirms their state starts this function again
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    if(lClientSockets[i].currentDeviceTempSyncState == KinectSocket.eTempSyncConfig.MASTER)
                    {
                        masterCount++;
                    }

                    if (lClientSockets[i].currentDeviceTempSyncState == KinectSocket.eTempSyncConfig.SUBORDINATE)
                    {
                        subordinateCount++;
                    }

                    if (lClientSockets[i].currentDeviceTempSyncState == KinectSocket.eTempSyncConfig.UNKNOWN)
                    {
                        return;
                    }
                }

                //On a second step we check if we have exactly one master and at least one subordinate

                if(masterCount != 1 || subordinateCount < 1)
                {
                    //If not, we show a error message and disable the temporal sync

                    fMainWindowForm?.SetStatusBarOnTimer("Temporal Sync cables not connected properly", 5000);
                    fSettingsForm?.ActivateTempSyncEnableButton();
                    return;
                }
                

                allDevicesInitialized = false;

                byte syncOffSetCounter = 0;

                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    if(lClientSockets[i].currentDeviceTempSyncState == KinectSocket.eTempSyncConfig.SUBORDINATE)
                    {
                        syncOffSetCounter++;
                    }

                    lClientSockets[i].SendTemporalSyncStatus(true, syncOffSetCounter);

                }

                bWaitForSubToStart = true;
            }
        }


        /// <summary>
        /// Sets all clients as standalone
        /// </summary>
        public void DisableTemporalSync()
        {
            allDevicesInitialized = false;

            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].SendTemporalSyncStatus(false, 0);
                }
            }
        }


        public void ConfirmTemporalSyncDisabled()
        {
            if (bWaitForSubToStart)
                return;

            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    if (lClientSockets[i].currentClientTempSyncState != KinectSocket.eTempSyncConfig.STANDALONE)
                    {
                        return;
                    }
                }
            }

            allDevicesInitialized = true;
        }

        /// <summary>
        /// Called when a sub client has started. This checks if all sub clients have already started. If yes, we initialize the master
        /// </summary>
        public void CheckForMasterStart()
        {
            if (!bWaitForSubToStart)
            {
                return;
            }

            bool allSubsStarted = true;

            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    if (!lClientSockets[i].bSubStarted && lClientSockets[i].currentClientTempSyncState == KinectSocket.eTempSyncConfig.SUBORDINATE)
                    {
                        allSubsStarted = false;
                        break;
                    }
                }

                bWaitForSubToStart = false;

                if (allSubsStarted)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        if(lClientSockets[i].currentDeviceTempSyncState == KinectSocket.eTempSyncConfig.MASTER)
                        {
                            lClientSockets[i].SendMasterInitialize();
                            return;
                        }
                    }
                }
            }           
        }

        /// <summary>
        /// Tells the server that it is now ok to start recieving user changes again
        /// </summary>
        public void MasterSuccessfullyRestarted()
        {
            allDevicesInitialized = true; 
        }

        public bool GetAllDevicesInitialized()
        {
            return allDevicesInitialized;
        }

        public bool GetStoredFrame(List<List<byte>> lFramesRGB, List<List<Single>> lFramesVerts)
        {
            bool bNoMoreStoredFrames;
            lFramesRGB.Clear();
            lFramesVerts.Clear();
            
            lock (oFrameRequestLock)
            {
                //Request frames
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                        lClientSockets[i].RequestStoredFrame();
                }

                //Wait till frames received
                bool allGathered = false;
                bNoMoreStoredFrames = false;
                while (!allGathered)
                {
                    allGathered = true;                
                    lock (oClientSocketLock)
                    {
                        for (int i = 0; i < lClientSockets.Count; i++)
                        {
                            if (!lClientSockets[i].bStoredFrameReceived)
                            {
                                allGathered = false;
                                break;
                            }

                            if (lClientSockets[i].bNoMoreStoredFrames)
                                bNoMoreStoredFrames = true;
                        }
                    }
                }

                //Store received frames
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        lFramesRGB.Add(new List<byte>(lClientSockets[i].lFrameRGB));
                        lFramesVerts.Add(new List<Single>(lClientSockets[i].lFrameVerts));
                    }
                }
            }

            if (bNoMoreStoredFrames)
                return false;
            else
                return true;
        }

        public void GetLatestFrame(List<List<byte>> lFramesRGB, List<List<Single>> lFramesVerts, List<List<Body>> lFramesBody)
        {
            lFramesRGB.Clear();
            lFramesVerts.Clear();
            lFramesBody.Clear();

            lock (oFrameRequestLock)
            {
                //Request frames
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                        lClientSockets[i].RequestLastFrame();
                }

                //Wait till frames received
                bool allGathered = false;

                while (!allGathered)
                {
                    allGathered = true;

                    lock (oClientSocketLock)
                    {
                        for (int i = 0; i < lClientSockets.Count; i++)
                        {
                            if (!lClientSockets[i].bLatestFrameReceived)
                            {
                                allGathered = false;
                                break;
                            }
                        }
                    }

                }

                //Store received frames
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        lFramesRGB.Add(new List<byte>(lClientSockets[i].lFrameRGB));
                        lFramesVerts.Add(new List<Single>(lClientSockets[i].lFrameVerts));
                        lFramesBody.Add(new List<Body>(lClientSockets[i].lBodies));
                    }
                }

            }
        }

        public void ClearStoredFrames()
        {
            lock (oClientSocketLock)
            {
                for (int i = 0; i < lClientSockets.Count; i++)
                {
                    lClientSockets[i].ClearStoredFrames();
                }
            }
        }

        private void ListeningWorker()
        {
            while (bServerRunning)
            {
                try
                {
                    Socket newClient = oServerSocket.Accept();

                    //we do not want to add new clients while a frame is being requested
                    lock (oFrameRequestLock)
                    {
                        lock (oClientSocketLock)
                        {
                            lClientSockets.Add(new KinectSocket(newClient));
                            lClientSockets[lClientSockets.Count - 1].SendSettings(oSettings);
                            lClientSockets[lClientSockets.Count - 1].eChanged += new SocketChangedHandler(SocketListChanged);
                            lClientSockets[lClientSockets.Count - 1].eSubInitialized += new SubOrdinateInitialized(CheckForMasterStart);
                            lClientSockets[lClientSockets.Count - 1].eMasterRestart += new MasterRestarted(MasterSuccessfullyRestarted);
                            lClientSockets[lClientSockets.Count - 1].eSyncJackstate += new RecievedSyncJackState(SendTemporalSyncData);
                            lClientSockets[lClientSockets.Count - 1].eStandAloneInitialized += new StandAloneInitialized(ConfirmTemporalSyncDisabled);

                            if (eSocketListChanged != null)
                            {
                                eSocketListChanged(lClientSockets);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                }
                System.Threading.Thread.Sleep(100);
            }

            if (eSocketListChanged != null)
            {
                eSocketListChanged(lClientSockets);
            }
        }

        private void ReceivingWorker()
        {
            System.Timers.Timer checkConnectionTimer = new System.Timers.Timer();
            checkConnectionTimer.Interval = 1000;

            checkConnectionTimer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        if (!lClientSockets[i].SocketConnected())
                        {
                            lClientSockets.RemoveAt(i);
                            if (eSocketListChanged != null)
                            {
                                eSocketListChanged(lClientSockets);
                            }
                            continue;
                        }
                    }
                }
            };

            checkConnectionTimer.Start();

            while (bServerRunning)
            {
                lock (oClientSocketLock)
                {
                    for (int i = 0; i < lClientSockets.Count; i++)
                    {
                        byte[] buffer = lClientSockets[i].Receive(1);

                        while (buffer.Length != 0)
                        {
                            if (buffer[0] == 0)
                            {
                                lClientSockets[i].bFrameCaptured = true;
                            }
                            else if (buffer[0] == 1)
                            {
                                lClientSockets[i].ReceiveCalibrationData();
                            }
                            //stored frame
                            else if (buffer[0] == 2)
                            {
                                lClientSockets[i].ReceiveFrame();
                                lClientSockets[i].bStoredFrameReceived = true;
                            }
                            //last frame
                            else if (buffer[0] == 3)
                            {
                                lClientSockets[i].ReceiveFrame();
                                lClientSockets[i].bLatestFrameReceived = true;
                            }

                            else if (buffer[0] == 4)
                            {
                                lClientSockets[i].ReceiveTemporalSyncStatus();
                            }

                            else if(buffer[0] == 5)
                            {
                                lClientSockets[i].RecieveMasterHasRestarted();
                            }

                            else if(buffer[0] == 6)
                            {
                                lClientSockets[i].RecieveDeviceSyncState();
                            }

                            buffer = lClientSockets[i].Receive(1);
                        }
                    }
                }

                Thread.Sleep(10);
            }

            checkConnectionTimer.Stop();
        }
    }
}
