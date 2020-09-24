using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Shared
{
    // Workaround for https://developercommunity.visualstudio.com/content/problem/190202/button-controls-hosted-in-popup-windows-do-not-wor.html
    /// <inheritdoc />
    public class TreeViewPopupCompatibleCheckBox : CheckBox
    {
        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }
    }
}