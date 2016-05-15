using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Artemis.Utilities
{
    public class GifImage
    {
        private readonly int _delay;
        private readonly FrameDimension _dimension;
        private readonly int _frameCount;
        private readonly Image _gifImage;
        private int _currentFrame = -1;
        private DateTime _lastRequest;
        private int _step = 1;

        public GifImage(string path)
        {
            _lastRequest = DateTime.Now;
            _gifImage = Image.FromFile(path); //initialize
            _dimension = new FrameDimension(_gifImage.FrameDimensionsList[0]); //gets the GUID
            _frameCount = _gifImage.GetFrameCount(_dimension); //total frames in the animation

            var item = _gifImage.GetPropertyItem(0x5100); // FrameDelay in libgdiplus
            _delay = (item.Value[0] + item.Value[1]*256)*10; // Time is in 1/100th of a second
        }

        /// <summary>
        ///     Whether the gif should play backwards when it reaches the end
        /// </summary>
        public bool ReverseAtEnd { get; set; }

        public Image GetNextFrame()
        {
            // Only pass the next frame if the proper amount of time has passed
            if ((DateTime.Now - _lastRequest).Milliseconds > _delay)
            {
                _currentFrame += _step;
                _lastRequest = DateTime.Now;
            }

            //if the animation reaches a boundary...
            if (_currentFrame < _frameCount && _currentFrame >= 1)
                return GetFrame(_currentFrame);

            if (ReverseAtEnd)
            {
                _step *= -1; //...reverse the count
                _currentFrame += _step; //apply it
            }
            else
                _currentFrame = 0; //...or start over

            return GetFrame(_currentFrame);
        }

        public Image GetFrame(int index)
        {
            _gifImage.SelectActiveFrame(_dimension, index); //find the frame
            return _gifImage;
        }
    }
}