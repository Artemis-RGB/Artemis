using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Keyframes;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelinePropertyViewModel<T> : ActivatableViewModelBase, ITimelinePropertyViewModel
{
    private readonly SourceList<LayerPropertyKeyframe<T>> _keyframes;
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<bool>? _keyframesEnabled;

    public TimelinePropertyViewModel(LayerProperty<T> layerProperty, PropertyViewModel propertyViewModel, IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        LayerProperty = layerProperty;
        PropertyViewModel = propertyViewModel;

        _keyframes = new SourceList<LayerPropertyKeyframe<T>>();

        _keyframes.Connect()
            // Only show items when keyframes are enabled
            .Filter(this.WhenAnyValue(vm => vm.KeyframesEnabled).Select(b => new Func<LayerPropertyKeyframe<T>, bool>(_ => b)))
            .Transform(k => new TimelineKeyframeViewModel<T>(k, _profileEditorService))
            .Bind(out ReadOnlyObservableCollection<TimelineKeyframeViewModel<T>> keyframeViewModels)
            .Subscribe();
        KeyframeViewModels = keyframeViewModels;

        this.WhenActivated(d =>
        {
            _keyframesEnabled = LayerProperty.WhenAnyValue(p => p.KeyframesEnabled).ToProperty(this, vm => vm.KeyframesEnabled).DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyKeyframeEventArgs>(x => LayerProperty.KeyframeAdded += x, x => LayerProperty.KeyframeAdded -= x)
                .Subscribe(e => _keyframes.Add((LayerPropertyKeyframe<T>) e.EventArgs.Keyframe))
                .DisposeWith(d);
            Observable.FromEventPattern<LayerPropertyKeyframeEventArgs>(x => LayerProperty.KeyframeRemoved += x, x => LayerProperty.KeyframeRemoved -= x)
                .Subscribe(e => _keyframes.RemoveMany(_keyframes.Items.Where(k => k == e.EventArgs.Keyframe)))
                .DisposeWith(d);

            _keyframes.Edit(k =>
            {
                k.Clear();
                k.AddRange(LayerProperty.Keyframes);
            });
        });
    }

    public LayerProperty<T> LayerProperty { get; }
    public PropertyViewModel PropertyViewModel { get; }
    public ReadOnlyObservableCollection<TimelineKeyframeViewModel<T>> KeyframeViewModels { get; }
    public bool KeyframesEnabled => _keyframesEnabled?.Value ?? false;

    private void UpdateKeyframes()
    {
        foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in KeyframeViewModels)
            timelineKeyframeViewModel.Update();
    }

    #region Implementation of ITimelinePropertyViewModel

    public List<ILayerPropertyKeyframe> GetAllKeyframes()
    {
        return LayerProperty.KeyframesEnabled ? new List<ILayerPropertyKeyframe>(LayerProperty.Keyframes) : new List<ILayerPropertyKeyframe>();
    }

    public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels()
    {
        return KeyframeViewModels.Cast<ITimelineKeyframeViewModel>().ToList();
    }

    public void WipeKeyframes(TimeSpan? start, TimeSpan? end)
    {
        start ??= TimeSpan.Zero;
        end ??= TimeSpan.MaxValue;

        List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position >= start && k.Position < end).ToList();
        foreach (LayerPropertyKeyframe<T> keyframe in toShift)
            LayerProperty.RemoveKeyframe(keyframe);

        UpdateKeyframes();
    }

    public void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
    {
        start ??= TimeSpan.Zero;
        end ??= TimeSpan.MaxValue;

        List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position > start && k.Position < end).ToList();
        foreach (LayerPropertyKeyframe<T> keyframe in toShift)
            keyframe.Position += amount;

        UpdateKeyframes();
    }

    #endregion
}