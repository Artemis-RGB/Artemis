using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Artemis.Utilities
{
    public class GifImage
    {
        private readonly int _delay;
        private readonly FrameDimension _dimension;
        private readonly Image _gifImage;
        private DateTime _lastRequest;
        private int _step = 1;

        public GifImage(string path)
        {
            _lastRequest = DateTime.Now;
            _gifImage = Image.FromFile(path); //initialize
            _dimension = new FrameDimension(_gifImage.FrameDimensionsList[0]); //gets the GUID
            FrameCount = _gifImage.GetFrameCount(_dimension); //total frames in the animation

            Source = path;

            var item = _gifImage.GetPropertyItem(0x5100); // FrameDelay in libgdiplus
            _delay = (item.Value[0] + item.Value[1]*256)*10; // Time is in 1/100th of a second
        }

        /// <summary>
        ///     Gets the path the GifImage is based on
        /// </summary>
        public string Source { get; private set; }

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
            // Only pass the next frame if the proper amount of time has passed
            if ((DateTime.Now - _lastRequest).Milliseconds > _delay)
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