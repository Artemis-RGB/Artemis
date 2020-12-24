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
    public class LayerPropertiesViewModel : Conductor<LayerPropertyGroupViewModel>.Collection.AllActive, IProfileEditorPanelViewModel, IDropTarget
    {
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private LayerPropertyGroupViewModel _brushPropertyGroup;
        private bool _repeating;
        private bool _repeatSegment;
        private bool _repeatTimeline = true;
        private int _propertyTreeIndex;
        private int _rightSideIndex;
        private RenderProfileElement _selectedProfileElement;
        private DateTime _lastEffectsViewModelToggle;
        private double _treeViewModelHeight;

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

            PropertyChanged += HandlePropertyTreeIndexChanged;

            // Left side 
            TreeViewModel = _layerPropertyVmFactory.TreeViewModel(this, Items);
            TreeViewModel.ConductWith(this);
            EffectsViewModel = _layerPropertyVmFactory.EffectsViewModel(this);
            EffectsViewModel.ConductWith(this);

            // Right side
            StartTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.Start, Items);
            StartTimelineSegmentViewModel.ConductWith(this);
            MainTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.Main, Items);
            MainTimelineSegmentViewModel.ConductWith(this);
            EndTimelineSegmentViewModel = _layerPropertyVmFactory.TimelineSegmentViewModel(SegmentViewModelType.End, Items);
            EndTimelineSegmentViewModel.ConductWith(this);
            TimelineViewModel = _layerPropertyVmFactory.TimelineViewModel(this, Items);
            TimelineViewModel.ConductWith(this);
            DataBindingsViewModel = dataBindingsViewModel;
            DataBindingsViewModel.ConductWith(this);
        }

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
            get => ProfileEditorService.Playing;
            set
            {
                ProfileEditorService.Playing = value;
                NotifyOfPropertyChange(nameof(Playing));
            }
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

        public double TreeViewModelHeight
        {
            get => _treeViewModelHeight;
            set => SetAndNotify(ref _treeViewModelHeight, value);
        }


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
            List<LayerPropertyGroupViewModel> groups = Items.ToList();
            List<LayerPropertyGroupViewModel> toAdd = groups.SelectMany(g => g.Items).Where(g => g is LayerPropertyGroupViewModel).Cast<LayerPropertyGroupViewModel>().ToList();
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
            Items.Clear();
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
                Items.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.General));
                Items.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.Transform));
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
                Items.Remove(_brushPropertyGroup);
                _brushPropertyGroup = null;
            }

            if (SelectedLayer.LayerBrush != null)
            {
                _brushPropertyGroup = _layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.LayerBrush.BaseProperties);
                Items.Add(_brushPropertyGroup);
            }

            SortProperties();
        }

        private void ApplyEffects()
        {
            if (SelectedProfileElement == null)
                return;

            // Remove VMs of effects no longer applied on the layer
            List<LayerPropertyGroupViewModel> toRemove = Items
                .Where(l => l.LayerPropertyGroup.LayerEffect != null && !SelectedProfileElement.LayerEffects.Contains(l.LayerPropertyGroup.LayerEffect))
                .ToList();
            Items.RemoveRange(toRemove);
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in toRemove)
                layerPropertyGroupViewModel.RequestClose();

            foreach (BaseLayerEffect layerEffect in SelectedProfileElement.LayerEffects)
            {
                if (Items.Any(l => l.LayerPropertyGroup.LayerEffect == layerEffect) || layerEffect.BaseProperties == null)
                    continue;

                Items.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerEffect.BaseProperties));
            }

            SortProperties();
        }

        private void SortProperties()
        {
            // Get all non-effect properties
            List<LayerPropertyGroupViewModel> nonEffectProperties = Items
                .Where(l => l.TreeGroupViewModel.GroupType != LayerEffectRoot)
                .ToList();
            // Order the effects
            List<LayerPropertyGroupViewModel> effectProperties = Items
                .Where(l => l.TreeGroupViewModel.GroupType == LayerEffectRoot)
                .OrderBy(l => l.LayerPropertyGroup.LayerEffect.Order)
                .ToList();

            // Put the non-effect properties in front
            for (int index = 0; index < nonEffectProperties.Count; index++)
            {
                LayerPropertyGroupViewModel layerPropertyGroupViewModel = nonEffectProperties[index];
                ((BindableCollection<LayerPropertyGroupViewModel>) Items).Move(Items.IndexOf(layerPropertyGroupViewModel), index);
            }

            // Put the effect properties after, sorted by their order
            for (int index = 0; index < effectProperties.Count; index++)
            {
                LayerPropertyGroupViewModel layerPropertyGroupViewModel = effectProperties[index];
                ((BindableCollection<LayerPropertyGroupViewModel>) Items).Move(Items.IndexOf(layerPropertyGroupViewModel), index + nonEffectProperties.Count);
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
            if (Items.IndexOf(target) == Items.IndexOf(source) + 1)
                return;

            ((BindableCollection<LayerPropertyGroupViewModel>) Items).Move(Items.IndexOf(source), Items.IndexOf(target));
        }

        private void MoveAfter(LayerPropertyGroupViewModel source, LayerPropertyGroupViewModel target)
        {
            Items.Remove(source);
            Items.Insert(Items.IndexOf(target) + 1, source);
        }

        private void ApplyCurrentEffectsOrder()
        {
            int order = 1;
            foreach (LayerPropertyGroupViewModel groupViewModel in Items.Where(p => p.TreeGroupViewModel.GroupType == LayerEffectRoot))
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
            ProfileEditorService.CurrentTime = SelectedProfileElement.Timeline.EndSegmentEndPosition;
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
            newTime = Math.Min(newTime, SelectedProfileElement.Timeline.EndSegmentEndPosition.TotalMilliseconds);
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
                    List<TimeSpan> snapTimes = Items.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
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

        public void TimelineJump(object sender, MouseButtonEventArgs e)
        {
            // Get the parent grid, need that for our position
            IInputElement parent = (IInputElement)VisualTreeHelper.GetParent((DependencyObject)sender);
            double x = Math.Max(0, e.GetPosition(parent).X);
            TimeSpan newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

            // Round the time to something that fits the current zoom level
            if (ProfileEditorService.PixelsPerSecond < 200)
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
            else if (ProfileEditorService.PixelsPerSecond < 500)
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
            else
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

            ProfileEditorService.CurrentTime = newTime;
        }

        #endregion
    }
}