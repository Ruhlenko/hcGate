using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Timers;

namespace hcGate
{
    class ComPort
    {
        #region " Fields & Properties "

        SerialPort Port = new SerialPort();
        byte[] inpBuffer = new byte[Defaults.IoBufferSize];
        int inpBufferSize = 0;
        LinkedList<hcData> outQueue = new LinkedList<hcData>();

        public bool IsOpen
        {
            get { return Port.IsOpen; }
        }

        public string Name
        {
            get { return Port.PortName; }
            set { if (!Port.IsOpen) Port.PortName = value; }
        }

        public int BaudRate
        {
            get { return Port.BaudRate; }
            set { if (!Port.IsOpen) Port.BaudRate = value; }
        }

        public string Settings
        {
            get { return Port.BaudRate + " " + Port.DataBits + "," + Port.Parity + "," + Port.StopBits; }
        }

        #endregion

        #region " Constructor "

        public ComPort()
        {
            Port.DataBits = 8;
            Port.Parity = Parity.None;
            Port.StopBits = StopBits.One;
            Port.Handshake = Handshake.None;
            Port.DiscardNull = true;
            Port.NewLine = Defaults.ComPortDelimiter;

            Port.DataReceived += new SerialDataReceivedEventHandler(receiveCallback);

            WatchdogTimer = new Timer(Defaults.WatchdogTimerInterval);
            WatchdogTimer.AutoReset = false;
            WatchdogTimer.Elapsed += new ElapsedEventHandler(WatchdogTimer_Elapsed);
        }

        #endregion

        #region " Events "



        public event EventHandler<hcData> DataReceived;
        void OnDataReceived(int _id, int _data)
        {
            if (DataReceived != null)
                DataReceived(this, new hcData(_id, _data));
        }



        public event EventHandler<hcCommand> CommandReceived;
        void OnCommandReceived(byte _id, string _cmd)
        {
            if (CommandReceived != null)
                CommandReceived(this, new hcCommand(_id, _cmd));
        }



        public event EventHandler ResetReceived;
        void OnResetReceived()
        {
            if (ResetReceived != null)
                ResetReceived(this, EventArgs.Empty);
        }



        #endregion

        #region " Keep Alive "

        bool valid = false;
        public bool Valid { get { return valid; } }

        Timer WatchdogTimer;

        void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WatchdogTimer.Stop();
            valid = false;
            OnResetReceived();
        }

        void WatchdogTimer_Reset()
        {
            WatchdogTimer.Stop();
            if (IsOpen)
                WatchdogTimer.Start();
        }

        #endregion

        #region " Public Methods "

        public void Open()
        {
            valid = false;

            Port.Open();
            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
            inpBufferSize = 0;

            WatchdogTimer_Reset();
        }

        public void Close()
        {
            valid = false;
            WatchdogTimer.Stop();
            inpBufferSize = 0;

            Port.Close();

            lock (outQueue)
            {
                outQueue.Clear();
            }
        }

        public void SendData(int _id, int _val)
        {
            if (!valid) return;

            lock (outQueue)
            {
                hcData newEvent = new hcData(_id, _val);
                LinkedListNode<hcData> currEvent = outQueue.Find(newEvent);
                if (currEvent != null)
                {
                    currEvent.Value.Data = newEvent.Data;
                }
                else
                {
                    outQueue.AddLast(newEvent);
                }
            }
        }

        #endregion

        #region " Private Methods "



        void receiveCallback(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesRead = Port.BytesToRead;
            if (0 != bytesRead)
            {
                byte b;
                for (int i = 0; i < bytesRead; i++)
                {
                    b = (byte)Port.ReadByte();
                    switch (b)
                    {
                        case 0x0A: break;
                        case 0x0D:
                            parseReceived();
                            inpBufferSize = 0;
                            break;
                        default:
                            if (b > 0x20 && b < 0x7F)
                            {
                                inpBuffer[inpBufferSize++] = b;
                                if (inpBufferSize > inpBuffer.Length)
                                    inpBufferSize = 0;
                            }
                            break;
                    }
                }
            }
        }



        void parseReceived()
        {
            if (inpBufferSize < 4) return;

            if (CheckSum.Sum8(inpBuffer, inpBufferSize - 2) !=
                HexParser.GetByte(inpBuffer, inpBufferSize - 2))
                return;
            inpBufferSize -= 2;

            WatchdogTimer_Reset();

            switch ((char)inpBuffer[0])
            {
                case '$':
                    if (inpBufferSize == (2))
                    {
                        switch ((char)inpBuffer[1])
                        {
                            case 'T':
                                if (!valid)
                                {
                                    Send("!R");
                                    break;
                                }
                                hcData _evt = outQueue_Peek();
                                if (_evt == null)
                                {
                                    Send("!0");
                                }
                                else
                                {
                                    Send(String.Format("!{0:X4}{1:X4}", (Int16)_evt.ID, (Int16)_evt.Data));
                                }
                                break;

                            case 'C':
                                lock (outQueue)
                                {
                                    if (outQueue.Count > 0)
                                        outQueue.RemoveFirst();
                                }
                                break;

                            case 'R':
                                lock (outQueue)
                                {
                                    outQueue.Clear();
                                }
                                OnResetReceived();
                                Send("!");
                                valid = true;
                                break;
                        }
                    }
                    else if (inpBufferSize > (2 + 2))
                    {
                        if ((char)inpBuffer[1] == 'K')
                        {
                            byte _id = HexParser.GetByte(inpBuffer, 2);
                            if (0 != _id)
                            {
                                String _str = Encoding.ASCII.GetString(inpBuffer, 4, inpBufferSize - 4);
                                OnCommandReceived(_id, _str);
                            }
                        }
                    }
                    break;

                case '#':
                    if (inpBufferSize == (1 + 4 + 4))
                    {
                        int _id = HexParser.GetInt16(inpBuffer, 1);
                        int _val = HexParser.GetInt16(inpBuffer, 5);
                        OnDataReceived(_id, _val);
                        Send(">");
                    }
                    break;
            }
        }



        hcData outQueue_Peek()
        {
            hcData _hd = null;
            lock (outQueue)
            {
                LinkedListNode<hcData> firstEvt = outQueue.First;
                if (firstEvt != null)
                    _hd = firstEvt.Value;
            }
            return _hd;
        }



        void Send(string _str)
        {
            byte sum = CheckSum.Sum8(_str);
            Port.Write(_str + sum.ToString("X2") + Defaults.ComPortDelimiter);
        }


        
        #endregion
    }
}
