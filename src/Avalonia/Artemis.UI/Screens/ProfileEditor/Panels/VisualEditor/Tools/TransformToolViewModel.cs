using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolViewModel : ToolViewModel
{
    private readonly ObservableAsPropertyHelper<bool>? _isEnabled;

    /// <inheritdoc />
    public TransformToolViewModel(IProfileEditorService profileEditorService)
    {
        _isEnabled = profileEditorService.ProfileElement.Select(p => p is Layer).ToProperty(this, vm => vm.IsEnabled);
    }

    /// <inheritdoc />
    public override bool IsEnabled => _isEnabled?.Value ?? false;

    /// <inheritdoc />
    public override bool IsExclusive => true;

    /// <inheritdoc />
    public override bool ShowInToolbar => true;

    /// <inheritdoc />
    public override int Order => 3;

    /// <inheritdoc />
    public override MaterialIconKind Icon => MaterialIconKind.TransitConnectionVariant;

    /// <inheritdoc />
    public override string ToolTip => "Transform the shape of the current layer";

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isEnabled?.Dispose();

        base.Dispose(disposing);
    }
}