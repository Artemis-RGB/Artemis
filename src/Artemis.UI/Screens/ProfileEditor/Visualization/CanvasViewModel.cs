using System;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public abstract class CanvasViewModel : PropertyChangedBase, IDisposable
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}