using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Operators.Screens
{
    public partial class EnumEqualsNodeCustomView : ReactiveUserControl<EnumEqualsNodeCustomViewModel>
    {
        public EnumEqualsNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
