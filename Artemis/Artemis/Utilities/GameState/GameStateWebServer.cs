using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Utilities.GameState
{
    /// <summary>
    ///     Listens for JSON calls, parses them and raises an event.
    ///     Includes some code from https://github.com/rakijah/CSGSI
    /// </summary>
    public class GameStateWebServer
    {
        public delegate void GameDataReceivedEventHandler(
            object sender, GameDataReceivedEventArgs gameDataReceivedEventArgs);

        private readonly AutoResetEvent _waitForConnection = new AutoResetEvent(false);

        private HttpListener _listener;

        public GameStateWebServer()
        {
            Start();
        }

        public int Port { get; private set; }
        public bool Running { get; private set; }

        public event GameDataReceivedEventHandler GameDataReceived;

        public void Start()
        {
            if (Running)
                return;

            Port = General.Default.GamestatePort;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{Port}/");
            var listenerThread = new Thread(ListenerRun);
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                MessageBox.Show(
                    "Couldn't start the webserver. CS:GO/Dota2 effects won't work :c \n\nTry changing the port in Settings and restart Artemis.");
            }

            Running = true;
            listenerThread.Start();
        }

        private void ListenerRun()
        {
            while (Running)
            {
                _listener.BeginGetContext(HandleRequest, _listener);
                _waitForConnection.WaitOne();
                _waitForConnection.Reset();
            }
        }

        private void HandleRequest(IAsyncResult ar)
        {
            HttpListenerContext context = null;
            try
            {
                context = _listener.EndGetContext(ar);
            }
            catch (ObjectDisposedException)
            {
                // Listener was Closed due to call of Stop();
            }
            catch (HttpListenerException)
            {
                // Listener was Closed due to call of Stop();
            }
            finally
            {
                _waitForConnection.Set();
            }

            if (context != null)
            {
                HandleRequest(context.Request);
                context.Response.OutputStream.Close();
            }
        }

        public void Stop()
        {
            _listener.Close();
            Running = false;
        }

        private void HandleRequest(HttpListenerRequest request)
        {
            object json;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                var result = reader.ReadToEnd();
                json = JsonConvert.DeserializeObject<object>(result);
            }

            if (json != null)
                OnGameDataReceived(new GameDataReceivedEventArgs(json));
        }

        protected virtual void OnGameDataReceived(GameDataReceivedEventArgs e)
        {
            GameDataReceived?.Invoke(this, e);
        }
    }
}