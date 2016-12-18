using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        public class pCarsParticipantsClass : INotifyPropertyChanged
        {
            private uint parcurrentlap;
            private float parcurrentlapdistance;
            private eCurrentSector parcurrentsector;
            private bool parisactive;
            private uint parlapscompleted;
            private string parname;
            private uint parraceposition;
            private List<float> parworldposition;

            public bool parIsActive
            {
                get { return parisactive; }
                set
                {
                    if (parisactive == value)
                        return;
                    SetProperty(ref parisactive, value);
                }
            }

            public string parName
            {
                get { return parname; }
                set
                {
                    if (parname == value)
                        return;
                    SetProperty(ref parname, value);
                }
            } // [ string ]

            public List<float> parWorldPosition
            {
                get { return parworldposition; }
                set
                {
                    if (parworldposition == value)
                        return;
                    SetProperty(ref parworldposition, value);
                }
            } // [ UNITS = World Space  X  Y  Z ]

            public float parCurrentLapDistance
            {
                get { return parcurrentlapdistance; }
                set
                {
                    if (parcurrentlapdistance == value)
                        return;
                    SetProperty(ref parcurrentlapdistance, value);
                }
            } // [ UNITS = Metres ]   [ RANGE = 0.0f->... ]    [ UNSET = 0.0f ]

            public uint parRacePosition
            {
                get { return parraceposition; }
                set
                {
                    if (parraceposition == value)
                        return;
                    SetProperty(ref parraceposition, value);
                }
            } // [ RANGE = 1->... ]   [ UNSET = 0 ]

            public uint parLapsCompleted
            {
                get { return parlapscompleted; }
                set
                {
                    if (parlapscompleted == value)
                        return;
                    SetProperty(ref parlapscompleted, value);
                }
            } // [ RANGE = 0->... ]   [ UNSET = 0 ]

            public uint parCurrentLap
            {
                get { return parcurrentlap; }
                set
                {
                    if (parcurrentlap == value)
                        return;
                    SetProperty(ref parcurrentlap, value);
                }
            } // [ RANGE = 0->... ]   [ UNSET = 0 ]

            public eCurrentSector parCurrentSector
            {
                get { return parcurrentsector; }
                set
                {
                    if (parcurrentsector == value)
                        return;

                    SetProperty(ref parcurrentsector, value);
                }
            } // [ enum (Type#4) Current Sector ]

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