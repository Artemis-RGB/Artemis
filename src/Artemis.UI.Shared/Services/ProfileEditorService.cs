using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Ninject;
using Ninject.Parameters;
using Serilog;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    internal class ProfileEditorService : IProfileEditorService
    {
        private readonly ILogger _logger;
        private readonly IProfileService _profileService;
        private readonly List<PropertyInputRegistration> _registeredPropertyEditors;
        private readonly object _selectedProfileElementLock = new object();
        private readonly object _selectedProfileLock = new object();
        private TimeSpan _currentTime;
        private int _pixelsPerSecond;

        public ProfileEditorService(IProfileService profileService, IKernel kernel, ILogger logger)
        {
            _profileService = profileService;
            _logger = logger;
            _registeredPropertyEditors = new List<PropertyInputRegistration>();

            Kernel = kernel;
            PixelsPerSecond = 100;
        }

        public IKernel Kernel { get; }
        public IReadOnlyList<PropertyInputRegistration> RegisteredPropertyEditors => _registeredPropertyEditors.AsReadOnly();
        public Profile SelectedProfile { get; private set; }
        public RenderProfileElement SelectedProfileElement { get; private set; }
        public ILayerProperty SelectedDataBinding { get; private set; }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime.Equals(value)) return;
                if (SelectedProfileElement != null && value > SelectedProfileElement.TimelineLength)
                    _currentTime = SelectedProfileElement.TimelineLength;
                else
                    _currentTime = value;
                UpdateProfilePreview();
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

        public void ChangeSelectedProfile(Profile profile)
        {
            lock (_selectedProfileLock)
            {
                if (SelectedProfile == profile)
                    return;

                if (profile != null && !profile.IsActivated)
                    throw new ArtemisSharedUIException("Cannot change the selected profile to an inactive profile");

                _logger.Verbose("ChangeSelectedProfile {profile}", profile);
                ChangeSelectedProfileElement(null);

                ProfileEventArgs profileElementEvent = new ProfileEventArgs(profile, SelectedProfile);

                // Ensure there is never a deactivated profile as the selected profile
                if (SelectedProfile != null)
                    SelectedProfile.Deactivated -= SelectedProfileOnDeactivated;
                SelectedProfile = profile;
                if (SelectedProfile != null)
                    SelectedProfile.Deactivated += SelectedProfileOnDeactivated;

                OnSelectedProfileChanged(profileElementEvent);
                UpdateProfilePreview();
            }
        }

        public void UpdateSelectedProfile()
        {
            lock (_selectedProfileLock)
            {
                _logger.Verbose("UpdateSelectedProfile {profile}", SelectedProfile);
                _profileService.UpdateProfile(SelectedProfile, true);

                OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile));
                UpdateProfilePreview();
            }
        }

        public void ChangeSelectedProfileElement(RenderProfileElement profileElement)
        {
            lock (_selectedProfileElementLock)
            {
                if (SelectedProfileElement == profileElement)
                    return;

                _logger.Verbose("ChangeSelectedProfileElement {profile}", profileElement);
                RenderProfileElementEventArgs profileElementEvent = new RenderProfileElementEventArgs(profileElement, SelectedProfileElement);
                SelectedProfileElement = profileElement;
                OnSelectedProfileElementChanged(profileElementEvent);

                ChangeSelectedDataBinding(null);
            }
        }

        public void UpdateSelectedProfileElement()
        {
            lock (_selectedProfileElementLock)
            {
                _logger.Verbose("UpdateSelectedProfileElement {profile}", SelectedProfileElement);
                _profileService.UpdateProfile(SelectedProfile, true);
                UpdateProfilePreview();
                OnSelectedProfileElementUpdated(new RenderProfileElementEventArgs(SelectedProfileElement));
            }
        }

        public void ChangeSelectedDataBinding(ILayerProperty layerProperty)
        {
            SelectedDataBinding = layerProperty;
            OnSelectedDataBindingChanged();
        }

        public void UpdateProfilePreview()
        {
            if (SelectedProfile == null)
                return;

            // Stick to the main segment for any element that is not currently selected
            foreach (Folder folder in SelectedProfile.GetAllFolders())
                folder.OverrideProgress(CurrentTime, folder != SelectedProfileElement);
            foreach (Layer layer in SelectedProfile.GetAllLayers())
                layer.OverrideProgress(CurrentTime, layer != SelectedProfileElement);

            OnProfilePreviewUpdated();
        }

        public bool UndoUpdateProfile()
        {
            bool undid = _profileService.UndoUpdateProfile(SelectedProfile);
            if (!undid)
                return false;

            ReloadProfile();
            return true;
        }

        public bool RedoUpdateProfile()
        {
            bool redid = _profileService.RedoUpdateProfile(SelectedProfile);
            if (!redid)
                return false;

            ReloadProfile();
            return true;
        }

        public PropertyInputRegistration RegisterPropertyInput<T>(PluginInfo pluginInfo) where T : PropertyInputViewModel
        {
            return RegisterPropertyInput(typeof(T), pluginInfo);
        }

        public PropertyInputRegistration RegisterPropertyInput(Type viewModelType, PluginInfo pluginInfo)
        {
            if (!typeof(PropertyInputViewModel).IsAssignableFrom(viewModelType))
                throw new ArtemisSharedUIException($"Property input VM type must implement {nameof(PropertyInputViewModel)}");

            lock (_registeredPropertyEditors)
            {
                Type supportedType = viewModelType.BaseType.GetGenericArguments()[0];
                // If the supported type is a generic, assume there is a base type
                if (supportedType.IsGenericParameter)
                {
                    if (supportedType.BaseType == null)
                        throw new ArtemisSharedUIException($"Generic property input VM type must have a type constraint");
                    supportedType = supportedType.BaseType;
                }

                PropertyInputRegistration existing = _registeredPropertyEditors.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.PluginInfo != pluginInfo)
                        throw new ArtemisPluginException($"Cannot register property editor for type {supportedType.Name} because an editor was already registered by {existing.PluginInfo.Name}");
                    return existing;
                }

                Kernel.Bind(viewModelType).ToSelf();
                PropertyInputRegistration registration = new PropertyInputRegistration(this, pluginInfo, supportedType, viewModelType);
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

                    Kernel.Unbind(registration.ViewModelType);
                }
            }
        }

        public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan> snapTimes = null)
        {
            if (snapToSegments)
            {
                // Snap to the end of the start segment
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.StartSegmentLength.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.StartSegmentLength;

                // Snap to the end of the main segment
                TimeSpan mainSegmentEnd = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength;
                if (Math.Abs(time.TotalMilliseconds - mainSegmentEnd.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return mainSegmentEnd;

                // Snap to the end of the end segment (end of the timeline)
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.TimelineLength.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.TimelineLength;
            }

            if (snapToCurrentTime)
            {
                // Snap to the current time
                if (Math.Abs(time.TotalMilliseconds - CurrentTime.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.StartSegmentLength;
            }

            if (snapTimes != null)
            {
                // Find the closest keyframe
                TimeSpan closeSnapTime = snapTimes.FirstOrDefault(s => Math.Abs(time.TotalMilliseconds - s.TotalMilliseconds) < tolerance.TotalMilliseconds);
                if (closeSnapTime != TimeSpan.Zero)
                    return closeSnapTime;
            }

            return time;
        }

        public PropertyInputViewModel<T> CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty)
        {
            Type viewModelType = null;
            PropertyInputRegistration registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(T));

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

            ConstructorArgument parameter = new ConstructorArgument("layerProperty", layerProperty);
            IKernel kernel = registration != null ? registration.PluginInfo.Kernel : Kernel;
            return (PropertyInputViewModel<T>) kernel.Get(viewModelType, parameter);
        }

        public ProfileModule GetCurrentModule()
        {
            return SelectedProfile?.Module;
        }

        private void ReloadProfile()
        {
            // Trigger a profile change
            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile, SelectedProfile));
            // Trigger a selected element change
            RenderProfileElement previousSelectedProfileElement = SelectedProfileElement;
            if (SelectedProfileElement is Folder folder)
                SelectedProfileElement = SelectedProfile.GetAllFolders().FirstOrDefault(f => f.EntityId == folder.EntityId);
            else if (SelectedProfileElement is Layer layer)
                SelectedProfileElement = SelectedProfile.GetAllLayers().FirstOrDefault(l => l.EntityId == layer.EntityId);
            OnSelectedProfileElementChanged(new RenderProfileElementEventArgs(SelectedProfileElement, previousSelectedProfileElement));
            // Trigger selected data binding change
            if (SelectedDataBinding != null)
            {
                SelectedDataBinding = SelectedProfileElement?.GetAllLayerProperties()?.FirstOrDefault(p => p.Path == SelectedDataBinding.Path);
                OnSelectedDataBindingChanged();
            }

            UpdateProfilePreview();
        }

        public event EventHandler<ProfileEventArgs> ProfileSelected;
        public event EventHandler<ProfileEventArgs> SelectedProfileUpdated;
        public event EventHandler<RenderProfileElementEventArgs> ProfileElementSelected;
        public event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementUpdated;
        public event EventHandler SelectedDataBindingChanged;
        public event EventHandler CurrentTimeChanged;
        public event EventHandler PixelsPerSecondChanged;
        public event EventHandler ProfilePreviewUpdated;
        public event EventHandler CurrentTimelineChanged;

        protected virtual void OnSelectedProfileChanged(ProfileEventArgs e)
        {
            ProfileSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileUpdated(ProfileEventArgs e)
        {
            SelectedProfileUpdated?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementChanged(RenderProfileElementEventArgs e)
        {
            ProfileElementSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementUpdated(RenderProfileElementEventArgs e)
        {
            SelectedProfileElementUpdated?.Invoke(this, e);
        }

        protected virtual void OnCurrentTimeChanged()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCurrentTimelineChanged()
        {
            CurrentTimelineChanged?.Invoke(this, EventArgs.Empty);
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

        private void SelectedProfileOnDeactivated(object sender, EventArgs e)
        {
            Execute.PostToUIThread(() => ChangeSelectedProfile(null));
        }
    }
}