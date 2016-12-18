using System.Collections.Generic;
using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private List<float> mbrakedamage; // [ RANGE = 0.0f->1.0f ]
        private List<float> mbraketempcelsius; // [ RANGE = 0.0f->1.0f ]
        private List<float> msuspensiondamage; // [ RANGE = 0.0f->1.0f ]
        private List<uint> mterrain; // [ enum (Type#3) Terrain Materials ]
        private List<float> mtyrecarcasstemp; // [ RANGE = 0.0f->1.0f ]
        private List<uint> mtyreflags; // [ enum (Type#7) Tyre Flags ]
        private List<float> mtyregrip; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyreheightaboveground; // [ UNITS = Local Space  Y ]
        private List<float> mtyreinternalairtemp; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyrelateralstiffness; // [ UNITS = Lateral stiffness coefficient used in tyre deformation ]
        private List<float> mtyrelayertemp; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyrerimtemp; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyrerps; // [ UNITS = Revolutions per second ]
        private List<float> mtyreslipspeed; // [ UNITS = Metres per-second ]
        private List<float> mtyretemp; // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
        private List<float> mtyretreadtemp; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyrewear; // [ RANGE = 0.0f->1.0f ]
        private List<float> mtyrey; // [ UNITS = Local Space  Y ]


        public List<uint> mTyreFlags
        {
            get { return mtyreflags; }
            set
            {
                if (mtyreflags == value)
                    return;
                SetProperty(ref mtyreflags, value);
            }
        }

        public List<uint> mTerrain
        {
            get { return mterrain; }
            set
            {
                if (mterrain == value)
                    return;
                SetProperty(ref mterrain, value);
            }
        }

        public List<float> mTyreY
        {
            get { return mtyrey; }
            set
            {
                if (mtyrey == value)
                    return;
                SetProperty(ref mtyrey, value);
            }
        }

        public List<float> mTyreRPS
        {
            get { return mtyrerps; }
            set
            {
                if (mtyrerps == value)
                    return;
                SetProperty(ref mtyrerps, value);
            }
        }

        public List<float> mTyreSlipSpeed
        {
            get { return mtyreslipspeed; }
            set
            {
                if (mtyreslipspeed == value)
                    return;
                SetProperty(ref mtyreslipspeed, value);
            }
        }

        public List<float> mTyreTemp
        {
            get { return mtyretemp; }
            set
            {
                if (mtyretemp == value)
                    return;
                SetProperty(ref mtyretemp, value);
            }
        }

        public List<float> mTyreGrip
        {
            get { return mtyregrip; }
            set
            {
                if (mtyregrip == value)
                    return;
                SetProperty(ref mtyregrip, value);
            }
        }

        public List<float> mTyreHeightAboveGround
        {
            get { return mtyreheightaboveground; }
            set
            {
                if (mtyreheightaboveground == value)
                    return;
                SetProperty(ref mtyreheightaboveground, value);
            }
        }

        public List<float> mTyreLateralStiffness
        {
            get { return mtyrelateralstiffness; }
            set
            {
                if (mtyrelateralstiffness == value)
                    return;
                SetProperty(ref mtyrelateralstiffness, value);
            }
        }

        public List<float> mTyreWear
        {
            get { return mtyrewear; }
            set
            {
                if (mtyrewear == value)
                    return;
                SetProperty(ref mtyrewear, value);
            }
        }

        public List<float> mBrakeDamage
        {
            get { return mbrakedamage; }
            set
            {
                if (mbrakedamage == value)
                    return;
                SetProperty(ref mbrakedamage, value);
            }
        }

        public List<float> mSuspensionDamage
        {
            get { return msuspensiondamage; }
            set
            {
                if (msuspensiondamage == value)
                    return;
                SetProperty(ref msuspensiondamage, value);
            }
        }

        public List<float> mBrakeTempCelsius
        {
            get { return mbraketempcelsius; }
            set
            {
                if (mbraketempcelsius == value)
                    return;
                SetProperty(ref mbraketempcelsius, value);
            }
        }

        public List<float> mTyreTreadTemp
        {
            get { return mtyretreadtemp; }
            set
            {
                if (mtyretreadtemp == value)
                    return;
                SetProperty(ref mtyretreadtemp, value);
            }
        }

        public List<float> mTyreLayerTemp
        {
            get { return mtyrelayertemp; }
            set
            {
                if (mtyrelayertemp == value)
                    return;
                SetProperty(ref mtyrelayertemp, value);
            }
        }

        public List<float> mTyreCarcassTemp
        {
            get { return mtyrecarcasstemp; }
            set
            {
                if (mtyrecarcasstemp == value)
                    return;
                SetProperty(ref mtyrecarcasstemp, value);
            }
        }

        public List<float> mTyreRimTemp
        {
            get { return mtyrerimtemp; }
            set
            {
                if (mtyrerimtemp == value)
                    return;
                SetProperty(ref mtyrerimtemp, value);
            }
        }

        public List<float> mTyreInternalAirTemp
        {
            get { return mtyreinternalairtemp; }
            set
            {
                if (mtyreinternalairtemp == value)
                    return;
                SetProperty(ref mtyreinternalairtemp, value);
            }
        }
    }
}