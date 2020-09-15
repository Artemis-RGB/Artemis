using System.Windows;
using System.Windows.Controls.Primitives;

namespace Artemis.UI.Shared
{
    public class LockableToggleButton : ToggleButton
    {
        protected override void OnToggle()
        {
            if (!IsLocked)
            {
                base.OnToggle();
            }
        }

        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LockToggle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register("IsLocked", typeof(bool), typeof(LockableToggleButton), new UIPropertyMetadata(false));
    }
}
