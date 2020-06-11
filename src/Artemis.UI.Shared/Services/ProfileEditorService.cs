using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject;

namespace Artemis.UI.Shared.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly ICoreService _coreService;
        private readonly IProfileService _profileService;
        private readonly List<PropertyInputRegistration> _registeredPropertyEditors;
        private TimeSpan _currentTime;
        private TimeSpan _lastUpdateTime;
        private int _pixelsPerSecond;

        public ProfileEditorService(ICoreService coreService, IProfileService profileService, IKernel kernel)
        {
            _coreService = coreService;
            _profileService = profileService;
            _registeredPropertyEditors = new List<PropertyInputRegistration>();

            Kernel = kernel;
            PixelsPerSecond = 31;
        }

        public IKernel Kernel { get; }
        public IReadOnlyList<PropertyInputRegistration> RegisteredPropertyEditors => _registeredPropertyEditors.AsReadOnly();
        public Profile SelectedProfile { get; private set; }
        public ProfileElement SelectedProfileElement { get; private set; }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime.Equals(value))
                    return;
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
            ChangeSelectedProfileElement(null);

            var profileElementEvent = new ProfileElementEventArgs(profile, SelectedProfile);
            SelectedProfile = profile;
            UpdateProfilePreview();
            OnSelectedProfileChanged(profileElementEvent);
        }

        public void UpdateSelectedProfile(bool includeChildren)
        {
            _profileService.UpdateProfile(SelectedProfile, includeChildren);
            UpdateProfilePreview();
            OnSelectedProfileElementUpdated(new ProfileElementEventArgs(SelectedProfile));
        }

        public void ChangeSelectedProfileElement(ProfileElement profileElement)
        {
            var profileElementEvent = new ProfileElementEventArgs(profileElement, SelectedProfileElement);
            SelectedProfileElement = profileElement;
            OnSelectedProfileElementChanged(profileElementEvent);
        }

        public void UpdateSelectedProfileElement()
        {
            _profileService.UpdateProfile(SelectedProfile, true);
            UpdateProfilePreview();
            OnSelectedProfileElementUpdated(new ProfileElementEventArgs(SelectedProfileElement));
        }

        public void UpdateProfilePreview()
        {
            if (SelectedProfile == null)
                return;

            var delta = CurrentTime - _lastUpdateTime;
            foreach (var layer in SelectedProfile.GetAllLayers())
            {
                layer.OverrideProgress(CurrentTime);
                layer.LayerBrush?.Update(delta.TotalSeconds);
            }

            _lastUpdateTime = CurrentTime;
            OnProfilePreviewUpdated();
        }

        public void UndoUpdateProfile(ProfileModule module)
        {
            var undid = _profileService.UndoUpdateProfile(SelectedProfile, module);
            if (!undid)
                return;

            OnSelectedProfileChanged(new ProfileElementEventArgs(SelectedProfile, SelectedProfile));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<ProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
        }

        public void RedoUpdateProfile(ProfileModule module)
        {
            var redid = _profileService.RedoUpdateProfile(SelectedProfile, module);
            if (!redid)
                return;

            OnSelectedProfileChanged(new ProfileElementEventArgs(SelectedProfile, SelectedProfile));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<ProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
        }

        public PropertyInputRegistration RegisterPropertyInput(PluginInfo pluginInfo, Type viewModelType)
        {
            // Bit ugly to do a name comparison but I don't know a nicer way right now
            if (viewModelType.BaseType == null || viewModelType.BaseType.Name != typeof(PropertyInputViewModel<>).Name)
                throw new ArtemisPluginException($"{nameof(viewModelType)} base type must be of type PropertyInputViewModel<T>");

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

        public event EventHandler<ProfileElementEventArgs> ProfileSelected;
        public event EventHandler<ProfileElementEventArgs> SelectedProfileUpdated;
        public event EventHandler<ProfileElementEventArgs> ProfileElementSelected;
        public event EventHandler<ProfileElementEventArgs> SelectedProfileElementUpdated;
        public event EventHandler CurrentTimeChanged;
        public event EventHandler PixelsPerSecondChanged;
        public event EventHandler ProfilePreviewUpdated;

        public void StopRegularRender()
        {
            _coreService.ModuleUpdatingDisabled = true;
        }

        public void ResumeRegularRender()
        {
            _coreService.ModuleUpdatingDisabled = false;
        }

        protected virtual void OnSelectedProfileChanged(ProfileElementEventArgs e)
        {
            ProfileSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileUpdated(ProfileElementEventArgs e)
        {
            SelectedProfileUpdated?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementChanged(ProfileElementEventArgs e)
        {
            ProfileElementSelected?.Invoke(this, e);
        }

        protected virtual void OnSelectedProfileElementUpdated(ProfileElementEventArgs e)
        {
            SelectedProfileElementUpdated?.Invoke(this, e);
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
    }
}