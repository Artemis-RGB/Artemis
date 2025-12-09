using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Models;
using Artemis.UI.Services.ProfileEditor.Commands;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Keyframes;

public partial class TimelineKeyframeViewModel<T> : ActivatableViewModelBase, ITimelineKeyframeViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<bool>? _isSelected;
    [Notify] private bool _canPaste;
    [Notify] private bool _isFlyoutOpen;
    [Notify] private string _timestamp;
    [Notify] private double _x;

    public TimelineKeyframeViewModel(LayerPropertyKeyframe<T> layerPropertyKeyframe, IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        _timestamp = "0.000";
        LayerPropertyKeyframe = layerPropertyKeyframe;
        EasingViewModels = new ObservableCollection<TimelineEasingViewModel>();

        this.WhenActivated(d =>
        {
            _isSelected = profileEditorService.SelectedKeyframes
                .ToObservableChangeSet()
                .Select(_ => profileEditorService.SelectedKeyframes.Contains(LayerPropertyKeyframe))
                .ToProperty(this, vm => vm.IsSelected)
                .DisposeWith(d);
            profileEditorService.PixelsPerSecond.Subscribe(p => _pixelsPerSecond = p).DisposeWith(d);
            profileEditorService.PixelsPerSecond.Subscribe(_ => Update()).DisposeWith(d);
            this.WhenAnyValue(vm => vm.LayerPropertyKeyframe.Position).Subscribe(_ => Update()).DisposeWith(d);
        });
        this.WhenAnyValue<TimelineKeyframeViewModel<T>, bool>(vm => vm.IsFlyoutOpen).Subscribe(UpdateCanPaste);

        Duplicate = ReactiveCommand.Create(ExecuteDuplicate);
        Copy = ReactiveCommand.CreateFromTask(ExecuteCopy);
        Paste = ReactiveCommand.CreateFromTask(ExecutePaste);
        Delete = ReactiveCommand.Create(ExecuteDelete);
        SelectEasingFunction = ReactiveCommand.Create<Easings.Functions>(ExecuteSelectEasingFunction);
    }

    public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }
    public ObservableCollection<TimelineEasingViewModel> EasingViewModels { get; }
    
    public void Update()
    {
        X = _pixelsPerSecond * LayerPropertyKeyframe.Position.TotalSeconds;
        Timestamp = $"{Math.Floor(LayerPropertyKeyframe.Position.TotalSeconds):00}.{LayerPropertyKeyframe.Position.Milliseconds:000}";
    }

    public ReactiveCommand<Unit, Unit> Duplicate { get; }
    public ReactiveCommand<Unit, Unit> Copy { get; }
    public ReactiveCommand<Unit, Unit> Paste { get; }
    public ReactiveCommand<Unit, Unit> Delete { get; }
    public ReactiveCommand<Easings.Functions, Unit> SelectEasingFunction { get; }
    
    public bool IsSelected => _isSelected?.Value ?? false;
    public TimeSpan Position => LayerPropertyKeyframe.Position;
    public ILayerPropertyKeyframe Keyframe => LayerPropertyKeyframe;

    /// <inheritdoc />
    public void Select(bool expand, bool toggle)
    {
        _profileEditorService.SelectKeyframe(Keyframe, expand, toggle);
    }

    public TimeSpan GetTimeSpanAtPosition(double x)
    {
        return TimeSpan.FromSeconds(x / _pixelsPerSecond);
    }

    #region Context menu actions

    private void ExecuteDelete()
    {
        if (!IsSelected)
        {
            _profileEditorService.ExecuteCommand(new DeleteKeyframe(Keyframe));
        }
        else
        {
            List<ILayerPropertyKeyframe> keyframes = _profileEditorService.SelectedKeyframes.ToList();
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Delete keyframes");
            foreach (ILayerPropertyKeyframe keyframe in keyframes)
                _profileEditorService.ExecuteCommand(new DeleteKeyframe(keyframe));
        }
    }

    private void ExecuteDuplicate()
    {
        if (!IsSelected)
        {
            DuplicateKeyframe command = new(Keyframe, FindKeyframeDuplicationPosition(Keyframe));
            _profileEditorService.ExecuteCommand(command);
            _profileEditorService.SelectKeyframe(command.Duplication, false, false);
        }
        else
        {
            List<ILayerPropertyKeyframe> keyframes = _profileEditorService.SelectedKeyframes.ToList();
            _profileEditorService.SelectKeyframe(null, false, false);
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Duplicate keyframes");
            foreach (ILayerPropertyKeyframe keyframe in keyframes)
            {
                DuplicateKeyframe command = new(keyframe, FindKeyframeDuplicationPosition(keyframe));
                _profileEditorService.ExecuteCommand(command);
                _profileEditorService.SelectKeyframe(command.Duplication, true, false);
            }
        }
    }

    private async Task ExecuteCopy()
    {
        List<KeyframeClipboardModel> keyframes = new();
        if (!IsSelected)
            keyframes.Add(new KeyframeClipboardModel(Keyframe));
        else
            keyframes.AddRange(_profileEditorService.SelectedKeyframes.Select(k => new KeyframeClipboardModel(k)));

        string copy = CoreJson.Serialize(keyframes);
        DataObject dataObject = new();
        dataObject.Set(KeyframeClipboardModel.ClipboardDataFormat, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);
    }

    private async Task ExecutePaste()
    {
        List<KeyframeClipboardModel>? keyframes = await Shared.UI.Clipboard.GetJsonAsync<List<KeyframeClipboardModel>>(KeyframeClipboardModel.ClipboardDataFormat);
        if (keyframes == null)
            return;

        PasteKeyframes command = new(Keyframe.UntypedLayerProperty.ProfileElement, keyframes, FindKeyframeDuplicationPosition(Keyframe));
        _profileEditorService.ExecuteCommand(command);
        if (command.PastedKeyframes != null && command.PastedKeyframes.Any())
            _profileEditorService.SelectKeyframes(command.PastedKeyframes, false);
    }

    private TimeSpan FindKeyframeDuplicationPosition(ILayerPropertyKeyframe keyframe)
    {
        TimeSpan position;
        TimeSpan distance = TimeSpan.FromSeconds(15 / _pixelsPerSecond);
        TimeSpan maxRight = keyframe.UntypedLayerProperty.ProfileElement.Timeline.Length;

        // Pick the side that has the most available space
        // Prefer right side
        if (keyframe.Position + distance <= maxRight)
            // Put the keyframe as far to the right as possible within the max
            position = TimeSpan.FromSeconds(Math.Min(maxRight.TotalSeconds, (keyframe.Position + distance).TotalSeconds));
        // Fall back to left side
        else
            // Put the keyframe as far to the left as possible withing the max
            position = TimeSpan.FromSeconds(Math.Max(0, (keyframe.Position - distance).TotalSeconds));

        return position;
    }

    private async void UpdateCanPaste(bool isFlyoutOpen)
    {
        string[]? formats = await Shared.UI.Clipboard.GetFormatsAsync();
        
        // Can be null on some platforms
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        CanPaste = formats != null && formats.Contains("Artemis.Keyframes");
    }

    #endregion

    #region Movement

    private TimeSpan? _offset;
    private TimeSpan? _startPosition;
    private double _pixelsPerSecond;

    public void StartMovement(ITimelineKeyframeViewModel source)
    {
        _startPosition = LayerPropertyKeyframe.Position;
        if (source != this)
            _offset = LayerPropertyKeyframe.Position - source.Position;
    }

    public void UpdateMovement(TimeSpan position)
    {
        if (_offset == null)
            UpdatePosition(position);
        else
            UpdatePosition(position + _offset.Value);
    }

    public void FinishMovement()
    {
        MoveKeyframe command = _startPosition != null
            ? new MoveKeyframe(Keyframe, Keyframe.Position, _startPosition.Value)
            : new MoveKeyframe(Keyframe, Keyframe.Position);

        _startPosition = null;
        _offset = null;

        _profileEditorService.ExecuteCommand(command);
    }

    private void UpdatePosition(TimeSpan position)
    {
        if (position < TimeSpan.Zero)
            LayerPropertyKeyframe.Position = TimeSpan.Zero;
        else if (position > LayerPropertyKeyframe.LayerProperty.ProfileElement.Timeline.Length)
            LayerPropertyKeyframe.Position = LayerPropertyKeyframe.LayerProperty.ProfileElement.Timeline.Length;
        else
            LayerPropertyKeyframe.Position = position;
    }

    #endregion

    #region Easing

    public void PopulateEasingViewModels()
    {
        if (EasingViewModels.Any())
            return;

        EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions))
            .Cast<Easings.Functions>()
            .Select(e => new TimelineEasingViewModel(e, Keyframe, SelectEasingFunction)));
    }

    private void ExecuteSelectEasingFunction(Easings.Functions easingFunction)
    {
        _profileEditorService.ExecuteCommand(new ChangeKeyframeEasing(Keyframe, easingFunction));
    }

    #endregion
}