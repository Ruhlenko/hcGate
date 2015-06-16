using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;

namespace hcGate
{
    public class hcGate : ServiceBase
    {
        #region " Constructor "

        public hcGate()
        {
            this.ServiceName = Defaults.ServiceName;
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            AutoLog = true;

            _comPort.Name = Defaults.ComPortName;
            _comPort.BaudRate = Defaults.ComPortBaud;
            _comPort.DataReceived += new EventHandler<hcData>(Serial_DataReceived);
            _comPort.CommandReceived += new EventHandler<hcCommand>(Serial_CommandReceived);
            _comPort.ResetReceived += new EventHandler(Serial_Reset);

            _tcpPort = Defaults.TcpPort;
        }

        #endregion

        #region " Logging "

        private bool _consoleMode;
        public bool ConsoleMode
        {
            get { return _consoleMode; }
            set { _consoleMode = value; }
        }

        private void WriteLog(string message)
        {
            if (_consoleMode)
                Console.WriteLine(message);
            else
                EventLog.WriteEntry(message);
        }

        private void WriteLog(string message, EventLogEntryType type)
        {
            if (_consoleMode)
            {
                switch (type)
                {
                    case EventLogEntryType.Error:
                        Console.Write("[Error] ");
                        break;
                    case EventLogEntryType.Warning:
                        Console.Write("[Warning] ");
                        break;
                }
                Console.WriteLine(message);
            }
            else
                EventLog.WriteEntry(message, type);
        }

        #endregion

        #region " Control "

        private bool _autonomous = false;
        public bool Autonomous
        {
            get { return _autonomous; }
            set { _autonomous = value; }
        }

        public void ConsoleStart()
        {
            if (_consoleMode) WriteLog("Console mode");
            OnStart(null);
        }

        private void Terminate()
        {
            if (_consoleMode)
            {
                OnStop();
            }
            else
            {
                base.Stop();
            }
        }

        protected override void OnStart(string[] args)
        {
            startTcpServer();
            if (_tcpServerStarted)
                _tcpServerSocket.BeginAccept(new AsyncCallback(tcpAcceptCallback), _tcpServerSocket);

            if (!_autonomous) Serial_Open();
        }

        protected override void OnStop()
        {
            if (_comPort.IsOpen) _comPort.Close();
        }

        public void ReadSettings()
        {
            IniFile reader = new IniFile(Defaults.SettingsFileName);
            try
            {
                _comPort.Name = reader.ReadValue("Serial", "Port", Defaults.ComPortName);
                _comPort.BaudRate = Int32.Parse(reader.ReadValue("Serial", "Baudrate", Defaults.ComPortBaud.ToString()));

                _tcpPort = UInt16.Parse(reader.ReadValue("Network", "Port", Defaults.TcpPort.ToString()));
            }
            catch (Exception exc)
            {
                WriteLog(exc.Message, EventLogEntryType.Warning);
            }
        }

        #endregion

        #region " Serial "

        private ComPort _comPort = new ComPort();

        private void Serial_Open()
        {
            try
            {
                _comPort.Open();
                WriteLog(String.Format("{0} opened at {1}", _comPort.Name, _comPort.Settings));
            }
            catch (Exception e)
            {
                WriteLog("ComPort Error: " + e.Message, EventLogEntryType.Error);
                Terminate();
            }
        }

        private void Serial_DataReceived(object sender, hcData e)
        {
            if (_consoleMode)
                Console.WriteLine("From Serial: {0} <- {1}", e.ID, e.Data);

            _cache.Write(e.ID, e.Data);
            SendDataToAllClients(e.ID, e.Data);
        }

        private void Serial_CommandReceived(object sender, hcCommand e)
        {
            foreach (TcpClient client in _tcpClients)
                client.SendCommand(e.ID, e.Keys);
        }

        private void Serial_Reset(object sender, EventArgs e)
        {
            ResetAllData();
        }

        #endregion

        #region " Network "

        private int _tcpPort;
        private Socket _tcpServerSocket;
        private bool _tcpServerStarted = false;

        private List<TcpClient> _tcpClients = new List<TcpClient>();

        private void startTcpServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _tcpPort);
            try
            {
                _tcpServerSocket = new Socket(endPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _tcpServerSocket.Bind(endPoint);
                _tcpServerSocket.Listen((int)SocketOptionName.MaxConnections);
                _tcpServerStarted = true;

                WriteLog("TCP Server started on port " + endPoint.Port);
            }
            catch (SocketException exc)
            {
                WriteLog("TCPSetup.SocketException: " + exc.SocketErrorCode, EventLogEntryType.Error);
                Terminate();
            }
            catch (Exception exc)
            {
                WriteLog("TCPSetup.Exception: " + exc, EventLogEntryType.Error);
                Terminate();
            }
        }

        private void tcpAcceptCallback(IAsyncResult result)
        {
            TcpClient newTcpClient = new TcpClient();
            try
            {
                // Завершение операции Accept
                Socket s = (Socket)result.AsyncState;

                // Инициализация клиента
                newTcpClient.Socket = s.EndAccept(result);
                lock (_tcpClients)
                {
                    _tcpClients.Add(newTcpClient);
                }

                if (_consoleMode)
                    WriteLog("Client " + newTcpClient.Socket.RemoteEndPoint + " accepted");

                // Начало операции Receive
                newTcpClient.Socket.BeginReceive(
                    newTcpClient.AsyncBuffer, 0, newTcpClient.AsyncBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(tcpReceiveCallback), newTcpClient);

            }
            catch (SocketException exc)
            {
                tcpCloseConnection(newTcpClient);
                WriteLog("TcpAccept.SocketException: " + exc.SocketErrorCode, EventLogEntryType.Warning);
            }
            catch (Exception exc)
            {
                tcpCloseConnection(newTcpClient);
                WriteLog("TcpAccept.Exception: " + exc, EventLogEntryType.Warning);
            }
            finally
            {
                // Начало новой операции Accept
                _tcpServerSocket.BeginAccept(new AsyncCallback(tcpAcceptCallback), result.AsyncState);
            }
        }

        private void tcpReceiveCallback(IAsyncResult result)
        {
            TcpClient tcpClient = (TcpClient)result.AsyncState;
            try
            {
                var bytesRead = tcpClient.Socket.EndReceive(result);
                if (0 != bytesRead)
                {
                    byte b;
                    for (var i = 0; i < bytesRead; i++)
                    {
                        b = tcpClient.AsyncBuffer[i];
                        switch (b)
                        {
                            case 0x0A: break;
                            case 0x0D:
                                if (_consoleMode)
                                {
                                    Console.Write("From {0}>", tcpClient.Socket.RemoteEndPoint);
                                    for (var j = 0; j < tcpClient.InputBufferSize; j++)
                                        Console.Write((char)tcpClient.InputBuffer[j]);
                                    Console.WriteLine();
                                }

                                tcpParseReceived(tcpClient);
                                tcpClient.InputBufferSize = 0;
                                break;
                            default:
                                tcpClient.InputBuffer[tcpClient.InputBufferSize++] = b;
                                if (tcpClient.InputBufferSize > tcpClient.InputBuffer.Length)
                                    tcpClient.InputBufferSize = 0;
                                break;
                        }
                    }

                    tcpClient.Socket.BeginReceive(
                        tcpClient.AsyncBuffer, 0, tcpClient.AsyncBuffer.Length,
                        SocketFlags.None,
                        new AsyncCallback(tcpReceiveCallback), tcpClient);
                }
                else
                {
                    tcpCloseConnection(tcpClient);
                }
            }
            catch (SocketException)
            {
                tcpCloseConnection(tcpClient);
            }
            catch (Exception exc)
            {
                WriteLog("TCPReceive.Exception: " + exc, EventLogEntryType.Warning);
                tcpCloseConnection(tcpClient);
            }
        }

        private void tcpCloseConnection(TcpClient client)
        {
            if (client != null)
            {
                if (client.Socket != null)
                {
                    if (_consoleMode)
                        WriteLog("Connection with " + client.Socket.RemoteEndPoint + " closed");
                    client.Socket.Close();
                }
                lock (_tcpClients)
                {
                    _tcpClients.Remove(client);
                }
            }
        }

        private void tcpParseReceived(TcpClient client)
        {
            if (0 == client.InputBufferSize) return;

            switch ((char)client.InputBuffer[0])
            {
                case '%':
                    if (client.InputBufferSize == (1 + 2 + 1))
                    {
                        client.ID = HexParser.GetByte(client.InputBuffer, 1);
                        if (client.InputBuffer[3] == '1')
                        {
                            client.AutoUpdate = true;
                            SendAllDataToClient(client);
                        }
                        else
                        {
                            client.AutoUpdate = false;
                        }
                    }
                    break;
                case '#':
                    if (client.InputBufferSize == (1 + 8 + 4))
                    {
                        int id = HexParser.GetInt32(client.InputBuffer, 1);
                        int data = HexParser.GetInt16(client.InputBuffer, 9);
                        SendDataToPLC(id, data);
                    }
                    break;
                case '@':
                    if (client.InputBufferSize == (1 + 8))
                    {
                        int id = HexParser.GetInt32(client.InputBuffer, 1);
                        client.SendData(id, _cache.Read(id));
                    }
                    break;
                case 'K':
                    if (client.InputBufferSize > (1 + 2))
                    {
                        byte id = HexParser.GetByte(client.InputBuffer, 1);
                        String str = Encoding.ASCII.GetString(client.InputBuffer, 3, client.InputBufferSize - 3);
                        Serial_CommandReceived(this, new hcCommand(id, str));
                    }
                    break;
            }

        }

        #endregion

        #region " Data Cache "

        private hcCache<int> _cache;

        private void initCache()
        {
            int defaultData = (_autonomous ? 0 : -1);
            _cache = new hcCache<int>(Defaults.DataCacheSize, defaultData);
        }

        private void SendDataToPLC(int id, int data)
        {
            if (id == 0) return;

            if (_autonomous)
            {
                _cache.Write(id, data);
                SendDataToAllClients(id, data);
            }
            else
            {
                _comPort.SendData(id, data);
            }
        }

        private void SendDataToAllClients(int id, int data)
        {
            foreach (var client in _tcpClients)
            {
                if (client.Valid && client.AutoUpdate)
                    client.SendData(id, data);
            }
        }

        private void SendAllDataToClient(TcpClient client)
        {
            if (!(client.Valid && client.AutoUpdate)) return;

            for (var i = 0; i < _cache.Count; i++)
                client.SendData(i, _cache.Read(i));
        }

        private void ResetAllData()
        {
            _cache.Reset();

            foreach (TcpClient client in _tcpClients)
                SendAllDataToClient(client);
        }

        #endregion
    }
}
