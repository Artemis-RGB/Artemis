using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.Screens;

public class DataModelNodeCustomView : ReactiveUserControl<DataModelNodeCustomViewModel>
{
    public DataModelNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}