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

namespace KinectServer
{
    [Serializable]
    public class AffineTransform
    {
        public float[,] R = new float[3, 3];
        public float[] t = new float[3];

        public AffineTransform()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == j)
                        R[i, j] = 1;
                    else
                        R[i, j] = 0;
                }
                t[i] = 0;
            }
        }
    }

    [Serializable]
    public class MarkerPose
    {
        public AffineTransform pose = new AffineTransform();
        public int id = -1;

        public MarkerPose()
        {
            UpdateRotationMatrix();
        }

        public void SetOrientation(float X, float Y, float Z)
        {
            r[0] = X;
            r[1] = Y;
            r[2] = Z;

            UpdateRotationMatrix();
        }

        public void GetOrientation(out float X, out float Y, out float Z)
        {
            X = r[0];
            Y = r[1];
            Z = r[2];
        }

        private void UpdateRotationMatrix()
        {
            float radX = r[0] * (float)Math.PI / 180.0f;
            float radY = r[1] * (float)Math.PI / 180.0f;
            float radZ = r[2] * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(radZ);
            float c2 = (float)Math.Cos(radY);
            float c3 = (float)Math.Cos(radX);
            float s1 = (float)Math.Sin(radZ);
            float s2 = (float)Math.Sin(radY);
            float s3 = (float)Math.Sin(radX);

            //Z Y X rotation
            pose.R[0, 0] = c1 * c2;
            pose.R[0, 1] = c1 * s2 * s3 - c3 * s1;
            pose.R[0, 2] = s1 * s3 + c1 * c3 * s2;
            pose.R[1, 0] = c2 * s1;
            pose.R[1, 1] = c1 * c3 + s1 * s2 * s3;
            pose.R[1, 2] = c3 * s1 * s2 - c1 * s3;
            pose.R[2, 0] = -s2;
            pose.R[2, 1] = c2 * s3;
            pose.R[2, 2] = c2 * c3;
        }

        private float[] r = new float[3];
    }
}