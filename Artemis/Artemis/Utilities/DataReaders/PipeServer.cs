using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Ninject.Extensions.Logging;

namespace Artemis.Utilities.DataReaders
{
    // Delegate for passing received message back to caller
    public delegate void DelegateMessage(string reply);

    public class PipeServer
    {
        private readonly ILogger _logger;
        private string _pipeName;
        private NamedPipeServerStream _pipeServer;
        public event DelegateMessage PipeMessage;

        public PipeServer(ILogger logger)
        {
            _logger = logger;
        }

        public void Start(string pipeName)
        {
            _pipeName = pipeName;

            var security = new PipeSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));
            _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 254,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 4096, 4096, security);
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
            _logger.Info("Opened named pipe '{0}'", _pipeName);
        }

        public void Stop()
        {
            _pipeServer.Close();
            _pipeServer.Dispose();
            _logger.Info("Closed named pipe '{0}'", _pipeName);
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                var pipeServer = (NamedPipeServerStream) iar.AsyncState;
                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                var buffer = new byte[4096];

                // Read the incoming message
                pipeServer.Read(buffer, 0, 4096);

                // Convert byte buffer to string
                var stringData = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                // Pass message back to calling form
                PipeMessage?.Invoke(stringData);

                // Kill original sever and create new wait server
                pipeServer.Close();
                pipeServer = GetPipeServer(_pipeName);

                // Recursively wait for the connection again and again....
                _pipeServer = pipeServer;
                pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, pipeServer);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception in named pipe '{0}'", _pipeName);
                // ignored
            }
        }

        private static NamedPipeServerStream GetPipeServer(string name)
        {
            var security = new PipeSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));
            return new NamedPipeServerStream(name, PipeDirection.In, 254, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous, 4096, 4096, security);
        }
    }
}