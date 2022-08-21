using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkiaSharp;

namespace Artemis.UI.Shared.DefaultTypes.DataModel.Display;

/// <summary>
///     Represents a data model display view used to display <see cref="SKColor" /> values.
/// </summary>
public class SKColorDataModelDisplayView : UserControl
{
    /// <summary>
    ///     Creates a new instance of the <see cref="SKColorDataModelDisplayView" /> class.
    /// </summary>
    public SKColorDataModelDisplayView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}