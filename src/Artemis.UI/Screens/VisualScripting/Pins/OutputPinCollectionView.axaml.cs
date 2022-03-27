using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins
{
    public partial class OutputPinCollectionView : ReactiveUserControl<PinCollectionViewModel>
    {
        public OutputPinCollectionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
