using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Search;

public partial class SearchResultView : ReactiveUserControl<SearchResultViewModel>
{
    public SearchResultView()
    {
        InitializeComponent();
    }
}