using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public partial class FloatRangePropertyInputView : ReactiveUserControl<FloatRangePropertyInputViewModel>
    {
        public FloatRangePropertyInputView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
