using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeView : ReactiveUserControl<NodeViewModel>
{
    public NodeView()
    {
        InitializeComponent();
    }

    #region Overrides of Layoutable

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        // Take the base implementation's size
        (double width, double height) = base.MeasureOverride(availableSize);

        // Ceil the resulting size   
        width = Math.Ceiling(width / 10.0) * 10.0;
        height = Math.Ceiling(height / 10.0) * 10.0;

        return new Size(width, height);
    }

    #endregion

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}