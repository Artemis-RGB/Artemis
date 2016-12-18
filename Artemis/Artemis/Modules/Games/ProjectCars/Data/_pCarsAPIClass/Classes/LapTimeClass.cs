using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        public class LapTimesClass : INotifyPropertyChanged
        {
            private float ltlaptime;
            private float ltsect1;
            private float ltsect2;
            private float ltsect3;

            public float ltLapTime
            {
                get { return ltlaptime; }
                set
                {
                    if (ltlaptime == value)
                        return;
                    SetProperty(ref ltlaptime, value);
                }
            }

            public float ltSect1
            {
                get { return ltsect1; }
                set
                {
                    if (ltsect1 == value)
                        return;
                    SetProperty(ref ltsect1, value);
                }
            }

            public float ltSect2
            {
                get { return ltsect2; }
                set
                {
                    if (ltsect2 == value)
                        return;
                    SetProperty(ref ltsect2, value);
                }
            }

            public float ltSect3
            {
                get { return ltsect3; }
                set
                {
                    if (ltsect3 == value)
                        return;
                    SetProperty(ref ltsect3, value);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
            {
                if (!EqualityComparer<T>.Default.Equals(field, value))
                {
                    field = value;
                    var handler = PropertyChanged;
                    if (handler != null)
                        handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}