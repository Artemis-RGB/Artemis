using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using Material.Icons;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class SelectionAddToolViewModel : ToolViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IRgbService _rgbService;
    private readonly ObservableAsPropertyHelper<bool>? _isEnabled;
    private Layer? _layer;

    /// <inheritdoc />
    public SelectionAddToolViewModel(IProfileEditorService profileEditorService, IRgbService rgbService)
    {
        _profileEditorService = profileEditorService;
        _rgbService = rgbService;
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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _isEnabled?.Dispose();

        base.Dispose(disposing);
    }

    public void AddLedsInRectangle(SKRect rect)
    {
        if (_layer == null)
            return;

        List<ArtemisLed> leds = _rgbService.EnabledDevices.SelectMany(d => d.Leds).Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).ToList();
        _profileEditorService.ExecuteCommand(new ChangeLayerLeds(_layer, leds));
    }
}