using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.CustomViews
{
    public partial class DataModelEventNodeCustomView : ReactiveUserControl<DataModelEventNodeCustomViewModel>
    {
        public DataModelEventNodeCustomView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
