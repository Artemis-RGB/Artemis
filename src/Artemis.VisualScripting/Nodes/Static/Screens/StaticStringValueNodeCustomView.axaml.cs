using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public partial class StaticStringValueNodeCustomView : ReactiveUserControl<StaticStringValueNodeCustomViewModel>
{
    public StaticStringValueNodeCustomView()
    {
        InitializeComponent();
    }

}