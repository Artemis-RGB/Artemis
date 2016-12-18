using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        private string mcarclassname;
        // Vehicle & Track information

        private string mcarname;

        public string mCarName
        {
            get { return mcarname; }
            set
            {
                if (mcarname == value)
                    return;
                SetProperty(ref mcarname, value);
            }
        }

        public string mCarClassName
        {
            get { return mcarclassname; }
            set
            {
                if (mcarclassname == value)
                    return;
                SetProperty(ref mcarclassname, value);
            }
        }
    }
}