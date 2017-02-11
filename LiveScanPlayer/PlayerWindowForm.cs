using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using KinectServer;

namespace LiveScanPlayer
{
    public partial class PlayerWindowForm : Form
    {
        BindingList<IFrameFileReader> lFrameFiles = new BindingList<IFrameFileReader>();
        bool bPlayerRunning = false;

        List<float> lAllVertices = new List<float>();
        List<byte> lAllColors = new List<byte>();

        TransferServer oTransferServer = new TransferServer();

        AutoResetEvent eUpdateWorkerFinished = new AutoResetEvent(false);

        public PlayerWindowForm()
        {
            InitializeComponent();
           
            oTransferServer.lVertices = lAllVertices;
            oTransferServer.lColors = lAllColors;

            lFrameFilesListView.Columns.Add("Current frame", 75);
            lFrameFilesListView.Columns.Add("Filename", 300);
        }

        private void PlayerWindowForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bPlayerRunning = false;
            oTransferServer.StopServer();
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowDialog();

            lock (lFrameFiles)
            {
                for (int i = 0; i < dialog.FileNames.Length; i++)
                {                
                    lFrameFiles.Add(new FrameFileReaderBin(dialog.FileNames[i]));

                    var item = new ListViewItem(new [] { "0", dialog.FileNames[i]});
                    lFrameFilesListView.Items.Add(item);
                }
            }
        }

        private void btnSelectPly_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowDialog();

            if (dialog.FileNames.Length == 0)
                return;

            lock (lFrameFiles)
            {
                    lFrameFiles.Add(new FrameFileReaderPly(dialog.FileNames));
                    
                    
                    var item = new ListViewItem(new[] { "0", Path.GetDirectoryName(dialog.FileNames[0]) });
                    lFrameFilesListView.Items.Add(item);              
            }
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            bPlayerRunning = !bPlayerRunning;

            if (bPlayerRunning)
            {            
                oTransferServer.StartServer();
                updateWorker.RunWorkerAsync();
                btStart.Text = "Stop player";
            }
            else
            {
                oTransferServer.StopServer();
                btStart.Text = "Start player";
                eUpdateWorkerFinished.WaitOne();
            }
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            if (lFrameFilesListView.SelectedIndices.Count == 0)
                return;

            lock (lFrameFiles)
            {
                int idx = lFrameFilesListView.SelectedIndices[0];
                lFrameFilesListView.Items.RemoveAt(idx);
                lFrameFiles.RemoveAt(idx);
            }
        }

        private void btRewind_Click(object sender, EventArgs e)
        {
            lock (lFrameFiles)
            {
                for (int i = 0; i < lFrameFiles.Count; i++)
                {                    
                    lFrameFiles[i].Rewind();
                    lFrameFilesListView.Items[i].Text = "0";
                }
            }
        }

        private void btShow_Click(object sender, EventArgs e)
        {
            if (!OpenGLWorker.IsBusy)
                OpenGLWorker.RunWorkerAsync();
        }

        private void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int curFrameIdx = 0;
            string outDir = "outPlayer\\";
            DirectoryInfo di = Directory.CreateDirectory(outDir);

            while (bPlayerRunning)
            {
                Thread.Sleep(50);

                List<float> tempAllVertices = new List<float>();
                List<byte> tempAllColors = new List<byte>();

                lock (lFrameFiles)
                {
                    for (int i = 0; i < lFrameFiles.Count; i++)
                    {
                        List<float> vertices = new List<float>();
                        List<byte> colors = new List<byte>();
                        lFrameFiles[i].ReadFrame(vertices, colors);

                        tempAllVertices.AddRange(vertices);                        
                        tempAllColors.AddRange(colors);
                    }
                }

                Thread frameIdxUpdate = new Thread(() => this.Invoke((MethodInvoker)delegate { this.UpdateDisplayedFrameIndices(); }));
                frameIdxUpdate.Start();

                lock (lAllVertices)
                {
                    lAllVertices.Clear();
                    lAllColors.Clear();
                    lAllVertices.AddRange(tempAllVertices);
                    lAllColors.AddRange(tempAllColors);
                }

                if (chSaveFrames.Checked)
                    SaveCurrentFrameToFile(outDir, curFrameIdx);
                

                curFrameIdx++;
            }

            eUpdateWorkerFinished.Set();
        }

        private void OpenGLWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OpenGLWindow openGLWindow = new OpenGLWindow();

            openGLWindow.vertices = lAllVertices;
            openGLWindow.colors = lAllColors;

            openGLWindow.Run();
        }

        private void lFrameFilesListView_DoubleClick(object sender, EventArgs e)
        {
            lFrameFilesListView.SelectedItems[0].BeginEdit();
        }

        private void lFrameFilesListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int fileIdx = lFrameFilesListView.SelectedIndices[0];
            int frameIdx;
            bool res = Int32.TryParse(e.Label, out frameIdx);

            if (!res)
            {
                e.CancelEdit = true;
                return;
            }

            lock (lFrameFiles)
            {
                lFrameFiles[fileIdx].JumpToFrame(frameIdx);
            }

        }

        private void UpdateDisplayedFrameIndices()
        {
            lock (lFrameFiles)
            {
                for (int i = 0; i < lFrameFiles.Count; i++)
                {
                    lFrameFilesListView.Items[i].SubItems[0].Text = lFrameFiles[i].frameIdx.ToString();
                }
            }
        }

        private void SaveCurrentFrameToFile(string outDir, int frameIdx)
        {
            List<float> lVertices = new List<float>();
            List<byte> lColors = new List<byte>();

            lock (lAllVertices)
            {
                lVertices.AddRange(lAllVertices);
                lColors.AddRange(lAllColors);
            }
            string outputFilename = outDir + frameIdx.ToString().PadLeft(5, '0') + ".ply";
            Utils.saveToPly(outputFilename, lVertices, lColors, true);
        }
    }
}
