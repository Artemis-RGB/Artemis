using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class ProfileEditorView : ReactiveUserControl<ProfileEditorViewModel>
{
    public ProfileEditorView()
    {
        InitializeComponent();
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    #endregion

}