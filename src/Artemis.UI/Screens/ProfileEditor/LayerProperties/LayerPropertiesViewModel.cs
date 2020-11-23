using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using GongSolutions.Wpf.DragDrop;
using Stylet;
using static Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree.TreeGroupViewModel.LayerPropertyGroupType;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : Conductor<IScreen>.Collection.AllActive, IProfileEditorPanelViewModel, IDropTarget
    {
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private LayerPropertyGroupViewModel _brushPropertyGroup;
        private bool _playing;
        private bool _repeating;
        private bool _repeatSegment;
        private bool _repeatTimeline = true;
        private int _propertyTreeIndex;
        private int _rightSideIndex;
        private RenderProfileElement _selectedProfileElement;
        private DateTime _lastEffectsViewModelToggle;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService,
            ICoreService coreService,
            ISettingsService settingsService,
            ILayerPropertyVmFactory layerPropertyVmFactory,
            DataBindingsViewModel dataBindingsViewModel)
        {
            _layerPropertyVmFactory = layerPropertyVmFactory;

            ProfileEditorService = profileEditorService;
            CoreService = coreService;
            SettingsService = settingsService;

            LayerPropertyGroups = new BindableCollection<LayerPropertyGroupViewModel>();
            PropertyChanged += HandlePropertyTreeIndexChanged;

            // Left side 
            TreeViewModel = _layerPropertyVmFactory.TreeViewModel(this, LayerPropertyGroups);
            EffectsViewModel = _layerPropertyVmFactory.EffectsViewModel(this);
            Items.Add(TreeViewModel);
            Items.Add(EffectsViewModel);

            // Right side
            StartTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.Start, LayerPropertyGroups);
            MainTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.Main, LayerPropertyGroups);
            EndTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.End, LayerPropertyGroups);
            TimelineViewModel = _layerPropertyVmFactory.TimelineViewModel(this, LayerPropertyGroups);
            DataBindingsViewModel = dataBindingsViewModel;
            Items.Add(StartTimelineSegmentViewModel);
            Items.Add(MainTimelineSegmentViewModel);
            Items.Add(EndTimelineSegmentViewModel);
            Items.Add(TimelineViewModel);
            Items.Add(DataBindingsViewModel);
        }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }


        #region Child VMs

        public TreeViewModel TreeViewModel { get; }
        public EffectsViewModel EffectsViewModel { get; }
        public TimelineSegmentViewModel StartTimelineSegmentViewModel { get; }
        public TimelineSegmentViewModel MainTimelineSegmentViewModel { get; }
        public TimelineSegmentViewModel EndTimelineSegmentViewModel { get; }
        public TimelineViewModel TimelineViewModel { get; }
        public DataBindingsViewModel DataBindingsViewModel { get; }

        #endregion

        public IProfileEditorService ProfileEditorService { get; }
        public ICoreService CoreService { get; }
        public ISettingsService SettingsService { get; }

        public bool Playing
        {
            get => _playing;
            set => SetAndNotify(ref _playing, value);
        }

        public bool Repeating
        {
            get => _repeating;
            set => SetAndNotify(ref _repeating, value);
        }

        public bool RepeatSegment
        {
            get => _repeatSegment;
            set => SetAndNotify(ref _repeatSegment, value);
        }

        public bool RepeatTimeline
        {
            get => _repeatTimeline;
            set => SetAndNotify(ref _repeatTimeline, value);
        }

        public string FormattedCurrentTime => $"{Math.Floor(ProfileEditorService.CurrentTime.TotalSeconds):00}.{ProfileEditorService.CurrentTime.Milliseconds:000}";

        public double TimeCaretPosition
        {
            get => ProfileEditorService.CurrentTime.TotalSeconds * ProfileEditorService.PixelsPerSecond;
            set => ProfileEditorService.CurrentTime = TimeSpan.FromSeconds(value / ProfileEditorService.PixelsPerSecond);
        }

        public int PropertyTreeIndex
        {
            get => _propertyTreeIndex;
            set
            {
                if (!SetAndNotify(ref _propertyTreeIndex, value)) return;
                NotifyOfPropertyChange(nameof(PropertyTreeVisible));
            }
        }

        public int RightSideIndex
        {
            get => _rightSideIndex;
            set => SetAndNotify(ref _rightSideIndex, value);
        }

        public bool CanToggleEffectsViewModel => SelectedProfileElement != null && DateTime.Now - _lastEffectsViewModelToggle > TimeSpan.FromMilliseconds(250);

        public bool PropertyTreeVisible => PropertyTreeIndex == 0;

        public RenderProfileElement SelectedProfileElement
        {
            get => _selectedProfileElement;
            set
            {
                if (!SetAndNotify(ref _selectedProfileElement, value)) return;
                NotifyOfPropertyChange(nameof(SelectedLayer));
                NotifyOfPropertyChange(nameof(SelectedFolder));
                NotifyOfPropertyChange(nameof(SelectedFolder));
                NotifyOfPropertyChange(nameof(CanToggleEffectsViewModel));
            }
        }

        public Layer SelectedLayer => SelectedProfileElement as Layer;
        public Folder SelectedFolder => SelectedProfileElement as Folder;


        #region Segments

        public void EnableSegment(string segment)
        {
            if (segment == "Start")
                StartTimelineSegmentViewModel.EnableSegment();
            else if (segment == "Main")
                MainTimelineSegmentViewModel.EnableSegment();
            else if (segment == "End")
                EndTimelineSegmentViewModel.EnableSegment();
        }

        #endregion

        protected override void OnInitialActivate()
        {
            PopulateProperties(ProfileEditorService.SelectedProfileElement);

            ProfileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            ProfileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged -= ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;

            PopulateProperties(null);
            base.OnClose();
        }

        protected override void OnDeactivate()
        {
            Pause();
            base.OnDeactivate();
        }

        private void HandlePropertyTreeIndexChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyTreeIndex) && PropertyTreeIndex == 1)
                EffectsViewModel.PopulateDescriptors();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            PopulateProperties(e.RenderProfileElement);
        }

        private void ProfileEditorServiceOnCurrentTimeChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(FormattedCurrentTime));
            NotifyOfPropertyChange(nameof(TimeCaretPosition));
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(TimeCaretPosition));
        }

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            RightSideIndex = ProfileEditorService.SelectedDataBinding != null ? 1 : 0;
        }

        #region View model managament

        public List<LayerPropertyGroupViewModel> GetAllLayerPropertyGroupViewModels()
        {
            List<LayerPropertyGroupViewModel> groups = LayerPropertyGroups.ToList();
            List<LayerPropertyGroupViewModel> toAdd = groups.SelectMany(g => g.Children).Where(g => g is LayerPropertyGroupViewModel).Cast<LayerPropertyGroupViewModel>().ToList();
            groups.AddRange(toAdd);
            return groups;
        }

        private void PopulateProperties(RenderProfileElement profileElement)
        {
            // Unsubscribe from old selected element
            if (SelectedProfileElement != null)
                SelectedProfileElement.LayerEffectsUpdated -= SelectedElementOnLayerEffectsUpdated;
            if (SelectedLayer != null)
                SelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;

            // Clear old properties
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.Dispose();
            LayerPropertyGroups.Clear();
            _brushPropertyGroup = null;

            if (profileElement == null)
                return;

            // Subscribe to new element
            SelectedProfileElement = profileElement;
            SelectedProfileElement.LayerEffectsUpdated += SelectedElementOnLayerEffectsUpdated;

            if (SelectedLayer != null)
            {
                SelectedLayer.LayerBrushUpdated += SelectedLayerOnLayerBrushUpdated;

                // Add the built-in root groups of the layer
                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.General));
                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.Transform));
            }

            ApplyLayerBrush();
            ApplyEffects();
        }

        private void SelectedLayerOnLayerBrushUpdated(object sender, EventArgs e)
        {
            ApplyLayerBrush();
        }

        private void SelectedElementOnLayerEffectsUpdated(object sender, EventArgs e)
        {
            ApplyEffects();
        }

        public void ApplyLayerBrush()
        {
            if (SelectedLayer == null)
                return;

            bool hideRenderRelatedProperties = SelectedLayer?.LayerBrush != null && !SelectedLayer.LayerBrush.SupportsTransformation;

            SelectedLayer.General.ShapeType.IsHidden = hideRenderRelatedProperties;
            SelectedLayer.General.BlendMode.IsHidden = hideRenderRelatedProperties;
            SelectedLayer.Transform.IsHidden = hideRenderRelatedProperties;

            if (_brushPropertyGroup != null)
            {
                LayerPropertyGroups.Remove(_brushPropertyGroup);
                _brushPropertyGroup = null;
            }

            if (SelectedLayer.LayerBrush != null)
            {
                _brushPropertyGroup = _layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.LayerBrush.BaseProperties);
                LayerPropertyGroups.Add(_brushPropertyGroup);
            }

            SortProperties();
        }

        private void ApplyEffects()
        {
            if (SelectedProfileElement == null)
                return;

            // Remove VMs of effects no longer applied on the layer
            List<LayerPropertyGroupViewModel> toRemove = LayerPropertyGroups
                .Where(l => l.LayerPropertyGroup.LayerEffect != null && !SelectedProfileElement.LayerEffects.Contains(l.LayerPropertyGroup.LayerEffect))
                .ToList();
            LayerPropertyGroups.RemoveRange(toRemove);
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in toRemove)
                layerPropertyGroupViewModel.Dispose();

            foreach (BaseLayerEffect layerEffect in SelectedProfileElement.LayerEffects)
            {
                if (LayerPropertyGroups.Any(l => l.LayerPropertyGroup.LayerEffect == layerEffect) || layerEffect.BaseProperties == null)
                    continue;

                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerEffect.BaseProperties));
            }

            SortProperties();
        }

        private void SortProperties()
        {
            // Get all non-effect properties
            List<LayerPropertyGroupViewModel> nonEffectProperties = LayerPropertyGroups
                .Where(l => l.TreeGroupViewModel.GroupType != LayerEffectRoot)
                .ToList();
            // Order the effects
            List<LayerPropertyGroupViewModel> effectProperties = LayerPropertyGroups
                .Where(l => l.TreeGroupViewModel.GroupType == LayerEffectRoot)
                .OrderBy(l => l.LayerPropertyGroup.LayerEffect.Order)
                .ToList();

            // Put the non-effect properties in front
            for (int index = 0; index < nonEffectProperties.Count; index++)
            {
                LayerPropertyGroupViewModel layerPropertyGroupViewModel = nonEffectProperties[index];
                LayerPropertyGroups.Move(LayerPropertyGroups.IndexOf(layerPropertyGroupViewModel), index);
            }

            // Put the effect properties after, sorted by their order
            for (int index = 0; index < effectProperties.Count; index++)
            {
                LayerPropertyGroupViewModel layerPropertyGroupViewModel = effectProperties[index];
                LayerPropertyGroups.Move(LayerPropertyGroups.IndexOf(layerPropertyGroupViewModel), index + nonEffectProperties.Count);
            }
        }

        public async void ToggleEffectsViewModel()
        {
            _lastEffectsViewModelToggle = DateTime.Now;
            NotifyOfPropertyChange(nameof(CanToggleEffectsViewModel));

            await Task.Delay(300);
            NotifyOfPropertyChange(nameof(CanToggleEffectsViewModel));
        }

        #endregion

        #region Drag and drop

        public void DragOver(IDropInfo dropInfo)
        {
            // Workaround for https://github.com/punker76/gong-wpf-dragdrop/issues/344
            // Luckily we know the index can never be 1 so it's an easy enough fix
            if (dropInfo.InsertIndex == 1)
                return;

            LayerPropertyGroupViewModel source = dropInfo.Data as LayerPropertyGroupViewModel;
            LayerPropertyGroupViewModel target = dropInfo.TargetItem as LayerPropertyGroupViewModel;

            if (source == target ||
                target?.TreeGroupViewModel.GroupType != LayerEffectRoot ||
                source?.TreeGroupViewModel.GroupType != LayerEffectRoot)
                return;

            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            // Workaround for https://github.com/punker76/gong-wpf-dragdrop/issues/344
            // Luckily we know the index can never be 1 so it's an easy enough fix
            if (dropInfo.InsertIndex == 1)
                return;

            LayerPropertyGroupViewModel source = dropInfo.Data as LayerPropertyGroupViewModel;
            LayerPropertyGroupViewModel target = dropInfo.TargetItem as LayerPropertyGroupViewModel;

            if (source == target ||
                target?.TreeGroupViewModel.GroupType != LayerEffectRoot ||
                source?.TreeGroupViewModel.GroupType != LayerEffectRoot)
                return;

            if (dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem)
                MoveBefore(source, target);
            else if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem)
                MoveAfter(source, target);

            ApplyCurrentEffectsOrder();
            ProfileEditorService.UpdateSelectedProfile();
        }

        private void MoveBefore(LayerPropertyGroupViewModel source, LayerPropertyGroupViewModel target)
        {
            if (LayerPropertyGroups.IndexOf(target) == LayerPropertyGroups.IndexOf(source) + 1)
                return;

            LayerPropertyGroups.Move(LayerPropertyGroups.IndexOf(source), LayerPropertyGroups.IndexOf(target));
        }

        private void MoveAfter(LayerPropertyGroupViewModel source, LayerPropertyGroupViewModel target)
        {
            LayerPropertyGroups.Remove(source);
            LayerPropertyGroups.Insert(LayerPropertyGroups.IndexOf(target) + 1, source);
        }

        private void ApplyCurrentEffectsOrder()
        {
            int order = 1;
            foreach (LayerPropertyGroupViewModel groupViewModel in LayerPropertyGroups.Where(p => p.TreeGroupViewModel.GroupType == LayerEffectRoot))
            {
                groupViewModel.UpdateOrder(order);
                order++;
            }
        }

        #endregion

        #region Controls

        public void PlayFromStart()
        {
            if (!Playing)
                ProfileEditorService.CurrentTime = TimeSpan.Zero;

            Play();
        }

        public void Play()
        {
            if (!IsActive)
                return;
            if (Playing)
            {
                Pause();
                return;
            }

            CoreService.FrameRendering += CoreServiceOnFrameRendering;
            Playing = true;
        }

        public void Pause()
        {
            if (!Playing)
                return;

            CoreService.FrameRendering -= CoreServiceOnFrameRendering;
            Playing = false;
        }


        public void GoToStart()
        {
            ProfileEditorService.CurrentTime = TimeSpan.Zero;
        }

        public void GoToEnd()
        {
            ProfileEditorService.CurrentTime = CalculateEndTime();
        }

        public void GoToPreviousFrame()
        {
            double frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            double newTime = Math.Max(0, Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        public void GoToNextFrame()
        {
            double frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            double newTime = Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
            newTime = Math.Min(newTime, CalculateEndTime().TotalMilliseconds);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        public void CycleRepeating()
        {
            if (!Repeating)
            {
                RepeatTimeline = true;
                RepeatSegment = false;
                Repeating = true;
            }
            else if (RepeatTimeline)
            {
                RepeatTimeline = false;
                RepeatSegment = true;
            }
            else if (RepeatSegment)
            {
                RepeatTimeline = true;
                RepeatSegment = false;
                Repeating = false;
            }
        }

        private TimeSpan CalculateEndTime()
        {
            List<ITimelineKeyframeViewModel> keyframeViewModels = LayerPropertyGroups.SelectMany(g => g.GetAllKeyframeViewModels(false)).ToList();

            // If there are no keyframes, don't stop at all
            if (!keyframeViewModels.Any())
                return TimeSpan.MaxValue;
            // If there are keyframes, stop after the last keyframe + 10 sec
            return keyframeViewModels.Max(k => k.Position).Add(TimeSpan.FromSeconds(10));
        }

        private TimeSpan GetCurrentSegmentStart()
        {
            TimeSpan current = ProfileEditorService.CurrentTime;
            if (current < StartTimelineSegmentViewModel.SegmentEnd)
                return StartTimelineSegmentViewModel.SegmentStart;
            if (current < MainTimelineSegmentViewModel.SegmentEnd)
                return MainTimelineSegmentViewModel.SegmentStart;
            if (current < EndTimelineSegmentViewModel.SegmentEnd)
                return EndTimelineSegmentViewModel.SegmentStart;

            return TimeSpan.Zero;
        }

        private TimeSpan GetCurrentSegmentEnd()
        {
            TimeSpan current = ProfileEditorService.CurrentTime;
            if (current < StartTimelineSegmentViewModel.SegmentEnd)
                return StartTimelineSegmentViewModel.SegmentEnd;
            if (current < MainTimelineSegmentViewModel.SegmentEnd)
                return MainTimelineSegmentViewModel.SegmentEnd;
            if (current < EndTimelineSegmentViewModel.SegmentEnd)
                return EndTimelineSegmentViewModel.SegmentEnd;

            return TimeSpan.Zero;
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                TimeSpan newTime = ProfileEditorService.CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
                if (SelectedProfileElement != null)
                {
                    if (Repeating && RepeatTimeline)
                    {
                        if (newTime > SelectedProfileElement.Timeline.Length)
                            newTime = TimeSpan.Zero;
                    }
                    else if (Repeating && RepeatSegment)
                    {
                        if (newTime > GetCurrentSegmentEnd())
                            newTime = GetCurrentSegmentStart();
                    }
                    else if (newTime > SelectedProfileElement.Timeline.Length)
                    {
                        newTime = SelectedProfileElement.Timeline.Length;
                        Pause();
                    }
                }

                ProfileEditorService.CurrentTime = newTime;
            });
        }

        #endregion

        #region Caret movement

        public void TimelineMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
        }

        public void TimelineMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the parent grid, need that for our position
                IInputElement parent = (IInputElement) VisualTreeHelper.GetParent((DependencyObject) sender);
                double x = Math.Max(0, e.GetPosition(parent).X);
                TimeSpan newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

                // Round the time to something that fits the current zoom level
                if (ProfileEditorService.PixelsPerSecond < 200)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
                else if (ProfileEditorService.PixelsPerSecond < 500)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
                else
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

                // If holding down shift, snap to the closest segment or keyframe
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    List<TimeSpan> snapTimes = LayerPropertyGroups.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
                    TimeSpan snappedTime = ProfileEditorService.SnapToTimeline(newTime, TimeSpan.FromMilliseconds(1000f / ProfileEditorService.PixelsPerSecond * 5), true, false, snapTimes);
                    ProfileEditorService.CurrentTime = snappedTime;
                    return;
                }

                // If holding down control, round to the closest 50ms
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    TimeSpan roundedTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);
                    ProfileEditorService.CurrentTime = roundedTime;
                    return;
                }

                ProfileEditorService.CurrentTime = newTime;
            }
        }

        #endregion
    }
}