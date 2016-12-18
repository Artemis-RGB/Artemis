using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        // Vehicle & Track information

        private uint mlapsinevent; // [ RANGE = 0->... ]   [ UNSET = 0 ]
        private float mtracklength;
        private string mtracklocation;
        private string mtrackvariant;

        public uint mLapsInEvent
        {
            get { return mlapsinevent; }
            set
            {
                if (mlapsinevent == value)
                    return;
                SetProperty(ref mlapsinevent, value);
            }
        }

        public string mTrackLocation
        {
            get { return mtracklocation; }
            set
            {
                if (mtracklocation == value)
                    return;
                SetProperty(ref mtracklocation, value);
            }
        }

        public string mTrackVariant
        {
            get { return mtrackvariant; }
            set
            {
                if (mtrackvariant == value)
                    return;
                SetProperty(ref mtrackvariant, value);
            }
        }

        public float mTrackLength
        {
            get { return mtracklength; }
            set
            {
                if (mtracklength == value)
                    return;
                SetProperty(ref mtracklength, value);
            }
        }
    }
}