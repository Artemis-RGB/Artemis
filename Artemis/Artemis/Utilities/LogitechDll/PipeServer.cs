using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Utilities.LogitechDll
{
    // Delegate for passing received message back to caller
    public delegate void DelegateMessage(string reply);

    public class PipeServer
    {
        private string _pipeName;

        public bool Running { get; set; }
        public event DelegateMessage PipeMessage;

        public void Start(string pipeName)
        {
            Running = true;
            _pipeName = pipeName;
            var task = new Task(PipeLoop);
            task.Start();
        }

        public void Stop()
        {
            Running = false;
        }

        private void PipeLoop()
        {
            try
            {
                var security = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl,
                    AccessControlType.Allow));

                while (Running)
                {
                    var namedPipeServerStream = new NamedPipeServerStream(_pipeName, PipeDirection.In, 254,
                        PipeTransmissionMode.Byte, PipeOptions.None, 254, 254, security);

                    namedPipeServerStream.WaitForConnection();
                    var buffer = new byte[254];
                    namedPipeServerStream.Read(buffer, 0, 254);
                    namedPipeServerStream.Close();

                    var task = new Task(() => HandleMessage(buffer));
                    task.Start();
                }
            }
            catch
            {
                // ignored
            }
        }

        private void HandleMessage(byte[] buffer)
        {
            var request = Encoding.ASCII.GetString(buffer);
            Debug.WriteLine(request);
            PipeMessage?.Invoke(request);
        }

        public void Listen(string pipeName)
        {
            try
            {
                var security = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));

                // Set to class level var so we can re-use in the async callback method
                _pipeName = pipeName;
                // Create the new async pipe 
                var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, 254, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous, 254, 254, security);

                // Wait for a connection
                pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, pipeServer);
            }
            catch (Exception oEx)
            {
                Debug.WriteLine(oEx.Message);
            }
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                var pipeServer = (NamedPipeServerStream) iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                var buffer = new byte[255];

                // Read the incoming message
                pipeServer.Read(buffer, 0, 255);

                // Convert byte buffer to string
                var stringData = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Debug.WriteLine(stringData + Environment.NewLine);

                // Pass message back to calling form
                PipeMessage?.Invoke(stringData);

                // Kill original sever and create new wait server
                pipeServer.Close();

                var security = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));

                pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 254, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous, 254, 254, security);

                // Recursively wait for the connection again and again....
                pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, pipeServer);
            }
            catch
            {
                // ignored
            }
        }
    }
}