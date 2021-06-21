using System.Windows;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Providers a proxy between data contexts
    /// </summary>
    public class BindingProxy : Freezable
    {
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        /// <summary>
        ///     Gets or sets the data that was proxied
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the data that was proxied
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #region Overrides of Freezable

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion
    }
}