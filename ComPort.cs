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

        private SerialPort _port = new SerialPort();
        private byte[] _buffer = new byte[Defaults.IoBufferSize];
        private int _bufferSize = 0;
        private LinkedList<hcData> _outQueue = new LinkedList<hcData>();

        public bool IsOpen
        {
            get { return _port.IsOpen; }
        }

        public string Name
        {
            get { return _port.PortName; }
            set { if (!_port.IsOpen) _port.PortName = value; }
        }

        public int BaudRate
        {
            get { return _port.BaudRate; }
            set { if (!_port.IsOpen) _port.BaudRate = value; }
        }

        public string Settings
        {
            get { return _port.BaudRate + " " + _port.DataBits + "," + _port.Parity + "," + _port.StopBits; }
        }

        #endregion

        #region " Constructor "

        public ComPort()
        {
            _port.DataBits = 8;
            _port.Parity = Parity.None;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.DiscardNull = true;
            _port.NewLine = Defaults.ComPortDelimiter;

            _port.DataReceived += new SerialDataReceivedEventHandler(receiveCallback);

            _watchdogTimer = new Timer(Defaults.WatchdogTimerInterval);
            _watchdogTimer.AutoReset = false;
            _watchdogTimer.Elapsed += new ElapsedEventHandler(WatchdogTimer_Elapsed);
        }

        #endregion

        #region " Events "

        public event EventHandler<hcData> DataReceived;
        void OnDataReceived(int id, int data)
        {
            if (DataReceived != null)
                DataReceived(this, new hcData(id, data));
        }

        public event EventHandler<hcCommand> CommandReceived;
        void OnCommandReceived(byte id, string cmd)
        {
            if (CommandReceived != null)
                CommandReceived(this, new hcCommand(id, cmd));
        }

        public event EventHandler ResetReceived;
        void OnResetReceived()
        {
            if (ResetReceived != null)
                ResetReceived(this, EventArgs.Empty);
        }

        #endregion

        #region " Keep Alive "

        private bool _valid = false;
        public bool Valid { get { return _valid; } }

        Timer _watchdogTimer;

        void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _watchdogTimer.Stop();
            _valid = false;
            OnResetReceived();
        }

        void WatchdogTimer_Reset()
        {
            _watchdogTimer.Stop();
            if (IsOpen)
                _watchdogTimer.Start();
        }

        #endregion

        #region " Public Methods "

        public void Open()
        {
            _valid = false;

            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            _bufferSize = 0;

            WatchdogTimer_Reset();
        }

        public void Close()
        {
            _valid = false;
            _watchdogTimer.Stop();
            _bufferSize = 0;

            _port.Close();

            lock (_outQueue)
            {
                _outQueue.Clear();
            }
        }

        public void SendData(int id, int val)
        {
            if (!_valid) return;

            lock (_outQueue)
            {
                hcData newEvent = new hcData(id, val);
                LinkedListNode<hcData> currEvent = _outQueue.Find(newEvent);
                if (currEvent != null)
                {
                    currEvent.Value.Data = newEvent.Data;
                }
                else
                {
                    _outQueue.AddLast(newEvent);
                }
            }
        }

        #endregion

        #region " Private Methods "

        void receiveCallback(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesRead = _port.BytesToRead;
            if (0 != bytesRead)
            {
                byte b;
                for (var i = 0; i < bytesRead; i++)
                {
                    b = (byte)_port.ReadByte();
                    if (b == 0x0D)
                    {
                        parseReceived();
                        _bufferSize = 0;
                    }
                    else if (b > 0x20 && b < 0x7F)
                    {
                        _buffer[_bufferSize++] = b;
                        if (_bufferSize > _buffer.Length)
                            _bufferSize = 0;
                    }
                }
            }
        }

        void parseReceived()
        {
            if (_bufferSize < 4) return;

            if (CheckSum.Sum8(_buffer, _bufferSize - 2) !=
                HexParser.GetByte(_buffer, _bufferSize - 2))
                return;
            _bufferSize -= 2;

            WatchdogTimer_Reset();

            switch ((char)_buffer[0])
            {
                case '$':
                    if (_bufferSize == (2))
                    {
                        switch ((char)_buffer[1])
                        {
                            case 'T':
                                if (!_valid)
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
                                lock (_outQueue)
                                {
                                    if (_outQueue.Count > 0)
                                        _outQueue.RemoveFirst();
                                }
                                break;

                            case 'R':
                                lock (_outQueue)
                                {
                                    _outQueue.Clear();
                                }
                                OnResetReceived();
                                Send("!");
                                _valid = true;
                                break;
                        }
                    }
                    else if (_bufferSize > (2 + 2))
                    {
                        if ((char)_buffer[1] == 'K')
                        {
                            byte _id = HexParser.GetByte(_buffer, 2);
                            if (0 != _id)
                            {
                                String _str = Encoding.ASCII.GetString(_buffer, 4, _bufferSize - 4);
                                OnCommandReceived(_id, _str);
                            }
                        }
                    }
                    break;

                case '#':
                    if (_bufferSize == (1 + 4 + 4))
                    {
                        int _id = HexParser.GetInt16(_buffer, 1);
                        int _val = HexParser.GetInt16(_buffer, 5);
                        OnDataReceived(_id, _val);
                        Send(">");
                    }
                    break;
            }
        }

        hcData outQueue_Peek()
        {
            hcData _hd = null;
            lock (_outQueue)
            {
                LinkedListNode<hcData> firstEvt = _outQueue.First;
                if (firstEvt != null)
                    _hd = firstEvt.Value;
            }
            return _hd;
        }

        void Send(string _str)
        {
            byte sum = CheckSum.Sum8(_str);
            _port.Write(_str + sum.ToString("X2") + Defaults.ComPortDelimiter);
        }

        #endregion
    }
}
