using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.DataModel;

public partial class DataModelDebugView : ReactiveUserControl<DataModelDebugViewModel>
{
    public DataModelDebugView()
    {
        InitializeComponent();
    }

}