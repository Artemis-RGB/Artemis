using System;
using System.Timers;
using SKYPE4COMLib;

namespace Artemis86Wrapper
{
    public class SkypeManager
    {
        private readonly Timer _loopTimer;

        public SkypeManager()
        {
            _loopTimer = new Timer(5000);
            _loopTimer.Elapsed += UpdateSkype;
        }

        public Skype Skype { get; set; }

        public void Start()
        {
            Skype = new Skype();
            _loopTimer.Start();
        }

        public void Stop()
        {
            _loopTimer.Stop();
            Skype = null;
        }

        private void UpdateSkype(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Missed messages: " + Skype.Messages.Count);
        }
    }
}