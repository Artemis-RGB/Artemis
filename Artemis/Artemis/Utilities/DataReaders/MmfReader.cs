using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Artemis.Utilities.DataReaders
{
    /// <summary>
    ///     A helper class for reading memory managed files
    /// </summary>
    public class MmfReader
    {
        private DateTime _lastFailure;

        public MmfReader(string mmfName)
        {
            MmfName = mmfName;
        }

        public string MmfName { get; set; }

        /// <summary>
        ///     Turns the MMF into an color array
        /// </summary>
        /// <returns></returns>
        public Color[,] GetColorArray()
        {
            var mffString = ReadMmf(MmfName);
            if (string.IsNullOrEmpty(mffString))
                return null;
            var intermediateArray = mffString.Split('|');
            if (intermediateArray[0] == "1" || intermediateArray.Length < 2)
                return null;
            var array = intermediateArray[1].Substring(1).Split(' ');
            if (!array.Any())
                return null;

            try
            {
                var colors = new Color[6, 22];
                foreach (var intermediate in array)
                {
                    if (intermediate.Length > 16)
                        continue;
                    
                    // Can't parse to a byte directly since it may contain values >254
                    var parts = intermediate.Split(',').Select(int.Parse).ToArray();
                    if (parts[0] >= 5 && parts[1] >= 21)
                        continue;

                    colors[parts[0], parts[1]] = Color.FromRgb((byte) parts[2], (byte) parts[3], (byte) parts[4]);
                }
                return colors;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Reads the contents of the given MFF into a string
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ReadMmf(string fileName)
        {
            // Don't read the file within one second after failing
            //if (DateTime.Now - _lastFailure > new TimeSpan(0, 0, 1))
            //    return null;

            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(fileName))
                {
                    using (var stream = mmf.CreateViewStream())
                    {
                        using (var binReader = new BinaryReader(stream))
                        {
                            var allBytes = binReader.ReadBytes((int) stream.Length);
                            return Encoding.UTF8.GetString(allBytes, 0, allBytes.Length);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                _lastFailure = DateTime.Now;
                return null;
                //ignored
            }
        }
    }
}