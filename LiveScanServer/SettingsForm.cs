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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace KinectServer
{
    public partial class SettingsForm : Form
    {
        public KinectSettings oSettings;
        public KinectServer oServer;

        bool bFormLoaded = false;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtMinX.Text = oSettings.aMinBounds[0].ToString(CultureInfo.InvariantCulture);
            txtMinY.Text = oSettings.aMinBounds[1].ToString(CultureInfo.InvariantCulture);
            txtMinZ.Text = oSettings.aMinBounds[2].ToString(CultureInfo.InvariantCulture);

            txtMaxX.Text = oSettings.aMaxBounds[0].ToString(CultureInfo.InvariantCulture);
            txtMaxY.Text = oSettings.aMaxBounds[1].ToString(CultureInfo.InvariantCulture);
            txtMaxZ.Text = oSettings.aMaxBounds[2].ToString(CultureInfo.InvariantCulture);

            chFilter.Checked = oSettings.bFilter;
            txtFilterNeighbors.Text = oSettings.nFilterNeighbors.ToString();
            txtFilterDistance.Text = oSettings.fFilterThreshold.ToString(CultureInfo.InvariantCulture);

            lisMarkers.DataSource = oSettings.lMarkerPoses;

            chBodyData.Checked = oSettings.bStreamOnlyBodies;
            chSkeletons.Checked = oSettings.bShowSkeletons;

            cbCompressionLevel.SelectedText = oSettings.iCompressionLevel.ToString();

            chMerge.Checked = oSettings.bMergeScansForSave;
            txtICPIters.Text = oSettings.nNumICPIterations.ToString();
            txtRefinIters.Text = oSettings.nNumRefineIters.ToString();
            if (oSettings.bSaveAsBinaryPLY)
            {
                rBinaryPly.Checked = true;
                rAsciiPly.Checked = false;
            }
            else
            {
                rBinaryPly.Checked = false;
                rAsciiPly.Checked = true;
            }

            bFormLoaded = true;
        }

        void UpdateClients()
        {
            if (bFormLoaded)
                oServer.SendSettings();
        }

        void UpdateMarkerFields()
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];

                float X, Y, Z;
                pose.GetOrientation(out X, out Y, out Z);

                txtOrientationX.Text = X.ToString(CultureInfo.InvariantCulture);
                txtOrientationY.Text = Y.ToString(CultureInfo.InvariantCulture);
                txtOrientationZ.Text = Z.ToString(CultureInfo.InvariantCulture);

                txtTranslationX.Text = pose.pose.t[0].ToString(CultureInfo.InvariantCulture);
                txtTranslationY.Text = pose.pose.t[1].ToString(CultureInfo.InvariantCulture);
                txtTranslationZ.Text = pose.pose.t[2].ToString(CultureInfo.InvariantCulture);

                txtId.Text = pose.id.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                txtOrientationX.Text = "";
                txtOrientationY.Text = "";
                txtOrientationZ.Text = "";

                txtTranslationX.Text = "";
                txtTranslationY.Text = "";
                txtTranslationZ.Text = "";

                txtId.Text = "";
            }
        }

        private void txtMinX_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMinX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMinBounds[0]);
            UpdateClients();
        }

        private void txtMinY_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMinY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMinBounds[1]);
            UpdateClients();
        }

        private void txtMinZ_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMinZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMinBounds[2]);
            UpdateClients();
        }

        private void txtMaxX_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMaxX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMaxBounds[0]);
            UpdateClients();
        }

        private void txtMaxY_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMaxY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMaxBounds[1]);
            UpdateClients();
        }

        private void txtMaxZ_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtMaxZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.aMaxBounds[2]);
            UpdateClients();
        }

        private void chFilter_CheckedChanged(object sender, EventArgs e)
        {
            oSettings.bFilter = chFilter.Checked;
            UpdateClients();
        }

        private void txtFilterNeighbors_TextChanged(object sender, EventArgs e)
        {
            Int32.TryParse(txtFilterNeighbors.Text, out oSettings.nFilterNeighbors);
            UpdateClients();
        }

        private void txtFilterDistance_TextChanged(object sender, EventArgs e)
        {
            Single.TryParse(txtFilterDistance.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out oSettings.fFilterThreshold);
            UpdateClients();
        }

        private void txtICPIters_TextChanged(object sender, EventArgs e)
        {
            Int32.TryParse(txtICPIters.Text, out oSettings.nNumICPIterations);
        }

        private void txtRefinIters_TextChanged(object sender, EventArgs e)
        {
            Int32.TryParse(txtRefinIters.Text, out oSettings.nNumRefineIters);
        }

        private void chMerge_CheckedChanged(object sender, EventArgs e)
        {
            oSettings.bMergeScansForSave = chMerge.Checked;
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            lock (oSettings)
                oSettings.lMarkerPoses.Add(new MarkerPose());
            lisMarkers.SelectedIndex = oSettings.lMarkerPoses.Count - 1;
            UpdateMarkerFields();
            UpdateClients();
        }
        private void btRemove_Click(object sender, EventArgs e)
        {
            if (oSettings.lMarkerPoses.Count > 0)
            {
                oSettings.lMarkerPoses.RemoveAt(lisMarkers.SelectedIndex);
                lisMarkers.SelectedIndex = oSettings.lMarkerPoses.Count - 1;
                UpdateMarkerFields();
                UpdateClients();
            }
        }

        private void lisMarkers_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMarkerFields();
        }

        private void txtOrientationX_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                float X, Y, Z;
                pose.GetOrientation(out X, out Y, out Z);
                Single.TryParse(txtOrientationX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out X);

                pose.SetOrientation(X, Y, Z);
                UpdateClients();
            }
        }

        private void txtOrientationY_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                float X, Y, Z;
                pose.GetOrientation(out X, out Y, out Z);
                Single.TryParse(txtOrientationY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out Y);

                pose.SetOrientation(X, Y, Z);
                UpdateClients();
            }
        }

        private void txtOrientationZ_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                float X, Y, Z;
                pose.GetOrientation(out X, out Y, out Z);
                Single.TryParse(txtOrientationZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out Z);

                pose.SetOrientation(X, Y, Z);
                UpdateClients();
            }
        }

        private void txtTranslationX_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                float X;
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                Single.TryParse(txtTranslationX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out X);

                pose.pose.t[0] = X;
                UpdateClients();
            }
        }

        private void txtTranslationY_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                float Y;
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                Single.TryParse(txtTranslationY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out Y);

                pose.pose.t[1] = Y;
                UpdateClients();
            }
        }

        private void txtTranslationZ_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                float Z;
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                Single.TryParse(txtTranslationZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out Z);

                pose.pose.t[2] = Z;
                UpdateClients();
            }
        }

        private void txtId_TextChanged(object sender, EventArgs e)
        {
            if (lisMarkers.SelectedIndex >= 0)
            {
                int id;
                MarkerPose pose = oSettings.lMarkerPoses[lisMarkers.SelectedIndex];
                Int32.TryParse(txtId.Text, out id);

                pose.id = id;
                UpdateClients();
            }
        }

        private void chBodyData_CheckedChanged(object sender, EventArgs e)
        {
            oSettings.bStreamOnlyBodies = chBodyData.Checked;
            UpdateClients();
        }

        private void PlyFormat_CheckedChanged(object sender, EventArgs e)
        {
            if (rAsciiPly.Checked)
            {
                oSettings.bSaveAsBinaryPLY = false;
            }
            else
            {
                oSettings.bSaveAsBinaryPLY = true;
            }
        }

        private void chSkeletons_CheckedChanged(object sender, EventArgs e)
        {
            oSettings.bShowSkeletons = chSkeletons.Checked;
        }

        private void cbCompressionLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cbCompressionLevel.SelectedIndex;
            if (index == 0)
                oSettings.iCompressionLevel = 0;
            else if (index == 2)
                oSettings.iCompressionLevel = 2;
            else
            {
                string value = cbCompressionLevel.SelectedItem.ToString();
                bool tryParse = Int32.TryParse(value, out oSettings.iCompressionLevel);
                if (!tryParse)
                    oSettings.iCompressionLevel = 0;
            }
            UpdateClients();
        }
    }
}
