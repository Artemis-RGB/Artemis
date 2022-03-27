using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.DataModel
{
    public class DataModelDebugView : ReactiveUserControl<DataModelDebugViewModel>
    {
        public DataModelDebugView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}