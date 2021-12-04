using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Root.Sidebar.Dialogs
{
    public partial class ProfileConfigurationEditView : ReactiveWindow<ProfileConfigurationEditViewModel>
    {
        public ProfileConfigurationEditView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
