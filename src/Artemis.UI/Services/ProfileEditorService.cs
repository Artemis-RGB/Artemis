using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using Artemis.UI.Services.Interfaces;
using Ninject;
using Ninject.Parameters;
using SkiaSharp;

namespace Artemis.UI.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly ICoreService _coreService;
        private readonly IProfileService _profileService;
        private readonly IKernel _kernel;
        private TimeSpan _currentTime;
        private TimeSpan _lastUpdateTime;
        private int _pixelsPerSecond;

        public ProfileEditorService(ICoreService coreService, IProfileService profileService, IKernel kernel)
        {
            _coreService = coreService;
            _profileService = profileService;
            _kernel = kernel;

            RegisteredPropertyEditors = new Dictionary<Type, Type>
            {
                {typeof(LayerBrushReference), typeof(BrushPropertyInputViewModel)},
                {typeof(ColorGradient), typeof(ColorGradientPropertyInputViewModel)},
                {typeof(float), typeof(FloatPropertyInputViewModel)},
                {typeof(int), typeof(IntPropertyInputViewModel)},
                {typeof(SKColor), typeof(SKColorPropertyInputViewModel)},
                {typeof(SKPoint), typeof(SKPointPropertyInputViewModel)},
                {typeof(SKSize), typeof(SKSizePropertyInputViewModel)}
            };
            PixelsPerSecond = 31;
        }

        public Dictionary<Type, Type> RegisteredPropertyEditors { get; set; }

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


        public LayerPropertyBaseViewModel CreateLayerPropertyViewModel(BaseLayerProperty baseLayerProperty, PropertyDescriptionAttribute propertyDescription)
        {
            // Go through the pain of instantiating a generic type VM now via reflection to make things a lot simpler down the line
            var genericType = baseLayerProperty.GetType().BaseType.GetGenericArguments()[0];
            // Only create entries for types supported by a tree input VM
            if (!genericType.IsEnum && !RegisteredPropertyEditors.ContainsKey(genericType))
                return null;
            var genericViewModel = typeof(LayerPropertyViewModel<>).MakeGenericType(genericType);
            var parameters = new IParameter[]
            {
                new ConstructorArgument("layerProperty", baseLayerProperty),
                new ConstructorArgument("propertyDescription", propertyDescription)
            };

            return (LayerPropertyBaseViewModel) _kernel.Get(genericViewModel, parameters);
        }

        public TreePropertyViewModel<T> CreateTreePropertyViewModel<T>(LayerPropertyViewModel<T> layerPropertyViewModel)
        {
            var type = typeof(T).IsEnum
                ? typeof(EnumPropertyInputViewModel<>).MakeGenericType(typeof(T))
                : RegisteredPropertyEditors[typeof(T)];

            var parameters = new IParameter[]
            {
                new ConstructorArgument("layerPropertyViewModel", layerPropertyViewModel)
            };
            return new TreePropertyViewModel<T>(layerPropertyViewModel, (PropertyInputViewModel<T>) _kernel.Get(type, parameters), this);
        }

        public void ChangeSelectedProfile(Profile profile)
        {
            ChangeSelectedProfileElement(null);

            var profileElementEvent = new ProfileElementEventArgs(profile, SelectedProfile);
            SelectedProfile = profile;
            UpdateProfilePreview();
            OnSelectedProfileChanged(profileElementEvent);
        }

        public void UpdateSelectedProfile()
        {
            _profileService.UpdateProfile(SelectedProfile, false);
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
            _profileService.UndoUpdateProfile(SelectedProfile, module);
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
            _profileService.RedoUpdateProfile(SelectedProfile, module);
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