using System.Windows;
using System.Windows.Controls.Primitives;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a toggle button that can be locked using a property
    /// </summary>
    public class LockableToggleButton : ToggleButton
    {
        /// <summary>
        ///     Gets or sets a boolean indicating whether the toggle button is locked
        /// </summary>
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register("IsLocked", typeof(bool), typeof(LockableToggleButton), new UIPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets a boolean indicating whether the toggle button is locked
        /// </summary>
        public bool IsLocked
        {
            get => (bool) GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        /// <inheritdoc />
        protected override void OnToggle()
        {
            if (!IsLocked) base.OnToggle();
        }
    }
}