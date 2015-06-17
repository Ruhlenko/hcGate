using System;
using System.Net.Sockets;
using System.Text;

namespace hcGate
{
    class TcpClient
    {
        public Socket Socket;
        public byte[] AsyncBuffer = new byte[Defaults.IoBufferSize];
        public byte[] InputBuffer = new byte[Defaults.IoBufferSize];
        public int InputBufferSize = 0;
        public int ID = 0;
        public bool Valid = true;
        public bool AutoUpdate = false;

        public void SendData(int id, int data)
        {
            Send(String.Format("#{0:X8}{1:X4}\r", id, data));
        }

        public void SendCommand(int id, string cmd)
        {
            if (!Valid) return;
            if (ID == 0) return;
            if (id != 0 && id != ID) return;

            Send(String.Format("K{0:X2}{1}\r", id, cmd));
        }

        public void Send(string str)
        {
            Socket.Send(Encoding.ASCII.GetBytes(str));
        }
    }
}
