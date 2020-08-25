using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Exceptions;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject;
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

                var profileElementEvent = new ProfileEventArgs(profile, SelectedProfile);

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
                var profileElementEvent = new RenderProfileElementEventArgs(profileElement, SelectedProfileElement);
                SelectedProfileElement = profileElement;
                OnSelectedProfileElementChanged(profileElementEvent);
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

        public void UpdateProfilePreview()
        {
            if (SelectedProfile == null)
                return;

            // Stick to the main segment for any element that is not currently selected
            foreach (var folder in SelectedProfile.GetAllFolders())
                folder.OverrideProgress(CurrentTime, folder != SelectedProfileElement);
            foreach (var layer in SelectedProfile.GetAllLayers())
                layer.OverrideProgress(CurrentTime, layer != SelectedProfileElement);

            OnProfilePreviewUpdated();
        }

        public bool UndoUpdateProfile()
        {
            var undid = _profileService.UndoUpdateProfile(SelectedProfile);
            if (!undid)
                return false;

            if (SelectedProfileElement is Folder folder)
                SelectedProfileElement = SelectedProfile.GetAllFolders().FirstOrDefault(f => f.EntityId == folder.EntityId);
            else if (SelectedProfileElement is Layer layer)
                SelectedProfileElement = SelectedProfile.GetAllLayers().FirstOrDefault(l => l.EntityId == layer.EntityId);

            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile, SelectedProfile));
            OnSelectedProfileElementChanged(new RenderProfileElementEventArgs(SelectedProfileElement));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<RenderProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
            return true;
        }

        public bool RedoUpdateProfile()
        {
            var redid = _profileService.RedoUpdateProfile(SelectedProfile);
            if (!redid)
                return false;

            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile, SelectedProfile));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<RenderProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
            return true;
        }

        public PropertyInputRegistration RegisterPropertyInput<T>(PluginInfo pluginInfo) where T : PropertyInputViewModel
        {
            var viewModelType = typeof(T);
            lock (_registeredPropertyEditors)
            {
                var supportedType = viewModelType.BaseType.GetGenericArguments()[0];
                var existing = _registeredPropertyEditors.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.PluginInfo != pluginInfo)
                        throw new ArtemisPluginException($"Cannot register property editor for type {supportedType.Name} because an editor was already registered by {pluginInfo.Name}");
                    return existing;
                }

                Kernel.Bind(viewModelType).ToSelf();
                var registration = new PropertyInputRegistration(this, pluginInfo, supportedType, viewModelType);
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

        public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, bool snapToKeyframes, BaseLayerPropertyKeyframe excludedKeyframe = null)
        {
            if (snapToSegments)
            {
                // Snap to the end of the start segment
                if (Math.Abs(time.TotalMilliseconds - SelectedProfileElement.StartSegmentLength.TotalMilliseconds) < tolerance.TotalMilliseconds)
                    return SelectedProfileElement.StartSegmentLength;

                // Snap to the end of the main segment
                var mainSegmentEnd = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength;
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

            if (snapToKeyframes)
            {
                // Get all visible keyframes
                var keyframes = SelectedProfileElement.GetAllKeyframes()
                    .Where(k => k != excludedKeyframe && SelectedProfileElement.IsPropertyGroupExpanded(k.BaseLayerProperty.Parent))
                    .ToList();

                // Find the closest keyframe
                var closeKeyframe = keyframes.FirstOrDefault(k => Math.Abs(time.TotalMilliseconds - k.Position.TotalMilliseconds) < tolerance.TotalMilliseconds);
                if (closeKeyframe != null)
                    return closeKeyframe.Position;
            }

            return time;
        }

        public ProfileModule GetCurrentModule()
        {
            return SelectedProfile?.Module;
        }

        public event EventHandler<ProfileEventArgs> ProfileSelected;
        public event EventHandler<ProfileEventArgs> SelectedProfileUpdated;
        public event EventHandler<RenderProfileElementEventArgs> ProfileElementSelected;
        public event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementUpdated;
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

        private void SelectedProfileOnDeactivated(object sender, EventArgs e)
        {
            Execute.PostToUIThread(() => ChangeSelectedProfile(null));
        }
    }
}