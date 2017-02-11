using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LiveScanPlayer
{
    class FrameFileReaderPly : IFrameFileReader
    {
        string[] filenames;
        int currentFrameIdx = 0;

        public FrameFileReaderPly(string[] filenames)
        {
            this.filenames = filenames;
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
            BinaryReader reader = new BinaryReader(new FileStream(filenames[currentFrameIdx], FileMode.Open));

            string line = ReadLine(reader);
            while (!line.Contains("element vertex"))
                line = ReadLine(reader);
            string[] lineElems = line.Split(' ');
            int nPoints = Int32.Parse(lineElems[2]);
            while (!line.Contains("end_header"))
                line = ReadLine(reader);

            for (int i = 0; i < nPoints; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    vertices.Add(reader.ReadSingle());
                }
                for (int j = 0; j < 3; j++)
                {
                    colors.Add(reader.ReadByte());
                }
            }

            reader.Dispose();

            currentFrameIdx++;
            if (currentFrameIdx >= filenames.Length)
                currentFrameIdx = 0;
        }

        public void JumpToFrame(int frameIdx)
        {
            currentFrameIdx = frameIdx;
            if (currentFrameIdx >= filenames.Length)
                currentFrameIdx = 0;
        }

        public void Rewind()
        {
            currentFrameIdx = 0;
        }

        public string ReadLine(BinaryReader binaryReader)
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
