using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables.Fluent;
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
    private readonly IDeviceService _deviceService;
    private Layer? _layer;

    /// <inheritdoc />
    public SelectionAddToolViewModel(IProfileEditorService profileEditorService, IDeviceService deviceService)
    {
        _profileEditorService = profileEditorService;
        _deviceService = deviceService;
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
    public override Hotkey? Hotkey { get; } = new(KeyboardKey.OemPlus, KeyboardModifierKey.Control);

    /// <inheritdoc />
    public override string ToolTip => "Add LEDs to the current layer (Ctrl + +)";

    public void AddLedsInRectangle(SKRect rect, bool expand, bool inverse)
    {
        if (_layer == null)
            return;

        if (inverse)
        {
            List<ArtemisLed> toRemove = _layer.Leds.Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).ToList();
            List<ArtemisLed> toAdd = _deviceService.EnabledDevices.SelectMany(d => d.Leds).Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).Except(toRemove).ToList();
            List<ArtemisLed> leds = _layer.Leds.Except(toRemove).ToList();
            leds.AddRange(toAdd);

            _profileEditorService.ExecuteCommand(new ChangeLayerLeds(_layer, leds));
        }
        else
        {
            List<ArtemisLed> leds = _deviceService.EnabledDevices.SelectMany(d => d.Leds).Where(l => l.AbsoluteRectangle.IntersectsWith(rect)).ToList();
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