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
using Serilog;

namespace Artemis.UI.Shared.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly ICoreService _coreService;
        private readonly IProfileService _profileService;
        private readonly ILogger _logger;
        private readonly List<PropertyInputRegistration> _registeredPropertyEditors;
        private TimeSpan _currentTime;
        private TimeSpan _lastUpdateTime;
        private int _pixelsPerSecond;

        public ProfileEditorService(ICoreService coreService, IProfileService profileService, IKernel kernel, ILogger logger)
        {
            _coreService = coreService;
            _profileService = profileService;
            _logger = logger;
            _registeredPropertyEditors = new List<PropertyInputRegistration>();

            Kernel = kernel;
            PixelsPerSecond = 31;
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
            if (SelectedProfile == profile)
                return;

            _logger.Verbose("ChangeSelectedProfile {profile}", profile);
            ChangeSelectedProfileElement(null);

            var profileElementEvent = new ProfileEventArgs(profile, SelectedProfile);
            SelectedProfile = profile;
            UpdateProfilePreview();
            OnSelectedProfileChanged(profileElementEvent);
        }

        public void UpdateSelectedProfile()
        {
            _logger.Verbose("UpdateSelectedProfile {profile}", SelectedProfile);
            _profileService.UpdateProfile(SelectedProfile, true);
            UpdateProfilePreview();
            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile));
        }

        public void ChangeSelectedProfileElement(RenderProfileElement profileElement)
        {
            if (SelectedProfileElement == profileElement)
                return;

            _logger.Verbose("ChangeSelectedProfileElement {profile}", profileElement);
            var profileElementEvent = new RenderProfileElementEventArgs(profileElement, SelectedProfileElement);
            SelectedProfileElement = profileElement;
            OnSelectedProfileElementChanged(profileElementEvent);
        }

        public void UpdateSelectedProfileElement()
        {
            _logger.Verbose("UpdateSelectedProfileElement {profile}", SelectedProfileElement);
            _profileService.UpdateProfile(SelectedProfile, true);
            UpdateProfilePreview();
            OnSelectedProfileElementUpdated(new RenderProfileElementEventArgs(SelectedProfileElement));
        }

        public void UpdateProfilePreview()
        {
            if (SelectedProfile == null)
                return;

            var delta = CurrentTime - _lastUpdateTime;
            foreach (var folder in SelectedProfile.GetAllFolders())
            {
                foreach (var baseLayerEffect in folder.LayerEffects)
                    baseLayerEffect.Update(delta.TotalSeconds);
            }

            foreach (var layer in SelectedProfile.GetAllLayers())
            {
                layer.OverrideProgress(CurrentTime);
                layer.LayerBrush?.Update(delta.TotalSeconds);
                foreach (var baseLayerEffect in layer.LayerEffects)
                    baseLayerEffect.Update(delta.TotalSeconds);
            }

            _lastUpdateTime = CurrentTime;
            OnProfilePreviewUpdated();
        }

        public void UndoUpdateProfile(ProfileModule module)
        {
            var undid = _profileService.UndoUpdateProfile(SelectedProfile, module);
            if (!undid)
                return;

            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile, SelectedProfile));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<RenderProfileElement>().ToList();
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

            OnSelectedProfileChanged(new ProfileEventArgs(SelectedProfile, SelectedProfile));

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<RenderProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
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

        public Module GetCurrentModule()
        {
            return (Module) SelectedProfile?.PluginInfo.Instance;
        }

        public event EventHandler<ProfileEventArgs> ProfileSelected;
        public event EventHandler<ProfileEventArgs> SelectedProfileUpdated;
        public event EventHandler<RenderProfileElementEventArgs> ProfileElementSelected;
        public event EventHandler<RenderProfileElementEventArgs> SelectedProfileElementUpdated;
        public event EventHandler CurrentTimeChanged;
        public event EventHandler PixelsPerSecondChanged;
        public event EventHandler ProfilePreviewUpdated;

        public void StopRegularRender()
        {
            _coreService.PluginUpdatingDisabled = true;
        }

        public void ResumeRegularRender()
        {
            _coreService.PluginUpdatingDisabled = false;
        }

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