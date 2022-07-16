using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public partial class EnumPropertyInputView : ReactiveUserControl<PropertyInputViewModel>
    {
        public EnumPropertyInputView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}