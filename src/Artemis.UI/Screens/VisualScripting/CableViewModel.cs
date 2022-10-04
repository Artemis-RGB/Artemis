using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class CableViewModel : ActivatableViewModelBase
{
    private readonly IPin _from;
    private readonly NodeScriptViewModel _nodeScriptViewModel;
    private readonly IPin _to;
    private ObservableAsPropertyHelper<Color>? _cableColor;
    private ObservableAsPropertyHelper<bool>? _connected;
    private bool _displayValue;
    private ObservableAsPropertyHelper<Point>? _fromPoint;

    private PinViewModel? _fromViewModel;
    private ObservableAsPropertyHelper<Point>? _toPoint;
    private PinViewModel? _toViewModel;

    public CableViewModel(NodeScriptViewModel nodeScriptViewModel, IPin from, IPin to, ISettingsService settingsService)
    {
        _nodeScriptViewModel = nodeScriptViewModel;
        _from = from;
        _to = to;

        AlwaysShowValues = settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);

        if (from.Direction != PinDirection.Output)
            throw new ArtemisUIException("Can only create cables originating from an output pin");
        if (to.Direction != PinDirection.Input)
            throw new ArtemisUIException("Can only create cables targeted to an input pin");

        this.WhenActivated(d =>
        {
            nodeScriptViewModel.PinViewModels.ToObservableChangeSet().Filter(p => ReferenceEquals(p.Pin, from)).Transform(model => FromViewModel = model).Subscribe().DisposeWith(d);
            nodeScriptViewModel.PinViewModels.ToObservableChangeSet().Filter(p => ReferenceEquals(p.Pin, to)).Transform(model => ToViewModel = model).Subscribe().DisposeWith(d);

            _cableColor = this.WhenAnyValue(vm => vm.FromViewModel, vm => vm.ToViewModel)
                .Select(tuple => tuple.Item1?.WhenAnyValue(p => p.PinColor) ?? tuple.Item2?.WhenAnyValue(p => p.PinColor) ?? Observable.Never<Color>())
                .Switch()
                .ToProperty(this, vm => vm.CableColor)
                .DisposeWith(d);

            _fromPoint = this.WhenAnyValue(vm => vm.FromViewModel)
                .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
                .Switch()
                .ToProperty(this, vm => vm.FromPoint)
                .DisposeWith(d);
            _toPoint = this.WhenAnyValue(vm => vm.ToViewModel)
                .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
                .Switch()
                .ToProperty(this, vm => vm.ToPoint)
                .DisposeWith(d);
            
            // Not a perfect solution but this makes sure the cable never renders at 0,0 (can happen when the cable spawns before the pin ever rendered)
            _connected = this.WhenAnyValue(vm => vm.FromPoint, vm => vm.ToPoint)
                .Select(tuple => tuple.Item1 != new Point(0, 0) && tuple.Item2 != new Point(0, 0))
                .ToProperty(this, vm => vm.Connected)
                .DisposeWith(d);

            AlwaysShowValues.SettingChanged += AlwaysShowValuesOnSettingChanged;
            Disposable.Create(() => AlwaysShowValues.SettingChanged -= AlwaysShowValuesOnSettingChanged).DisposeWith(d);
        });


        UpdateDisplayValue(false);
    }

    public PluginSetting<bool> AlwaysShowValues { get; }

    public PinViewModel? FromViewModel
    {
        get => _fromViewModel;
        set => RaiseAndSetIfChanged(ref _fromViewModel, value);
    }

    public PinViewModel? ToViewModel
    {
        get => _toViewModel;
        set => RaiseAndSetIfChanged(ref _toViewModel, value);
    }

    public bool DisplayValue
    {
        get => _displayValue;
        set => RaiseAndSetIfChanged(ref _displayValue, value);
    }

    public bool Connected => _connected?.Value ?? false;
    public bool IsFirst => _from.ConnectedTo.FirstOrDefault() == _to;

    public Point FromPoint => _fromPoint?.Value ?? new Point();
    public Point ToPoint => _toPoint?.Value ?? new Point();
    public Color CableColor => _cableColor?.Value ?? new Color(255, 255, 255, 255);

    public void UpdateDisplayValue(bool hoveringOver)
    {
        if (_nodeScriptViewModel.IsPreview)
            DisplayValue = false;

        if (hoveringOver)
            DisplayValue = true;
        else
            DisplayValue = AlwaysShowValues.Value && IsFirst;
    }

    private void AlwaysShowValuesOnSettingChanged(object? sender, EventArgs e)
    {
        UpdateDisplayValue(false);
    }
}