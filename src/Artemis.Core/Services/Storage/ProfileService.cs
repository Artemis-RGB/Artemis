using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class ProfileService : IProfileService
{
    private readonly ILogger _logger;
    private readonly IRgbService _rgbService;
    private readonly IProfileCategoryRepository _profileCategoryRepository;
    private readonly IPluginManagementService _pluginManagementService;

    private readonly List<ArtemisKeyboardKeyEventArgs> _pendingKeyboardEvents = new();
    private readonly List<ProfileCategory> _profileCategories;
    private readonly IProfileRepository _profileRepository;
    private readonly List<Exception> _renderExceptions = new();
    private readonly List<Exception> _updateExceptions = new();
    private DateTime _lastRenderExceptionLog;
    private DateTime _lastUpdateExceptionLog;

    public ProfileService(ILogger logger,
        IRgbService rgbService,
        IProfileCategoryRepository profileCategoryRepository,
        IPluginManagementService pluginManagementService,
        IInputService inputService,
        IProfileRepository profileRepository)
    {
        _logger = logger;
        _rgbService = rgbService;
        _profileCategoryRepository = profileCategoryRepository;
        _pluginManagementService = pluginManagementService;
        _profileRepository = profileRepository;
        _profileCategories = new List<ProfileCategory>(_profileCategoryRepository.GetAll().Select(c => new ProfileCategory(c)).OrderBy(c => c.Order));

        _rgbService.LedsChanged += RgbServiceOnLedsChanged;
        _pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureToggled;
        _pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureToggled;

        inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;

        if (!_profileCategories.Any())
            CreateDefaultProfileCategories();
        UpdateModules();
    }

    public ProfileConfiguration? FocusProfile { get; set; }
    public ProfileElement? FocusProfileElement { get; set; }
    public bool UpdateFocusProfile { get; set; }

    /// <inheritdoc />
    public void UpdateProfiles(double deltaTime)
    {
        // If there is a focus profile update only that, and only if UpdateFocusProfile is true
        if (FocusProfile != null)
        {
            if (UpdateFocusProfile)
                FocusProfile.Profile?.Update(deltaTime);
            return;
        }

        lock (_profileCategories)
        {
            // Iterate the children in reverse because the first category must be rendered last to end up on top
            for (int i = _profileCategories.Count - 1; i > -1; i--)
            {
                ProfileCategory profileCategory = _profileCategories[i];
                for (int j = profileCategory.ProfileConfigurations.Count - 1; j > -1; j--)
                {
                    ProfileConfiguration profileConfiguration = profileCategory.ProfileConfigurations[j];

                    // Process hotkeys that where pressed since this profile last updated
                    ProcessPendingKeyEvents(profileConfiguration);

                    bool shouldBeActive = profileConfiguration.ShouldBeActive(false);
                    if (shouldBeActive)
                    {
                        profileConfiguration.Update();
                        shouldBeActive = profileConfiguration.ActivationConditionMet;
                    }

                    try
                    {
                        // Make sure the profile is active or inactive according to the parameters above
                        if (shouldBeActive && profileConfiguration.Profile == null && profileConfiguration.BrokenState != "Failed to activate profile")
                            profileConfiguration.TryOrBreak(() => ActivateProfile(profileConfiguration), "Failed to activate profile");
                        if (shouldBeActive && profileConfiguration.Profile != null && !profileConfiguration.Profile.ShouldDisplay)
                            profileConfiguration.Profile.ShouldDisplay = true;
                        else if (!shouldBeActive && profileConfiguration.Profile != null)
                        {
                            if (!profileConfiguration.FadeInAndOut)
                                DeactivateProfile(profileConfiguration);
                            else if (!profileConfiguration.Profile.ShouldDisplay && profileConfiguration.Profile.Opacity <= 0)
                                DeactivateProfile(profileConfiguration);
                            else if (profileConfiguration.Profile.Opacity > 0)
                                RequestDeactivation(profileConfiguration);
                        }

                        profileConfiguration.Profile?.Update(deltaTime);
                    }
                    catch (Exception e)
                    {
                        _updateExceptions.Add(e);
                    }
                }
            }

            LogProfileUpdateExceptions();
            _pendingKeyboardEvents.Clear();
        }
    }

    /// <inheritdoc />
    public void RenderProfiles(SKCanvas canvas)
    {
        // If there is a focus profile, render only that
        if (FocusProfile != null)
        {
            FocusProfile.Profile?.Render(canvas, SKPointI.Empty, FocusProfileElement);
            return;
        }

        lock (_profileCategories)
        {
            // Iterate the children in reverse because the first category must be rendered last to end up on top
            for (int i = _profileCategories.Count - 1; i > -1; i--)
            {
                ProfileCategory profileCategory = _profileCategories[i];
                for (int j = profileCategory.ProfileConfigurations.Count - 1; j > -1; j--)
                {
                    try
                    {
                        ProfileConfiguration profileConfiguration = profileCategory.ProfileConfigurations[j];
                        // Ensure all criteria are met before rendering
                        bool fadingOut = profileConfiguration.Profile?.ShouldDisplay == false && profileConfiguration.Profile?.Opacity > 0;
                        if (!profileConfiguration.IsSuspended && !profileConfiguration.IsMissingModule && (profileConfiguration.ActivationConditionMet || fadingOut))
                            profileConfiguration.Profile?.Render(canvas, SKPointI.Empty, null);
                    }
                    catch (Exception e)
                    {
                        _renderExceptions.Add(e);
                    }
                }
            }

            LogProfileRenderExceptions();
        }
    }

    /// <inheritdoc />
    public ReadOnlyCollection<ProfileCategory> ProfileCategories
    {
        get
        {
            lock (_profileRepository)
            {
                return _profileCategories.AsReadOnly();
            }
        }
    }

    /// <inheritdoc />
    public ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations
    {
        get
        {
            lock (_profileRepository)
            {
                return _profileCategories.SelectMany(c => c.ProfileConfigurations).ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc />
    public void LoadProfileConfigurationIcon(ProfileConfiguration profileConfiguration)
    {
        if (profileConfiguration.Icon.IconType == ProfileConfigurationIconType.MaterialIcon)
            return;

        using Stream? stream = _profileCategoryRepository.GetProfileIconStream(profileConfiguration.Entity.FileIconId);
        if (stream != null)
            profileConfiguration.Icon.SetIconByStream(stream);
    }

    /// <inheritdoc />
    public void SaveProfileConfigurationIcon(ProfileConfiguration profileConfiguration)
    {
        if (profileConfiguration.Icon.IconType == ProfileConfigurationIconType.MaterialIcon)
            return;

        using Stream? stream = profileConfiguration.Icon.GetIconStream();
        if (stream != null)
            _profileCategoryRepository.SaveProfileIconStream(profileConfiguration.Entity, stream);
    }

    /// <inheritdoc />
    public ProfileConfiguration CloneProfileConfiguration(ProfileConfiguration profileConfiguration)
    {
        return new ProfileConfiguration(profileConfiguration.Category, profileConfiguration.Entity);
    }

    /// <inheritdoc />
    public Profile ActivateProfile(ProfileConfiguration profileConfiguration)
    {
        if (profileConfiguration.Profile != null)
        {
            profileConfiguration.Profile.ShouldDisplay = true;
            return profileConfiguration.Profile;
        }

        ProfileEntity profileEntity;
        try
        {
            profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
        }
        catch (Exception e)
        {
            profileConfiguration.SetBrokenState("Failed to activate profile", e);
            throw;
        }

        if (profileEntity == null)
            throw new ArtemisCoreException($"Cannot find profile named: {profileConfiguration.Name} ID: {profileConfiguration.Entity.ProfileId}");

        Profile profile = new(profileConfiguration, profileEntity);
        profile.PopulateLeds(_rgbService.EnabledDevices);

        if (profile.IsFreshImport)
        {
            _logger.Debug("Profile is a fresh import, adapting to surface - {profile}", profile);
            AdaptProfile(profile);
        }

        profileConfiguration.Profile = profile;

        OnProfileActivated(new ProfileConfigurationEventArgs(profileConfiguration));
        return profile;
    }

    /// <inheritdoc />
    public void DeactivateProfile(ProfileConfiguration profileConfiguration)
    {
        if (FocusProfile == profileConfiguration)
            throw new ArtemisCoreException("Cannot disable a profile that is being edited, that's rude");
        if (profileConfiguration.Profile == null)
            return;

        Profile profile = profileConfiguration.Profile;
        profileConfiguration.Profile = null;
        profile.Dispose();

        OnProfileDeactivated(new ProfileConfigurationEventArgs(profileConfiguration));
    }

    /// <inheritdoc />
    public void RequestDeactivation(ProfileConfiguration profileConfiguration)
    {
        if (FocusProfile == profileConfiguration)
            throw new ArtemisCoreException("Cannot disable a profile that is being edited, that's rude");
        if (profileConfiguration.Profile == null)
            return;

        profileConfiguration.Profile.ShouldDisplay = false;
    }

    /// <inheritdoc />
    public void DeleteProfile(ProfileConfiguration profileConfiguration)
    {
        DeactivateProfile(profileConfiguration);

        ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
        if (profileEntity == null)
            return;

        profileConfiguration.Category.RemoveProfileConfiguration(profileConfiguration);
        _profileRepository.Remove(profileEntity);
        SaveProfileCategory(profileConfiguration.Category);
    }

    /// <inheritdoc />
    public ProfileCategory CreateProfileCategory(string name, bool addToTop = false)
    {
        ProfileCategory profileCategory;
        lock (_profileRepository)
        {
            if (addToTop)
            {
                profileCategory = new ProfileCategory(name, 1);
                foreach (ProfileCategory category in _profileCategories)
                {
                    category.Order++;
                    category.Save();
                    _profileCategoryRepository.Save(category.Entity);
                }
            }
            else
            {
                profileCategory = new ProfileCategory(name, _profileCategories.Count + 1);
            }

            _profileCategories.Add(profileCategory);
            SaveProfileCategory(profileCategory);
        }

        OnProfileCategoryAdded(new ProfileCategoryEventArgs(profileCategory));
        return profileCategory;
    }

    /// <inheritdoc />
    public void DeleteProfileCategory(ProfileCategory profileCategory)
    {
        List<ProfileConfiguration> profileConfigurations = profileCategory.ProfileConfigurations.ToList();
        foreach (ProfileConfiguration profileConfiguration in profileConfigurations)
            RemoveProfileConfiguration(profileConfiguration);

        lock (_profileRepository)
        {
            _profileCategories.Remove(profileCategory);
            _profileCategoryRepository.Remove(profileCategory.Entity);
        }

        OnProfileCategoryRemoved(new ProfileCategoryEventArgs(profileCategory));
    }

    /// <inheritdoc />
    public ProfileConfiguration CreateProfileConfiguration(ProfileCategory category, string name, string icon)
    {
        ProfileConfiguration configuration = new(category, name, icon);
        ProfileEntity entity = new();
        _profileRepository.Add(entity);

        configuration.Entity.ProfileId = entity.Id;
        category.AddProfileConfiguration(configuration, 0);
        return configuration;
    }

    /// <inheritdoc />
    public void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration)
    {
        profileConfiguration.Category.RemoveProfileConfiguration(profileConfiguration);

        DeactivateProfile(profileConfiguration);
        SaveProfileCategory(profileConfiguration.Category);
        ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
        if (profileEntity != null)
            _profileRepository.Remove(profileEntity);

        profileConfiguration.Dispose();
    }

    /// <inheritdoc />
    public void SaveProfileCategory(ProfileCategory profileCategory)
    {
        profileCategory.Save();
        _profileCategoryRepository.Save(profileCategory.Entity);

        lock (_profileCategories)
        {
            _profileCategories.Sort((a, b) => a.Order - b.Order);
        }
    }

    /// <inheritdoc />
    public void SaveProfile(Profile profile, bool includeChildren)
    {
        _logger.Debug("Updating profile - Saving {Profile}", profile);
        profile.Save();
        if (includeChildren)
        {
            foreach (RenderProfileElement child in profile.GetAllRenderElements())
                child.Save();
        }

        // At this point the user made actual changes, save that
        profile.IsFreshImport = false;
        profile.ProfileEntity.IsFreshImport = false;

        _profileRepository.Save(profile.ProfileEntity);

        // If the provided profile is external (cloned or from the workshop?) but it is loaded locally too, reload the local instance
        // A bit dodge but it ensures local instances always represent the latest stored version
        ProfileConfiguration? localInstance = ProfileConfigurations.FirstOrDefault(p => p.Profile != null && p.Profile != profile && p.ProfileId == profile.ProfileEntity.Id);
        if (localInstance == null)
            return;
        DeactivateProfile(localInstance);
        ActivateProfile(localInstance);
    }

    /// <inheritdoc />
    public ProfileConfigurationExportModel ExportProfile(ProfileConfiguration profileConfiguration)
    {
        // The profile may not be active and in that case lets activate it real quick
        Profile profile = profileConfiguration.Profile ?? ActivateProfile(profileConfiguration);

        return new ProfileConfigurationExportModel
        {
            ProfileConfigurationEntity = profileConfiguration.Entity,
            ProfileEntity = profile.ProfileEntity,
            ProfileImage = profileConfiguration.Icon.GetIconStream()
        };
    }

    /// <inheritdoc />
    public ProfileConfiguration ImportProfile(ProfileCategory category, ProfileConfigurationExportModel exportModel,
        bool makeUnique, bool markAsFreshImport, string? nameAffix)
    {
        if (exportModel.ProfileEntity == null)
            throw new ArtemisCoreException("Cannot import a profile without any data");

        // Create a copy of the entity because we'll be using it from now on
        ProfileEntity profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(
            JsonConvert.SerializeObject(exportModel.ProfileEntity, IProfileService.ExportSettings),
            IProfileService.ExportSettings
        )!;

        // Assign a new GUID to make sure it is unique in case of a previous import of the same content
        if (makeUnique)
            profileEntity.UpdateGuid(Guid.NewGuid());

        if (nameAffix != null)
            profileEntity.Name = $"{profileEntity.Name} - {nameAffix}";
        if (markAsFreshImport)
            profileEntity.IsFreshImport = true;

        if (_profileRepository.Get(profileEntity.Id) == null)
            _profileRepository.Add(profileEntity);
        else
            throw new ArtemisCoreException($"Cannot import this profile without {nameof(makeUnique)} being true");

        ProfileConfiguration profileConfiguration;
        if (exportModel.ProfileConfigurationEntity != null)
        {
            ProfileConfigurationEntity profileConfigurationEntity = JsonConvert.DeserializeObject<ProfileConfigurationEntity>(
                JsonConvert.SerializeObject(exportModel.ProfileConfigurationEntity, IProfileService.ExportSettings), IProfileService.ExportSettings
            )!;
            // A new GUID will be given on save
            profileConfigurationEntity.FileIconId = Guid.Empty;
            profileConfiguration = new ProfileConfiguration(category, profileConfigurationEntity);
            if (nameAffix != null)
                profileConfiguration.Name = $"{profileConfiguration.Name} - {nameAffix}";
        }
        else
        {
            profileConfiguration = new ProfileConfiguration(category, profileEntity.Name, "Import");
        }

        if (exportModel.ProfileImage != null)
        {
            profileConfiguration.Icon.SetIconByStream(exportModel.ProfileImage);
            SaveProfileConfigurationIcon(profileConfiguration);
        }

        profileConfiguration.Entity.ProfileId = profileEntity.Id;
        category.AddProfileConfiguration(profileConfiguration, 0);

        List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>();
        profileConfiguration.LoadModules(modules);
        SaveProfileCategory(category);

        return profileConfiguration;
    }

    /// <inheritdoc />
    public void AdaptProfile(Profile profile)
    {
        List<ArtemisDevice> devices = _rgbService.EnabledDevices.ToList();
        foreach (Layer layer in profile.GetAllLayers())
            layer.Adapter.Adapt(devices);

        profile.Save();

        foreach (RenderProfileElement renderProfileElement in profile.GetAllRenderElements())
            renderProfileElement.Save();

        _logger.Debug("Adapt profile - Saving " + profile);
        _profileRepository.Save(profile.ProfileEntity);
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        lock (_profileCategories)
        {
            _pendingKeyboardEvents.Add(e);
        }
    }

    /// <summary>
    ///     Populates all missing LEDs on all currently active profiles
    /// </summary>
    private void ActiveProfilesPopulateLeds()
    {
        foreach (ProfileConfiguration profileConfiguration in ProfileConfigurations)
        {
            if (profileConfiguration.Profile == null) continue;
            profileConfiguration.Profile.PopulateLeds(_rgbService.EnabledDevices);

            if (!profileConfiguration.Profile.IsFreshImport) continue;
            _logger.Debug("Profile is a fresh import, adapting to surface - {profile}", profileConfiguration.Profile);
            AdaptProfile(profileConfiguration.Profile);
        }
    }

    private void UpdateModules()
    {
        lock (_profileRepository)
        {
            List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>();
            foreach (ProfileCategory profileCategory in _profileCategories)
            {
                foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                    profileConfiguration.LoadModules(modules);
            }
        }
    }

    private void RgbServiceOnLedsChanged(object? sender, EventArgs e)
    {
        ActiveProfilesPopulateLeds();
    }

    private void PluginManagementServiceOnPluginFeatureToggled(object? sender, PluginFeatureEventArgs e)
    {
        if (e.PluginFeature is Module)
            UpdateModules();
    }

    private void ProcessPendingKeyEvents(ProfileConfiguration profileConfiguration)
    {
        if (profileConfiguration.HotkeyMode == ProfileConfigurationHotkeyMode.None)
            return;

        bool before = profileConfiguration.IsSuspended;
        foreach (ArtemisKeyboardKeyEventArgs e in _pendingKeyboardEvents)
        {
            if (profileConfiguration.HotkeyMode == ProfileConfigurationHotkeyMode.Toggle)
            {
                if (profileConfiguration.EnableHotkey != null && profileConfiguration.EnableHotkey.MatchesEventArgs(e))
                    profileConfiguration.IsSuspended = !profileConfiguration.IsSuspended;
            }
            else
            {
                if (profileConfiguration.IsSuspended && profileConfiguration.EnableHotkey != null && profileConfiguration.EnableHotkey.MatchesEventArgs(e))
                    profileConfiguration.IsSuspended = false;
                else if (!profileConfiguration.IsSuspended && profileConfiguration.DisableHotkey != null && profileConfiguration.DisableHotkey.MatchesEventArgs(e))
                    profileConfiguration.IsSuspended = true;
            }
        }

        // If suspension was changed, save the category
        if (before != profileConfiguration.IsSuspended)
            SaveProfileCategory(profileConfiguration.Category);
    }

    private void CreateDefaultProfileCategories()
    {
        foreach (DefaultCategoryName defaultCategoryName in Enum.GetValues<DefaultCategoryName>())
            CreateProfileCategory(defaultCategoryName.ToString());
    }

    private void LogProfileUpdateExceptions()
    {
        // Only log update exceptions every 10 seconds to avoid spamming the logs
        if (DateTime.Now - _lastUpdateExceptionLog < TimeSpan.FromSeconds(10))
            return;
        _lastUpdateExceptionLog = DateTime.Now;

        if (!_updateExceptions.Any())
            return;

        // Group by stack trace, that should gather up duplicate exceptions
        foreach (IGrouping<string?, Exception> exceptions in _updateExceptions.GroupBy(e => e.StackTrace))
        {
            _logger.Warning(exceptions.First(),
                "Exception was thrown {count} times during profile update in the last 10 seconds",
                exceptions.Count());
        }

        // When logging is finished start with a fresh slate
        _updateExceptions.Clear();
    }

    private void LogProfileRenderExceptions()
    {
        // Only log update exceptions every 10 seconds to avoid spamming the logs
        if (DateTime.Now - _lastRenderExceptionLog < TimeSpan.FromSeconds(10))
            return;
        _lastRenderExceptionLog = DateTime.Now;

        if (!_renderExceptions.Any())
            return;

        // Group by stack trace, that should gather up duplicate exceptions
        foreach (IGrouping<string?, Exception> exceptions in _renderExceptions.GroupBy(e => e.StackTrace))
        {
            _logger.Warning(exceptions.First(),
                "Exception was thrown {count} times during profile render in the last 10 seconds",
                exceptions.Count());
        }

        // When logging is finished start with a fresh slate
        _renderExceptions.Clear();
    }

    #region Events

    public event EventHandler<ProfileConfigurationEventArgs>? ProfileActivated;
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileDeactivated;
    public event EventHandler<ProfileCategoryEventArgs>? ProfileCategoryAdded;
    public event EventHandler<ProfileCategoryEventArgs>? ProfileCategoryRemoved;

    protected virtual void OnProfileActivated(ProfileConfigurationEventArgs e)
    {
        ProfileActivated?.Invoke(this, e);
    }

    protected virtual void OnProfileDeactivated(ProfileConfigurationEventArgs e)
    {
        ProfileDeactivated?.Invoke(this, e);
    }

    protected virtual void OnProfileCategoryAdded(ProfileCategoryEventArgs e)
    {
        ProfileCategoryAdded?.Invoke(this, e);
    }

    protected virtual void OnProfileCategoryRemoved(ProfileCategoryEventArgs e)
    {
        ProfileCategoryRemoved?.Invoke(this, e);
    }

    #endregion
}