using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private float maerodamage; // [ RANGE = 0.0f->1.0f ]
        private eCrashDamageState mcrashstate; // [ enum (Type#4) Crash Damage State ]
        private float menginedamage; // [ RANGE = 0.0f->1.0f ]

        public eCrashDamageState mCrashState
        {
            get { return mcrashstate; }
            set
            {
                if (mcrashstate == value)
                    return;
                SetProperty(ref mcrashstate, value);
            }
        }

        public float mAeroDamage
        {
            get { return maerodamage; }
            set
            {
                if (maerodamage == value)
                    return;
                SetProperty(ref maerodamage, value);
            }
        }

        public float mEngineDamage
        {
            get { return menginedamage; }
            set
            {
                if (menginedamage == value)
                    return;
                SetProperty(ref menginedamage, value);
            }
        }
    }
}