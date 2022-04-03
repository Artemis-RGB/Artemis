using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.DataBinding
{
    public partial class DataBindingView : ReactiveUserControl<DataBindingViewModel>
    {
        public DataBindingView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
