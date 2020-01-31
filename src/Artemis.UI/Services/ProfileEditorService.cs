using System;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly ICoreService _coreService;
        private readonly IProfileService _profileService;
        private TimeSpan _currentTime;
        private TimeSpan _lastUpdateTime;

        public ProfileEditorService(ICoreService coreService, IProfileService profileService)
        {
            _coreService = coreService;
            _profileService = profileService;
        }

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

        public void ChangeSelectedProfile(Profile profile)
        {
            SelectedProfile = profile;
            UpdateProfilePreview();
            OnSelectedProfileChanged();
        }

        public void UpdateSelectedProfile()
        {
            _profileService.UpdateProfile(SelectedProfile, false);
            UpdateProfilePreview();
            OnSelectedProfileElementUpdated();
        }

        public void ChangeSelectedProfileElement(ProfileElement profileElement)
        {
            SelectedProfileElement = profileElement;
            OnSelectedProfileElementChanged();
        }

        public void UpdateSelectedProfileElement()
        {
            _profileService.UpdateProfile(SelectedProfile, true);
            UpdateProfilePreview();
            OnSelectedProfileElementUpdated();
        }


        public void UpdateProfilePreview()
        {
            if (SelectedProfile == null)
                return;
            var delta = CurrentTime - _lastUpdateTime;
            foreach (var layer in SelectedProfile.GetAllLayers())
            {
                // Override keyframe progress
                foreach (var baseLayerProperty in layer.Properties)
                    baseLayerProperty.KeyframeEngine?.OverrideProgress(CurrentTime);

                // Force layer shape to redraw
                layer.LayerShape?.CalculateRenderProperties();
                // Manually update the layer's engine and brush
                foreach (var property in layer.Properties)
                    property.KeyframeEngine?.Update(delta.TotalSeconds);
                layer.LayerBrush?.Update(delta.TotalSeconds);
            }

            _lastUpdateTime = CurrentTime;
            OnProfilePreviewUpdated();
        }

        public void UndoUpdateProfile(ProfileModule module)
        {
            _profileService.UndoUpdateProfile(SelectedProfile, module);
            OnSelectedProfileChanged();

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
            _profileService.RedoUpdateProfile(SelectedProfile, module);
            OnSelectedProfileChanged();

            if (SelectedProfileElement != null)
            {
                var elements = SelectedProfile.GetAllLayers().Cast<ProfileElement>().ToList();
                elements.AddRange(SelectedProfile.GetAllFolders());
                var element = elements.FirstOrDefault(l => l.EntityId == SelectedProfileElement.EntityId);
                ChangeSelectedProfileElement(element);
            }

            UpdateProfilePreview();
        }

        public event EventHandler SelectedProfileChanged;
        public event EventHandler SelectedProfileUpdated;
        public event EventHandler SelectedProfileElementChanged;
        public event EventHandler SelectedProfileElementUpdated;
        public event EventHandler CurrentTimeChanged;
        public event EventHandler ProfilePreviewUpdated;

        public void StopRegularRender()
        {
            _coreService.ModuleUpdatingDisabled = true;
        }

        public void ResumeRegularRender()
        {
            _coreService.ModuleUpdatingDisabled = false;
        }

        protected virtual void OnSelectedProfileElementUpdated()
        {
            SelectedProfileElementUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileElementChanged()
        {
            SelectedProfileElementChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileUpdated()
        {
            SelectedProfileUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedProfileChanged()
        {
            SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCurrentTimeChanged()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnProfilePreviewUpdated()
        {
            ProfilePreviewUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}