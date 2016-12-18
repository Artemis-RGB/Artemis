using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private eFlagColors mflagcolour;
        private eFlagReason mflagreason;

        public eFlagColors mFlagColour
        {
            get { return mflagcolour; }
            set
            {
                if (mflagcolour == value)
                    return;
                SetProperty(ref mflagcolour, value);
            }
        } // [ enum (Type#5) Flag Colour ]

        public eFlagReason mFlagReason
        {
            get { return mflagreason; }
            set
            {
                if (mflagreason == value)
                    return;
                SetProperty(ref mflagreason, value);
            }
        } // [ enum (Type#6) Flag Reason ]
    }
}