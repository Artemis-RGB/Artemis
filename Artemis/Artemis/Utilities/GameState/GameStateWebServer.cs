using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Utilities.GameState
{
    public class GameStateWebServer
    {
        public delegate void GameDataReceivedEventHandler(
            object sender, GameDataReceivedEventArgs gameDataReceivedEventArgs);

        private readonly HttpListener _listener = new HttpListener();

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

            try
            {
                _listener.Prefixes.Clear();
                Port = General.Default.GamestatePort;
                _listener.Prefixes.Add($"http://localhost:{Port}/");

                _listener.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't start the webserver. CS:GO effect won't work :c \n\nTry changing the port in Settings and restart Artemis.");
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            if (ctx == null)
                                return;
                            try
                            {
                                var rstr = HandleRequest(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch
                {
                    // ignored
                }
            });

            Running = true;
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private string HandleRequest(HttpListenerRequest request)
        {
            object json;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                var result = reader.ReadToEnd();
                json = JsonConvert.DeserializeObject<object>(result);
            }

            if (json != null)
                OnGameDataReceived(new GameDataReceivedEventArgs(json));
            return JsonConvert.SerializeObject(json);
        }

        protected virtual void OnGameDataReceived(GameDataReceivedEventArgs e)
        {
            GameDataReceived?.Invoke(this, e);
        }
    }
}