using System.Diagnostics;
using NamedPipeWrapper;

namespace Artemis.Utilities.LogitechDll
{
    public class LogitechNamedPipe
    {
        public LogitechNamedPipe()
        {
            LogitechPipe = new NamedPipeServer<string>("ArtemisLogitech");

            LogitechPipe.ClientMessage += LogitechPipeOnClientMessage;
            LogitechPipe.Start();
        }

        public NamedPipeServer<string> LogitechPipe { get; set; }

        private void LogitechPipeOnClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Debug.WriteLine(message);
        }
    }
}