using Artemis.VisualScripting.Nodes.CustomViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.CustomViews
{
    public partial class StaticNumericValueNodeCustomView : ReactiveUserControl<StaticNumericValueNodeCustomViewModel>
    {
        public StaticNumericValueNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
