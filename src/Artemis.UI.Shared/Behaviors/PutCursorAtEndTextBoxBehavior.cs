using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a behavior that puts the cursor at the end of a text box when it receives focus
    /// </summary>
    public class PutCursorAtEndTextBox : Behavior<UIElement>
    {
        private TextBox? _textBox;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            _textBox = AssociatedObject as TextBox;

            if (_textBox == null) return;
            _textBox.GotFocus += TextBoxGotFocus;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            if (_textBox == null) return;
            _textBox.GotFocus -= TextBoxGotFocus;

            base.OnDetaching();
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_textBox == null) return;
            _textBox.CaretIndex = _textBox.Text.Length;
        }
    }
}