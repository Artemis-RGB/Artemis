using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Artemis.Utilities
{
    internal class ImageUtilities
    {
        private static Dictionary<string, ImageCodecInfo> encoders;

        /// <summary>
        ///     A lock to prevent concurrency issues loading the encoders.
        /// </summary>
        private static readonly object encodersLock = new object();

        /// <summary>
        ///     A quick lookup for getting image encoders
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand
            get
            {
                //if the quick lookup isn't initialised, initialise it
                if (encoders == null)
                {
                    //protect against concurrency issues
                    lock (encodersLock)
                    {
                        //check again, we might not have been the first person to acquire the lock (see the double checked lock pattern)
                        if (encoders == null)
                        {
                            encoders = new Dictionary<string, ImageCodecInfo>();

                            //get all the codecs
                            foreach (var codec in ImageCodecInfo.GetImageEncoders())
                            {
                                //add each codec to the quick lookup
                                encoders.Add(codec.MimeType.ToLower(), codec);
                            }
                        }
                    }
                }

                //return the lookup
                return encoders;
            }
        }

        /// <summary>
        ///     Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //a holder for the result
            var result = new Bitmap(width, height);
            //set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        /// <summary>
        ///     Saves an image as a jpeg image, with the given quality
        /// </summary>
        /// <param name="path">Path to which the image would be saved.</param>
        /// <param name="quality">
        ///     An integer from 0 to 100, with 100 being the
        ///     highest quality
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An invalid value was entered for image quality.
        /// </exception>
        public static void SaveJpeg(string path, Image image, int quality)
        {
            //ensure the quality is within the correct range
            if ((quality < 0) || (quality > 100))
            {
                //create the error message
                var error =
                    string.Format(
                        "Jpeg image quality must be between 0 and 100, with 100 being the highest quality.  A value of {0} was specified.",
                        quality);
                //throw a helpful exception
                throw new ArgumentOutOfRangeException(error);
            }

            //create an encoder parameter for the image quality
            var qualityParam = new EncoderParameter(Encoder.Quality, quality);
            //get the jpeg codec
            var jpegCodec = GetEncoderInfo("image/jpeg");

            //create a collection of all parameters that we will pass to the encoder
            var encoderParams = new EncoderParameters(1);
            //set the quality parameter for the codec
            encoderParams.Param[0] = qualityParam;
            //save the image using the codec and the parameters
            image.Save(path, jpegCodec, encoderParams);
        }

        /// <summary>
        ///     Returns the image codec with the given mime type
        /// </summary>
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //do a case insensitive search for the mime type
            var lookupKey = mimeType.ToLower();

            //the codec to return, default to null
            ImageCodecInfo foundCodec = null;

            //if we have the encoder, get it to return
            if (Encoders.ContainsKey(lookupKey))
            {
                //pull the codec from the lookup
                foundCodec = Encoders[lookupKey];
            }

            return foundCodec;
        }
    }
}