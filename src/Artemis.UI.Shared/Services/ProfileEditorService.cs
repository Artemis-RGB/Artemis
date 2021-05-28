using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Shared.Services.Models;
using Ninject;
using Ninject.Parameters;
using Serilog;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    internal class ProfileEditorService : IProfileEditorService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IProfileService _profileService;
        private readonly List<PropertyInputRegistration> _registeredPropertyEditors;
        private readonly IRgbService _rgbService;
        private readonly IModuleService _moduleService;
        private readonly object _selectedProfileElementLock = new();
        private readonly object _selectedProfileLock = new();
        private TimeSpan _currentTime;
        private bool _doTick;
        private int _pixelsPerSecond;

        public ProfileEditorService(IKernel kernel, ILogger logger, IProfileService profileService, ICoreService coreService, IRgbService rgbService, IModuleService moduleService)
        {
            _kernel = kernel;
            _logger = logger;
            _profileService = profileService;
            _rgbService = rgbService;
            _moduleService = moduleService;
            _registeredPropertyEditors = new List<PropertyInputRegistration>();
            coreService.FrameRendered += CoreServiceOnFrameRendered;
            PixelsPerSecond = 100;
        }

        private void CoreServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
        {
            if (!_doTick) return;
            _doTick = false;
            Execute.PostToUIThread(OnProfilePreviewUpdated);
        }

        private void ReloadProfile()
        {
            if (SelectedProfile == null)
                return;

            // Trigger a profile change
            OnSelectedProfileChanged(new ProfileConfigurationEventArgs(SelectedProfileConfiguration, SelectedProfileConfiguration));
            // Trigger a selected element change
            RenderProfileElement? previousSelectedProfileElement = SelectedProfileElement;
            if (SelectedProfileElement is Folder folder)
                SelectedProfileElement = SelectedProfile.GetAllFolders().FirstOrDefault(f => f.EntityId == folder.EntityId);
            else if (SelectedProfileElement is Layer layer)
                SelectedProfileElement = SelectedProfile.GetAllLayers().FirstOrDefault(l => l.EntityId == layer.EntityId);
            OnSelectedProfileElementChanged(new RenderProfileElementEventArgs(SelectedProfileElement, previousSelectedProfileElement));
            // Trigger selected data binding change
            if (SelectedDataBinding != null)
            {
                SelectedDataBinding = SelectedProfileElement?.GetAllLayerProperties().FirstOrDefault(p => p.Path == SelectedDataBinding.Path);
                OnSelectedDataBindingChanged();
            }

            UpdateProfilePreview();
        }

        private void Tick()
        {
            if (SelectedProfile == null || _doTick)
                return;

            TickProfileElement(SelectedProfile.GetRootFolder());
            _doTick = true;
        }

        private void TickProfileElement(ProfileElement profileElement)
        {
            if (profileElement is not RenderProfileElement renderElement)
                return;

            if (renderElement.Suspended)
                renderElement.Disable();
            else
            {
                renderElement.Enable();
                renderElement.Timeline.Override(
                    CurrentTime,
                    (renderElement != SelectedProfileElement || renderElement.Timeline.Length < CurrentTime) && renderElement.Timeline.PlayMode == TimelinePlayMode.Repeat
                );

                foreach (ProfileElement child in renderElement.Children)
                    TickProfileElement(child);
            }
        }

        public ReadOnlyCollection<PropertyInputRegistration> RegisteredPropertyEditors => _registeredPropertyEditors.AsReadOnly();

        public bool Playing { get; set; }
        public ProfileConfiguration? SelectedProfileConfiguration { get; private set; }
        public Profile? SelectedProfile => SelectedProfileConfiguration?.Profile;
        public RenderProfileElement? SelectedProfileElement { get; private set; }
        public ILayerProperty? SelectedDataBinding { get; private set; }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime.Equals(value)) return;
                _currentTime = value;
                Tick();
                OnCurrentTimeChanged();
            }
        }

        public int PixelsPerSecond
        {
            get => _pixelsPerSecond;
            set
            {
                _pixelsPerSecond = value;
                OnPixelsPerSecondChanged();
            }
        }

        public void ChangeSelectedProfileConfiguration(ProfileConfiguration? profileConfiguration)
        {
            lock (_selectedProfileLock)
            {
                if (profileConfiguration?.Profile != null && profileConfiguration.Profile.Disposed)
                    throw new ArtemisSharedUIException("Cannot select a disposed profile");
                if (SelectedProfileConfiguration == profileConfiguration)
                    return;

                _logger.Verbose("ChangeSelectedProfileConfiguration {profile}", profileConfiguration);
                ChangeSelectedProfileElement(null);
                ProfileConfigurationEventArgs profileConfigurationElementEvent = new(profileConfiguration, SelectedProfileConfiguration);

                // No need to deactivate the profile, if needed it will be deactivated next update
                if (SelectedProfileConfiguration != null)
                    SelectedProfileConfiguration.IsBeingEdited = false;

                // The new profile may need activation
                SelectedProfileConfiguration = profileConfiguration;
                if (SelectedProfileConfiguration != null)
                {
                    SelectedProfileConfiguration.IsBeingEdited = true;
                    _moduleService.SetActivationOverride(SelectedProfileConfiguration.Modules);
                    _profileService.ActivateProfile(SelectedProfileConfiguration);
                    _profileService.RenderForEditor = true;
                }
                else
                {
                    _moduleService.SetActivationOverride(Enumerable.Empty<Module>());
                    _profileService.RenderForEditor = false;
                }

                OnSelectedProfileChanged(profileConfigurationElementEvent);
                UpdateProfilePreview();
            }
        }

        public void SaveSelectedProfileConfiguration()
        {
            lock (_selectedProfileLock)
            {
                _logger.Verbose("SaveSelectedProfileConfiguration {profile}", SelectedProfile);
                if (SelectedProfile == null)
                    return;

                _profileService.SaveProfile(SelectedProfile, true);
                OnSelectedProfileUpdated(new ProfileConfigurationEventArgs(SelectedProfileConfiguration));
                UpdateProfilePreview();
            }
        }

        public void ChangeSelectedProfileElement(RenderProfileElement? profileElement)
        {
            lock (_selectedProfileElementLock)
            {
                if (SelectedProfileElement == profileElement)
                    return;

                _logger.Verbose("ChangeSelectedProfileElement {profile}", profileElement);
                RenderProfileElementEventArgs profileElementEvent = new(profileElement, SelectedProfileElement);
                SelectedProfileElement = profileElement;
                OnSelectedProfileElementChanged(profileElementEvent);

                ChangeSelectedDataBinding(null);
            }
        }

        public void SaveSelectedProfileElement()
        {
            lock (_selectedProfileElementLock)
            {
                _logger.Verbose("SaveSelectedProfileElement {profile}", SelectedProfileElement);
                if (SelectedProfile == null)
                    return;

                _profileService.SaveProfile(SelectedProfile, true);
                OnSelectedProfileElementUpdated(new RenderProfileElementEventArgs(SelectedProfileElement));
                UpdateProfilePreview();
            }
        }

        public void ChangeSelectedDataBinding(ILayerProperty? layerProperty)
        {
            SelectedDataBinding = layerProperty;
            OnSelectedDataBindingChanged();
        }

        public void UpdateProfilePreview()
        {
            if (Playing)
                return;
            Tick();
        }

        public bool UndoSaveProfile()
        {
            if (SelectedProfile == null)
                return false;

            bool undid = _profileService.UndoSaveProfile(SelectedProfile);
            if (!undid)
                return false;

            ReloadProfile();
            return true;
        }

        public bool RedoSaveProfile()
        {
            if (SelectedProfile == null)
                return false;

            bool redid = _profileService.RedoSaveProfile(SelectedProfile);
            if (!redid)
                return false;

            ReloadProfile();
            return true;
        }

        public PropertyInputRegistration RegisterPropertyInput<T>(Plugin plugin) where T : PropertyInputViewModel
        {
            return RegisterPropertyInput(typeof(T), plugin);
        }

        public PropertyInputRegistration RegisterPropertyInput(Type viewModelType, Plugin plugin)
        {
            if (!typeof(PropertyInputViewModel).IsAssignableFrom(viewModelType))
                throw new ArtemisSharedUIException($"Property input VM type must implement {nameof(PropertyInputViewModel)}");

            lock (_registeredPropertyEditors)
            {
                // Indirectly checked if there's a BaseType above
                Type supportedType = viewModelType.BaseType!.GetGenericArguments()[0];
                // If the supported type is a generic, assume there is a base type
                if (supportedType.IsGenericParameter)
                {
                    if (supportedType.BaseType == null)
                        throw new ArtemisSharedUIException("Generic property input VM type must have a type constraint");
                    supportedType = supportedType.BaseType;
                }

                PropertyInputRegistration? existing = _registeredPropertyEditors.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.Plugin != plugin)
                    {
                        throw new ArtemisSharedUIException($"Cannot register property editor for type {supportedType.Name} because an editor was already " +
                                                           $"registered by {existing.Plugin}");
                    }

                    return existing;
                }

                _kernel.Bind(viewModelType).ToSelf();
                PropertyInputRegistration registration = new(this, plugin, supportedType, viewModelType);
                _registeredPropertyEditors.Add(registration);
                return registration;
            }
        }

        public void RemovePropertyInput(PropertyInputRegistration registration)
        {
            lock (_registeredPropertyEditors)
            {
                if (_registeredPropertyEditors.Contains(registration))
                {
                    registration.Unsubscribe();
                    _registeredPropertyEditors.Remove(registration);

                    _kernel.Unbind(registration.ViewModelType);
                }
            }
        }

        public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null)
        {
            if (snapToSegments && SelectedProfileElement != null)
            {
                // Snap to the end of the start segment
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.Timeline.StartSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.Timeline.StartSegmentEndPosition;

                // Snap to the end of the main segment
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.Timeline.MainSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.Timeline.MainSegmentEndPosition;

                // Snap to the end of the end segment (end of the timeline)
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.Timeline.EndSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.Timeline.EndSegmentEndPosition;
            }

            if (snapToCurrentTime)
                // Snap to the current time
            {
                if (Math.Abs(time.TotalMilliseconds - CurrentTime.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return CurrentTime;
            }

            if (snapTimes != null)
            {
                // Find the closest keyframe
                TimeSpan closeSnapTime = snapTimes.FirstOrDefault(s => Math.Abs(time.TotalMilliseconds - s.TotalMilliseconds) < tolerance.TotalMilliseconds)!;
                if (closeSnapTime != TimeSpan.Zero)
                    return closeSnapTime;
            }

            return time;
        }

        public bool CanCreatePropertyInputViewModel(ILayerProperty layerProperty)
        {
            PropertyInputRegistration? registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == layerProperty.PropertyType);
            if (registration == null && layerProperty.PropertyType.IsEnum)
                registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(Enum));

            return registration != null;
        }

        public PropertyInputViewModel<T>? CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty)
        {
            Type? viewModelType = null;
            PropertyInputRegistration? registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(T));

            // Check for enums if no supported type was found
            if (registration == null && typeof(T).IsEnum)
            {
                // The enum VM will likely be a generic, that requires creating a generic type matching the layer property
                registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(Enum));
                if (registration != null && registration.ViewModelType.IsGenericType)
                    viewModelType = registration.ViewModelType.MakeGenericType(layerProperty.GetType().GenericTypeArguments);
            }
            else if (registration != null)
                viewModelType = registration.ViewModelType;
            else
                return null;

            if (viewModelType == null)
                return null;

            ConstructorArgument parameter = new("layerProperty", layerProperty);
            // ReSharper disable once InconsistentlySynchronizedField
            // When you've just spent the last 2 hours trying to figure out a deadlock and reach this line, I'm so, so sorry. I thought this would be fine.
            IKernel kernel = registration?.Plugin.Kernel ?? _kernel;
            return (PropertyInputViewModel<T>) kernel.Get(viewModelType, parameter);
        }

        public List<ArtemisLed> GetLedsInRectangle(Rect rect)
        {
            return _rgbService.EnabledDevices
                .SelectMany(d => d.Leds)
                .Where(led => led.AbsoluteRectangle.IntersectsWith(SKRectI.Round(rect.ToSKRect())))
                .ToList();
        }

        #region Copy/paste

        public ProfileElement? DuplicateProfileElement(ProfileElement profileElement)
        {
            if (!(profileElement.Parent is Folder parent))
                return null;

            object? clipboardModel = null;
            switch (profileElement)
            {
                case Folder folder:
                {
                    clipboardModel = CoreJson.DeserializeObject(CoreJson.SerializeObject(new FolderClipboardModel(folder), true), true);
                    break;
                }
                case Layer layer:
                    clipboardModel = CoreJson.DeserializeObject(CoreJson.SerializeObject(layer.LayerEntity, true), true);
                    break;
            }

            return clipboardModel == null ? null : PasteClipboardData(clipboardModel, parent, profileElement.Order);
        }

        public void CopyProfileElement(ProfileElement profileElement)
        {
            switch (profileElement)
            {
                case Folder folder:
                {
                    FolderClipboardModel clipboardModel = new(folder);
                    JsonClipboard.SetObject(clipboardModel);
                    break;
                }
                case Layer layer:
                    JsonClipboard.SetObject(layer.LayerEntity);
                    break;
            }
        }

        public ProfileElement? PasteProfileElement(Folder target, int position)
        {
            object? clipboardObject = JsonClipboard.GetData();
            return clipboardObject != null ? PasteClipboardData(clipboardObject, target, position) : null;
        }

        public bool GetCanPasteProfileElement()
        {
            object? clipboardObject = JsonClipboard.GetData();
            return clipboardObject is LayerEntity || clipboardObject is FolderClipboardModel;
        }

        private RenderProfileElement? PasteClipboardData(object clipboardObject, Folder target, int position)
        {
            RenderProfileElement? pasted = null;
            switch (clipboardObject)
            {
                case FolderClipboardModel folderClipboardModel:
                    pasted = folderClipboardModel.Paste(target.Profile, target);
                    target.AddChild(pasted, position);
                    break;
                case LayerEntity layerEntity:
                    layerEntity.Id = Guid.NewGuid();
                    layerEntity.Name += " - copy";
                    pasted = new Layer(target.Profile, target, layerEntity);
                    target.AddChild(pasted, position);
                    break;
            }

            if (pasted != null)
            {
                target.Profile.PopulateLeds(_rgbService.EnabledDevices);
                SaveSelectedProfileConfiguration();
                ChangeSelectedProfileElement(pasted);
            }

            return pasted;
        }

        #endregion

        #region Events

        public event EventHandler<ProfileConfigurationEventArgs>? SelectedProfileChanged;
        public event EventHandler<ProfileConfigurationEventArgs>? SelectedProfileSaved;
        public event EventHandler<RenderProfileElementEventArgs>? SelectedProfileElementChanged;
        public event EventHandler<RenderProfileElementEventArgs>? SelectedProfileElementSaved;
        public event EventHandler? SelectedDataBindingChanged;
        public event EventHandler? CurrentTimeChanged;
        public event EventHandler? PixelsPerSecondChanged;
        public event EventHandler? ProfilePreviewUpdated;

        protected virtual void OnSelectedProfileChanged(ProfileConfigurationEventArgs e)
        {
            SelectedProfileChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileUpdated(ProfileConfigurationEventArgs e)
        {
            SelectedProfileSaved?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementChanged(RenderProfileElementEventArgs e)
        {
            SelectedProfileElementChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementUpdated(RenderProfileElementEventArgs e)
        {
            SelectedProfileElementSaved?.Invoke(this, e);
        }

        protected virtual void OnCurrentTimeChanged()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPixelsPerSecondChanged()
        {
            PixelsPerSecondChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnProfilePreviewUpdated()
        {
            ProfilePreviewUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedDataBindingChanged()
        {
            SelectedDataBindingChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}