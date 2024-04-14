using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionDetailsView : ReactiveUserControl<SubmissionDetailsViewModel>
{
    public SubmissionDetailsView()
    {
        InitializeComponent();
    }
}