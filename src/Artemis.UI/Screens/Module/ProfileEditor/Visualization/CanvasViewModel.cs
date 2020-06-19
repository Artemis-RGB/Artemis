using System;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public abstract class CanvasViewModel : PropertyChangedBase, IDisposable
    {
        public double X { get; set; }
        public double Y { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}