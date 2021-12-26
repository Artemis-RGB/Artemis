using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Sidebar
{
    public partial class ProfileConfigurationEditView : ReactiveCoreWindow<ProfileConfigurationEditViewModel>
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
