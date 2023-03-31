using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.Screens;

public partial class DataModelEventCycleNodeCustomView : ReactiveUserControl<DataModelEventCycleNodeCustomViewModel>
{
    public DataModelEventCycleNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}