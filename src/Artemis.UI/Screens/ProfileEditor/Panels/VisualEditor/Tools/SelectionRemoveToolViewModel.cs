using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Material.Icons;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class SelectionRemoveToolViewModel : ToolViewModel
{
    private readonly ObservableAsPropertyHelper<bool>? _isEnabled;
    private readonly IProfileEditorService _profileEditorService;
    private Layer? _layer;

    /// <inheritdoc />
    public SelectionRemoveToolViewModel(IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        // Not disposed when deactivated but when really disposed
        _isEnabled = profileEditorService.ProfileElement.Select(p => p is Layer).ToProperty(this, vm => vm.IsEnabled);
        this.WhenActivated(d => profileEditorService.ProfileElement.Subscribe(p => _layer = p as Layer).DisposeWith(d));
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
    public override MaterialIconKind Icon => MaterialIconKind.SelectOff;

    /// <inheritdoc />
    public override string ToolTip => "Remove LEDs from the current layer";

    public void RemoveLedsInRectangle(SKRect rect)
    {
        if (_layer == null)
            return;

        List<ArtemisLed> leds = _layer.Leds.Except(_layer.Leds.Where(l => l.AbsoluteRectangle.IntersectsWith(rect))).ToList();
        _profileEditorService.ExecuteCommand(new ChangeLayerLeds(_layer, leds));
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isEnabled?.Dispose();

        base.Dispose(disposing);
    }
}