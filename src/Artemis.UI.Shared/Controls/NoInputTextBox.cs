using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace Artemis.UI.Shared
{
    internal class NoInputTextBox : TextBox, IStyleable
    {
        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Don't call the base method on purpose
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            // Don't call the base method on purpose
        }

        Type IStyleable.StyleKey => typeof(TextBox);
    }
}