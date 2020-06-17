using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;
using GongSolutions.Wpf.DragDrop;
using Stylet;
using static Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.LayerPropertyGroupViewModel.ViewModelType;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel, IDropTarget
    {
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private LayerPropertyGroupViewModel _brushPropertyGroup;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService, ICoreService coreService, ISettingsService settingsService,
            ILayerPropertyVmFactory layerPropertyVmFactory)
        {
            _layerPropertyVmFactory = layerPropertyVmFactory;

            ProfileEditorService = profileEditorService;
            CoreService = coreService;
            SettingsService = settingsService;

            EffectsViewModel = _layerPropertyVmFactory.EffectsViewModel(this);
            LayerPropertyGroups = new BindableCollection<LayerPropertyGroupViewModel>();
            PropertyChanged += HandlePropertyTreeIndexChanged;
        }

        public IProfileEditorService ProfileEditorService { get; }
        public ICoreService CoreService { get; }
        public ISettingsService SettingsService { get; }

        public bool Playing { get; set; }
        public bool RepeatAfterLastKeyframe { get; set; }
        public string FormattedCurrentTime => $"{Math.Floor(ProfileEditorService.CurrentTime.TotalSeconds):00}.{ProfileEditorService.CurrentTime.Milliseconds:000}";

        public Thickness TimeCaretPosition
        {
            get => new Thickness(ProfileEditorService.CurrentTime.TotalSeconds * ProfileEditorService.PixelsPerSecond, 0, 0, 0);
            set => ProfileEditorService.CurrentTime = TimeSpan.FromSeconds(value.Left / ProfileEditorService.PixelsPerSecond);
        }

        public int PropertyTreeIndex { get; set; }
        public bool PropertyTreeVisible => PropertyTreeIndex == 0;

        public PropertiesProfileElement SelectedPropertiesElement { get; set; }
        public Layer SelectedLayer => SelectedPropertiesElement as Layer;
        public Folder SelectedFolder => SelectedPropertiesElement as Folder;

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; set; }
        public TreeViewModel TreeViewModel { get; set; }
        public EffectsViewModel EffectsViewModel { get; set; }
        public TimelineViewModel TimelineViewModel { get; set; }

        protected override void OnInitialActivate()
        {
            if (ProfileEditorService.SelectedProfileElement is PropertiesProfileElement propertiesElement)
                PopulateProperties(propertiesElement);

            ProfileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            ProfileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged -= ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;

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

        private void ProfileEditorServiceOnProfileElementSelected(object sender, ProfileElementEventArgs e)
        {
            PopulateProperties(e.ProfileElement as PropertiesProfileElement);
        }


        private void ProfileEditorServiceOnCurrentTimeChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => FormattedCurrentTime);
            NotifyOfPropertyChange(() => TimeCaretPosition);
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(TimeCaretPosition));
        }

        #region View model managament

        public List<LayerPropertyGroupViewModel> GetAllLayerPropertyGroupViewModels()
        {
            var groups = LayerPropertyGroups.ToList();
            groups.AddRange(groups.SelectMany(g => g.Children).Where(g => g is LayerPropertyGroupViewModel).Cast<LayerPropertyGroupViewModel>());
            return groups;
        }

        private void PopulateProperties(PropertiesProfileElement profileElement)
        {
            if (SelectedPropertiesElement != null && SelectedPropertiesElement is EffectProfileElement effectElement)
                effectElement.LayerEffectsUpdated -= SelectedElementOnLayerEffectsUpdated;
            if (SelectedLayer != null)
                SelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;

            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.Dispose();
            LayerPropertyGroups.Clear();
            _brushPropertyGroup = null;

            SelectedPropertiesElement = profileElement;
            if (SelectedPropertiesElement is EffectProfileElement newEffectElement)
                newEffectElement.LayerEffectsUpdated += SelectedElementOnLayerEffectsUpdated;

            // Apply layer properties
            if (SelectedLayer != null)
            {
                SelectedLayer.LayerBrushUpdated += SelectedLayerOnLayerBrushUpdated;

                // Add the built-in root groups of the layer
                var generalAttribute = Attribute.GetCustomAttribute(
                    SelectedLayer.GetType().GetProperty(nameof(SelectedLayer.General)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                var transformAttribute = Attribute.GetCustomAttribute(
                    SelectedLayer.GetType().GetProperty(nameof(SelectedLayer.Transform)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.General, (PropertyGroupDescriptionAttribute) generalAttribute));
                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.Transform, (PropertyGroupDescriptionAttribute) transformAttribute));
            }

            TreeViewModel = _layerPropertyVmFactory.TreeViewModel(this, LayerPropertyGroups);
            TimelineViewModel = _layerPropertyVmFactory.TimelineViewModel(this, LayerPropertyGroups);

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

            var hideRenderRelatedProperties = SelectedLayer?.LayerBrush != null && !SelectedLayer.LayerBrush.SupportsTransformation;

            SelectedLayer.General.ShapeType.IsHidden = hideRenderRelatedProperties;
            SelectedLayer.General.FillType.IsHidden = hideRenderRelatedProperties;
            SelectedLayer.General.BlendMode.IsHidden = hideRenderRelatedProperties;
            SelectedLayer.Transform.IsHidden = hideRenderRelatedProperties;

            if (_brushPropertyGroup != null)
            {
                LayerPropertyGroups.Remove(_brushPropertyGroup);
                _brushPropertyGroup = null;
            }

            if (SelectedLayer.LayerBrush != null)
            {
                // Add the rout group of the brush
                // The root group of the brush has no attribute so let's pull one out of our sleeve
                var brushDescription = new PropertyGroupDescriptionAttribute
                {
                    Name = SelectedLayer.LayerBrush.Descriptor.DisplayName,
                    Description = SelectedLayer.LayerBrush.Descriptor.Description
                };
                _brushPropertyGroup = _layerPropertyVmFactory.LayerPropertyGroupViewModel(SelectedLayer.LayerBrush.BaseProperties, brushDescription);
                LayerPropertyGroups.Add(_brushPropertyGroup);
            }

            SortProperties();
            TimelineViewModel.UpdateKeyframes();
        }

        private void ApplyEffects()
        {
            EffectProfileElement effectElement;
            if (SelectedLayer != null)
                effectElement = SelectedLayer;
            else if (SelectedFolder != null)
                effectElement = SelectedFolder;
            else
                return;

            // Remove VMs of effects no longer applied on the layer
            var toRemove = LayerPropertyGroups.Where(l => l.LayerPropertyGroup.LayerEffect != null && !effectElement.LayerEffects.Contains(l.LayerPropertyGroup.LayerEffect)).ToList();
            LayerPropertyGroups.RemoveRange(toRemove);
            foreach (var layerPropertyGroupViewModel in toRemove)
                layerPropertyGroupViewModel.Dispose();

            foreach (var layerEffect in effectElement.LayerEffects)
            {
                if (LayerPropertyGroups.Any(l => l.LayerPropertyGroup.LayerEffect == layerEffect))
                    continue;

                // Add the rout group of the brush
                // The root group of the brush has no attribute so let's pull one out of our sleeve
                var brushDescription = new PropertyGroupDescriptionAttribute
                {
                    Name = layerEffect.Descriptor.DisplayName,
                    Description = layerEffect.Descriptor.Description
                };
                LayerPropertyGroups.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerEffect.BaseProperties, brushDescription));
            }

            SortProperties();
            TimelineViewModel.UpdateKeyframes();
        }

        private void SortProperties()
        {
            // Get all non-effect properties
            var nonEffectProperties = LayerPropertyGroups.Where(l => l.GroupType != LayerEffectRoot).ToList();
            // Order the effects
            var effectProperties = LayerPropertyGroups.Where(l => l.GroupType == LayerEffectRoot).OrderBy(l => l.LayerPropertyGroup.LayerEffect.Order).ToList();
            
            // Put the non-effect properties in front
            for (var index = 0; index < nonEffectProperties.Count; index++)
            {
                var layerPropertyGroupViewModel = nonEffectProperties[index];
                LayerPropertyGroups.Move(LayerPropertyGroups.IndexOf(layerPropertyGroupViewModel), index);
            }

            // Put the effect properties after, sorted by their order
            for (var index = 0; index < effectProperties.Count; index++)
            {
                var layerPropertyGroupViewModel = effectProperties[index];
                LayerPropertyGroups.Move(LayerPropertyGroups.IndexOf(layerPropertyGroupViewModel), index + nonEffectProperties.Count);
            }
        }

        #endregion

        #region Drag and drop

        public void DragOver(IDropInfo dropInfo)
        {
            // Workaround for https://github.com/punker76/gong-wpf-dragdrop/issues/344
            // Luckily we know the index can never be 1 so it's an easy enough fix
            if (dropInfo.InsertIndex == 1)
                return;

            var source = dropInfo.Data as LayerPropertyGroupViewModel;
            var target = dropInfo.TargetItem as LayerPropertyGroupViewModel;

            if (source == target || target?.GroupType != LayerEffectRoot || source?.GroupType != LayerEffectRoot)
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

            var source = dropInfo.Data as LayerPropertyGroupViewModel;
            var target = dropInfo.TargetItem as LayerPropertyGroupViewModel;

            if (source == target || target?.GroupType != LayerEffectRoot || source?.GroupType != LayerEffectRoot)
                return;

            if (dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem)
                MoveBefore(source, target);
            else if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem)
                MoveAfter(source, target);

            ApplyCurrentEffectsOrder();
            ProfileEditorService.UpdateSelectedProfile(true);
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
            var order = 1;
            foreach (var groupViewModel in LayerPropertyGroups.Where(p => p.GroupType == LayerEffectRoot))
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
            var frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Max(0, Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        public void GoToNextFrame()
        {
            var frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
            newTime = Math.Min(newTime, CalculateEndTime().TotalMilliseconds);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        private TimeSpan CalculateEndTime()
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return TimeSpan.MaxValue;

            var keyframes = GetKeyframes(false);

            // If there are no keyframes, don't stop at all
            if (!keyframes.Any())
                return TimeSpan.MaxValue;
            // If there are keyframes, stop after the last keyframe + 10 sec
            return keyframes.Max(k => k.Position).Add(TimeSpan.FromSeconds(10));
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                var newTime = ProfileEditorService.CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
                if (RepeatAfterLastKeyframe)
                {
                    if (newTime > CalculateEndTime().Subtract(TimeSpan.FromSeconds(10)))
                        newTime = TimeSpan.Zero;
                }
                else if (newTime > CalculateEndTime())
                {
                    newTime = CalculateEndTime();
                    Pause();
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
                var parent = (IInputElement) VisualTreeHelper.GetParent((DependencyObject) sender);
                var x = Math.Max(0, e.GetPosition(parent).X);
                var newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

                // Round the time to something that fits the current zoom level
                if (ProfileEditorService.PixelsPerSecond < 200)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
                else if (ProfileEditorService.PixelsPerSecond < 500)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
                else
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    ProfileEditorService.CurrentTime = newTime;
                    return;
                }

                var visibleKeyframes = GetKeyframes(true);

                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / ProfileEditorService.PixelsPerSecond * 5;
                var closeKeyframe = visibleKeyframes.FirstOrDefault(k => Math.Abs(k.Position.TotalMilliseconds - newTime.TotalMilliseconds) < tolerance);
                ProfileEditorService.CurrentTime = closeKeyframe?.Position ?? newTime;
            }
        }

        private List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly)
        {
            var result = new List<BaseLayerPropertyKeyframe>();
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                result.AddRange(layerPropertyGroupViewModel.GetKeyframes(visibleOnly));

            return result;
        }

        #endregion
    }
}