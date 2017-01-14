using System;
using CSCore.CoreAudioAPI;

namespace Artemis.Events
{
    public class AudioDeviceChangedEventArgs : EventArgs
    {
        public AudioDeviceChangedEventArgs(MMDevice defaultPlayback, MMDevice defaultRecording)
        {
            DefaultPlayback = defaultPlayback;
            DefaultRecording = defaultRecording;
        }

        public MMDevice DefaultPlayback { get; }
        public MMDevice DefaultRecording { get; }
    }
}