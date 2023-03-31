using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.Screens;

public partial class DataModelEventNodeCustomView : ReactiveUserControl<DataModelEventNodeCustomViewModel>
{
    public DataModelEventNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}