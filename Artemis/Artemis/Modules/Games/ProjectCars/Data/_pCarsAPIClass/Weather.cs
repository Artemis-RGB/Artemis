using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private float mambienttemperature; // [ UNITS = Celsius ]   [ UNSET = 25.0f ]
        private float mcloudbrightness; // [ RANGE = 0.0f->... ]
        private float mraindensity; // [ UNITS = How much rain will fall ]   [ RANGE = 0.0f->1.0f ]
        private float mtracktemperature; // [ UNITS = Celsius ]   [ UNSET = 30.0f ]
        private float mwinddirectionx; // [ UNITS = Normalised Vector X ]
        private float mwinddirectiony; // [ UNITS = Normalised Vector Y ]
        private float mwindspeed; // [ RANGE = 0.0f->100.0f ]   [ UNSET = 2.0f ]

        public float mAmbientTemperature
        {
            get { return mambienttemperature; }
            set
            {
                if (mambienttemperature == value)
                    return;
                SetProperty(ref mambienttemperature, value);
            }
        }

        public float mTrackTemperature
        {
            get { return mtracktemperature; }
            set
            {
                if (mtracktemperature == value)
                    return;
                SetProperty(ref mtracktemperature, value);
            }
        }

        public float mRainDensity
        {
            get { return mraindensity; }
            set
            {
                if (mraindensity == value)
                    return;
                SetProperty(ref mraindensity, value);
            }
        }

        public float mWindSpeed
        {
            get { return mwindspeed; }
            set
            {
                if (mwindspeed == value)
                    return;
                SetProperty(ref mwindspeed, value);
            }
        }

        public float mWindDirectionX
        {
            get { return mwinddirectionx; }
            set
            {
                if (mwinddirectionx == value)
                    return;
                SetProperty(ref mwinddirectionx, value);
            }
        }

        public float mWindDirectionY
        {
            get { return mwinddirectiony; }
            set
            {
                if (mwinddirectiony == value)
                    return;
                SetProperty(ref mwinddirectiony, value);
            }
        }

        public float mCloudBrightness
        {
            get { return mcloudbrightness; }
            set
            {
                if (mcloudbrightness == value)
                    return;
                SetProperty(ref mcloudbrightness, value);
            }
        }
    }
}