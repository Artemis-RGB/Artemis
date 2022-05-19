using System;
using System.Collections.ObjectModel;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineGroupViewModel : ActivatableViewModelBase
{
    private ReadOnlyObservableCollection<double> _keyframePositions;
    private int _pixelsPerSecond;

    public TimelineGroupViewModel(PropertyGroupViewModel propertyGroupViewModel, IProfileEditorService profileEditorService)
    {
        PropertyGroupViewModel = propertyGroupViewModel;
        _keyframePositions = new ReadOnlyObservableCollection<double>(new ObservableCollection<double>());

        this.WhenActivated(d =>
        {
            profileEditorService.PixelsPerSecond.Subscribe(p => _pixelsPerSecond = p).DisposeWith(d);

            PropertyGroupViewModel.WhenAnyValue(vm => vm.IsExpanded).Subscribe(_ => this.RaisePropertyChanged(nameof(Children))).DisposeWith(d);
            PropertyGroupViewModel.Keyframes
                .ToObservableChangeSet()
                .AutoRefreshOnObservable(_ => profileEditorService.PixelsPerSecond)
                .Transform(k => k.Position.TotalSeconds * _pixelsPerSecond, true)
                .Bind(out ReadOnlyObservableCollection<double> keyframePositions)
                .Subscribe()
                .DisposeWith(d);
            KeyframePositions = keyframePositions;
        });
    }

    public PropertyGroupViewModel PropertyGroupViewModel { get; }

    public ObservableCollection<PropertyViewModelBase>? Children => PropertyGroupViewModel.IsExpanded ? PropertyGroupViewModel.Children : null;

    public ReadOnlyObservableCollection<double> KeyframePositions
    {
        get => _keyframePositions;
        set => RaiseAndSetIfChanged(ref _keyframePositions, value);
    }
}