using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        //public void Initialise()
        //{
        //    mRPM = 0;
        //}


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