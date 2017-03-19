using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Artemis.Utilities
{
    public class GifImage
    {
        private readonly FrameDimension _dimension;
        private readonly Image _gifImage;
        private readonly PropertyItem _gifProperties;
        private DateTime _lastRequest;
        private int _step = 1;

        public GifImage(string path, double animationSpeed)
        {
            _lastRequest = DateTime.Now;
            _gifImage = Image.FromFile(path); //initialize
            _dimension = new FrameDimension(_gifImage.FrameDimensionsList[0]); //gets the GUID
            _gifProperties = _gifImage.GetPropertyItem(0x5100); // FrameDelay in libgdiplus

            Source = path;
            AnimationSpeed = animationSpeed;
            FrameCount = _gifImage.GetFrameCount(_dimension); //total frames in the animation
        }

        /// <summary>
        ///     Gets the path the GifImage is based on
        /// </summary>
        public string Source { get; private set; }

        public double AnimationSpeed { get; set; }

        /// <summary>
        ///     Gets or sets the current frame, set to -1 to reset
        /// </summary>
        public int CurrentFrame { get; set; } = -1;

        /// <summary>
        ///     Gets the total amount of frames in the GIF
        /// </summary>
        public int FrameCount { get; }

        /// <summary>
        ///     Whether the gif should play backwards when it reaches the end
        /// </summary>
        public bool ReverseAtEnd { get; set; }

        public Image GetNextFrame()
        {
            var animationSpeed = 2 - AnimationSpeed/3 * 2;
            var fileDelay = (_gifProperties.Value[0] + _gifProperties.Value[1] * 256) * 10;
            // If the file is missing this metadata such as in #319 then default to 100
            if (fileDelay == 0)
                fileDelay = 100;
            // Apply the animation speed to the delay
            var delay = fileDelay * animationSpeed;

            // Only pass the next frame if the proper amount of time has passed
            if ((DateTime.Now - _lastRequest).Milliseconds > delay)
            {
                CurrentFrame += _step;
                _lastRequest = DateTime.Now;
            }

            //if the animation reaches a boundary...
            if (CurrentFrame < FrameCount && CurrentFrame >= 1)
                return GetFrame(CurrentFrame);

            if (ReverseAtEnd)
            {
                _step *= -1; //...reverse the count
                CurrentFrame += _step; //apply it
            }
            else
                CurrentFrame = 0; //...or start over

            return GetFrame(CurrentFrame);
        }

        public Image GetFrame(int index)
        {
            _gifImage.SelectActiveFrame(_dimension, index); //find the frame
            return _gifImage;
        }
    }
}
