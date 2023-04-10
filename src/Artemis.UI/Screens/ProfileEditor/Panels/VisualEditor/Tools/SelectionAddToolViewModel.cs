using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Material.Icons;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class SelectionAddToolViewModel : ToolViewModel
{
    private readonly ObservableAsPropertyHelper<bool>? _isEnabled;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IRgbService _rgbService;
    private Layer? _layer;

    /// <inheritdoc />
    public SelectionAddToolViewModel(IProfileEditorService profileEditorService, IRgbService rgbService)
    {
        _profileEditorService = profileEditorService;
        _rgbService = rgbService;
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
    public override MaterialIconKind Icon => MaterialIconKind.SelectionDrag;

    /// <inheritdoc />
    public override string ToolTip => "Add LEDs to the current layer";

    public void AddLedsInRectangle(SKRect rect, bool expand, bool inverse)
    {
        if (_layer == null)
            return;

        if (inverse)
        {
            List<ArtemisLed> toRemove = _layer.Leds.Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).ToList();
            List<ArtemisLed> toAdd = _rgbService.EnabledDevices.SelectMany(d => d.Leds).Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).Except(toRemove).ToList();
            List<ArtemisLed> leds = _layer.Leds.Except(toRemove).ToList();
            leds.AddRange(toAdd);

            _profileEditorService.ExecuteCommand(new ChangeLayerLeds(_layer, leds));
        }
        else
        {
            List<ArtemisLed> leds = _rgbService.EnabledDevices.SelectMany(d => d.Leds).Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).ToList();
            if (expand)
                leds.AddRange(_layer.Leds);
            _profileEditorService.ExecuteCommand(new ChangeLayerLeds(_layer, leds.Distinct().ToList()));
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isEnabled?.Dispose();

        base.Dispose(disposing);
    }
}