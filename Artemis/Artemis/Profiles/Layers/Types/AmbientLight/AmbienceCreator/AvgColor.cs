namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public class AvgColor
    {
        #region Properties & Fields

        private int _rCount = 0;
        private int _r = 0;
        private int _gCount = 0;
        private int _g = 0;
        private int _bCount = 0;
        private int _b = 0;

        public byte R => (byte)(_r / _rCount);
        public byte G => (byte)(_g / _gCount);
        public byte B => (byte)(_b / _bCount);

        #endregion

        #region Methods

        public void AddR(byte r)
        {
            _r += r;
            _rCount++;
        }

        public void AddG(byte g)
        {
            _g += g;
            _gCount++;
        }

        public void AddB(byte b)
        {
            _b += b;
            _bCount++;
        }

        #endregion
    }
}
