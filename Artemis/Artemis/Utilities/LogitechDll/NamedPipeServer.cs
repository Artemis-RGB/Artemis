using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Artemis.Utilities.LogitechDll
{
    public class NamedPipeServer
    {
        public const uint DUPLEX = 0x00000003;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;

        public const int BUFFER_SIZE = 100;
        private SafeFileHandle clientHandle;
        public Client clientse;
        public int ClientType;
        private Thread listenThread;

        public string pipeName;

        public NamedPipeServer(string PName, int Mode)
        {
            pipeName = PName;
            ClientType = Mode; //0 Reading Pipe, 1 Writing Pipe
        }

        public event PipeDataReceivedEventHandler PipeDataReceived;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
            string pipeName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
            SafeFileHandle hNamedPipe,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int DisconnectNamedPipe(
            SafeFileHandle hNamedPipe);

        public void Start()
        {
            listenThread = new Thread(ListenForClients);
            listenThread.Start();
        }

        private void ListenForClients()
        {
            while (true)
            {
                clientHandle = CreateNamedPipe(pipeName, DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE,
                    0, IntPtr.Zero);
                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                var success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;

                clientse = new Client();
                clientse.handle = clientHandle;
                clientse.stream = new FileStream(clientse.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);

                if (ClientType == 0)
                {
                    var readThread = new Thread(Read);
                    readThread.Start();
                }
            }
        }

        private void Read()
        {
            //Client client = (Client)clientObj;
            //clientse.stream = new FileStream(clientse.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = null;
            var encoder = new ASCIIEncoding();

            while (true)
            {
                var bytesRead = 0;

                try
                {
                    buffer = new byte[BUFFER_SIZE];
                    bytesRead = clientse.stream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error has occurred
                    break;
                }

                //client has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                //if (this.MessageReceived != null)
                //    this.MessageReceived(clientse, encoder.GetString(buffer, 0, bytesRead));

                var ReadLength = 0;
                for (var i = 0; i < BUFFER_SIZE; i++)
                {
                    if (buffer[i].ToString("x2") != "cc")
                    {
                        ReadLength++;
                    }
                    else
                        break;
                }
                if (ReadLength > 0)
                {
                    var Rc = new byte[ReadLength];
                    Buffer.BlockCopy(buffer, 0, Rc, 0, ReadLength);
                    OnPipeDataReceived(new PipeDataReceivedEventArgs(encoder.GetString(Rc, 0, ReadLength)));

                    buffer.Initialize();
                }
            }

            //clean up resources
            clientse.stream.Close();
            clientse.handle.Close();
        }

        public void SendMessage(string message, Client client)
        {
            var encoder = new ASCIIEncoding();
            var messageBuffer = encoder.GetBytes(message);

            if (client.stream.CanWrite)
            {
                client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                client.stream.Flush();
            }
        }

        public void StopServer()
        {
            //clean up resources

            DisconnectNamedPipe(clientHandle);


            listenThread.Abort();
        }

        private void OnPipeDataReceived(PipeDataReceivedEventArgs e)
        {
            PipeDataReceived?.Invoke(this, e);
        }

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }
    }

    public delegate void PipeDataReceivedEventHandler(
        object sender, PipeDataReceivedEventArgs pipeDataReceivedEventArgs);

    public class PipeDataReceivedEventArgs
    {
        public PipeDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; set; }
    }
}