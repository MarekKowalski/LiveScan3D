using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectServer
{
    public class TransferSocket
    {
        TcpClient oSocket;

        public TransferSocket(TcpClient clientSocket)
        {
            oSocket = clientSocket;
        }

        public byte[] Receive(int nBytes)
        {
            byte[] buffer;
            if (oSocket.Available != 0)
            {
                buffer = new byte[Math.Min(nBytes, oSocket.Available)];
                oSocket.GetStream().Read(buffer, 0, nBytes);
            }
            else
                buffer = new byte[0];

            return buffer;
        }

        public bool SocketConnected()
        {
            return oSocket.Connected;
        }

        public void WriteInt(int val)
        {
            oSocket.GetStream().Write(BitConverter.GetBytes(val), 0, 4);
        }

        public void WriteFloat(float val)
        {
            oSocket.GetStream().Write(BitConverter.GetBytes(val), 0, 4);
        }

        public void SendFrame(List<float> vertices, List<byte> colors)
        {
            short[] sVertices = Array.ConvertAll(vertices.ToArray(), x => (short)(x * 1000));

            
            int nVerticesToSend = vertices.Count / 3;
            byte[] buffer = new byte[sizeof(short) * 3 * nVerticesToSend];
            Buffer.BlockCopy(sVertices, 0, buffer, 0, sizeof(short) * 3 * nVerticesToSend);
            try
            {                 
                WriteInt(nVerticesToSend);                               
                oSocket.GetStream().Write(buffer, 0, buffer.Length);
                oSocket.GetStream().Write(colors.ToArray(), 0, sizeof(byte) * 3 * nVerticesToSend);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
