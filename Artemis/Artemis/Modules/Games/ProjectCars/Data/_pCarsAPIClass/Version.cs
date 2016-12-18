using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private uint mbuildversion; // [ RANGE = 0->... ]
        private uint mversion; // [ RANGE = 0->... ]

        public uint mVersion
        {
            get { return mversion; }
            set
            {
                if (mversion == value)
                    return;
                SetProperty(ref mversion, value);
            }
        }

        public uint mBuildVersion
        {
            get { return mbuildversion; }
            set
            {
                if (mbuildversion == value)
                    return;
                SetProperty(ref mbuildversion, value);
            }
        }
    }
}