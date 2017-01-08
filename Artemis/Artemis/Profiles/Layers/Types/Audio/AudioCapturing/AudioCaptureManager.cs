using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using Ninject.Extensions.Logging;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class AudioCaptureManager
    {
        private readonly List<AudioCapture> _audioCaptures;

        public AudioCaptureManager(ILogger logger)
        {
            Logger = logger;
            _audioCaptures = new List<AudioCapture>();
        }

        public AudioCapture GetAudioCapture(MMDevice device)
        {
            // Return existing audio capture if found
            var audioCapture = _audioCaptures.FirstOrDefault(a => a.Device == device);
            if (audioCapture != null)
                return audioCapture;

            // Else create a new one and return that
            var newAudioCapture = new AudioCapture(Logger, device);
            _audioCaptures.Add(newAudioCapture);
            return newAudioCapture;
        }

        public ILogger Logger { get; set; }
    }
}