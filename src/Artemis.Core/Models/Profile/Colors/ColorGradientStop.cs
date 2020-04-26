using System.ComponentModel;
using System.Runtime.CompilerServices;
using Artemis.Core.Annotations;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.Colors
{
    public class ColorGradientStop : INotifyPropertyChanged
    {
        public ColorGradientStop(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        public SKColor Color { get; set; }
        public float Position { get; set; }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}