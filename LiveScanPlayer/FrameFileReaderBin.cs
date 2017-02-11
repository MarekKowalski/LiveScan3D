using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LiveScanPlayer
{
    class FrameFileReaderBin : IFrameFileReader
    {
        BinaryReader binaryReader;
        int currentFrameIdx = 0;
        string filename;

        public FrameFileReaderBin(string filename)
        {
            this.filename = filename;
            binaryReader = new BinaryReader(File.Open(this.filename, FileMode.Open));
        }

        ~FrameFileReaderBin()
        {
            binaryReader.Dispose();
        }

        public int frameIdx
        {
            get
            {
                return currentFrameIdx;
            }
            set
            {
                JumpToFrame(value);
            }
        }

        public void ReadFrame(List<float> vertices, List<byte> colors)
        {
            if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
                Rewind();

            string[] lineParts = ReadLine().Split(' ');
            int nPoints = Int32.Parse(lineParts[1]);
            lineParts = ReadLine().Split(' ');
            int frameTimestamp = Int32.Parse(lineParts[1]);

            Console.WriteLine((frameTimestamp / 1000).ToString());

            short[] tempVertices = new short[3 * nPoints];
            byte[] tempColors = new byte[4 * nPoints];

            int bytesPerVertexPoint = 3 * sizeof(short);
            int bytesPerColorPoint = 4 * sizeof(byte);
            int bytesPerPoint = bytesPerVertexPoint + bytesPerColorPoint;
            
            byte[] frameData = binaryReader.ReadBytes(bytesPerPoint * nPoints);

            if (frameData.Length < bytesPerPoint * nPoints)
            {
                Rewind();
                ReadFrame(vertices, colors);
                return;
            }

            int vertexDataSize = nPoints * bytesPerVertexPoint;
            int colorDataSize = nPoints * bytesPerColorPoint;
            Buffer.BlockCopy(frameData, 0, tempVertices, 0, vertexDataSize);
            Buffer.BlockCopy(frameData, vertexDataSize, tempColors, 0, colorDataSize);

            for (int i = 0; i < nPoints; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    vertices.Add(tempVertices[3 * i + j] / 1000.0f);
                    colors.Add(tempColors[4 * i + j]);
                }                    
            }

            binaryReader.ReadByte();

            currentFrameIdx++;
        }

        public void JumpToFrame(int frameIdx)
        {
            Rewind();
            for (int i = 0; i < frameIdx; i++)
            {
                List<float> vertices = new List<float>();
                List<byte> colors = new List<byte>();
                ReadFrame(vertices, colors);
            }
        }

        public void Rewind()
        {
            currentFrameIdx = 0;
            binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        public string ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            byte buffer = binaryReader.ReadByte();

            while (buffer != '\n')
            {
                builder.Append((char)buffer);
                buffer = binaryReader.ReadByte();
            }

            return builder.ToString();
        }
    }
}
