using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.Views
{
    public class SurfaceEditorView : ReactiveUserControl<SurfaceEditorViewModel>
    {
        public SurfaceEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}