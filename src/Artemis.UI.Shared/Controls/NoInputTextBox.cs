using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace Artemis.UI.Shared;

internal class NoInputTextBox : TextBox
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

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TextBox);
}