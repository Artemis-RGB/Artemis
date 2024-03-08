using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations;
using Artemis.Storage.Repositories.Interfaces;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class ProfileService : IProfileService
{
    private readonly ILogger _logger;
    private readonly IProfileCategoryRepository _profileCategoryRepository;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IDeviceService _deviceService;
    private readonly List<ArtemisKeyboardKeyEventArgs> _pendingKeyboardEvents = new();
    private readonly List<ProfileCategory> _profileCategories;
    private readonly List<IProfileMigration> _profileMigrators;
    private readonly List<Exception> _renderExceptions = new();
    private readonly List<Exception> _updateExceptions = new();

    private DateTime _lastRenderExceptionLog;
    private DateTime _lastUpdateExceptionLog;

    public ProfileService(ILogger logger,
        IProfileCategoryRepository profileCategoryRepository,
        IPluginManagementService pluginManagementService,
        IInputService inputService,
        IDeviceService deviceService,
        List<IProfileMigration> profileMigrators)
    {
        _logger = logger;
        _profileCategoryRepository = profileCategoryRepository;
        _pluginManagementService = pluginManagementService;
        _deviceService = deviceService;
        _profileMigrators = profileMigrators;
        _profileCategories = new List<ProfileCategory>(_profileCategoryRepository.GetAll().Select(c => new ProfileCategory(c)).OrderBy(c => c.Order));

        ProfileCategories = new ReadOnlyCollection<ProfileCategory>(_profileCategories);
        
        _deviceService.LedsChanged += DeviceServiceOnLedsChanged;
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

    public bool ProfileRenderingDisabled { get; set; }

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
    public ReadOnlyCollection<ProfileCategory> ProfileCategories { get; }

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

        Profile profile = new(profileConfiguration, profileConfiguration.Entity.Profile);
        profile.PopulateLeds(_deviceService.EnabledDevices);

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

    private void RequestDeactivation(ProfileConfiguration profileConfiguration)
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

        ProfileCategory category = profileConfiguration.Category;

        category.RemoveProfileConfiguration(profileConfiguration);
        category.Entity.ProfileConfigurations.Remove(profileConfiguration.Entity);

        _profileCategoryRepository.SaveChanges();
    }

    /// <inheritdoc />
    public ProfileCategory CreateProfileCategory(string name, bool addToTop = false)
    {
        ProfileCategory profileCategory;
        if (addToTop)
        {
            profileCategory = new ProfileCategory(name, 1);
            foreach (ProfileCategory category in _profileCategories)
            {
                category.Order++;
                category.Save();
            }
        }
        else
        {
            profileCategory = new ProfileCategory(name, _profileCategories.Count + 1);
        }

        _profileCategoryRepository.Add(profileCategory.Entity);
        _profileCategories.Add(profileCategory);

        OnProfileCategoryAdded(new ProfileCategoryEventArgs(profileCategory));
        return profileCategory;
    }

    /// <inheritdoc />
    public void DeleteProfileCategory(ProfileCategory profileCategory)
    {
        foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations.ToList())
            RemoveProfileConfiguration(profileConfiguration);

        _profileCategories.Remove(profileCategory);
        _profileCategoryRepository.Remove(profileCategory.Entity);

        OnProfileCategoryRemoved(new ProfileCategoryEventArgs(profileCategory));
    }

    /// <inheritdoc />
    public ProfileConfiguration CreateProfileConfiguration(ProfileCategory category, string name, string icon)
    {
        ProfileConfiguration configuration = new(category, name, icon);

        category.AddProfileConfiguration(configuration, 0);
        SaveProfileCategory(category);
        return configuration;
    }

    /// <inheritdoc />
    public void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration)
    {
        ProfileCategory category = profileConfiguration.Category;
        category.RemoveProfileConfiguration(profileConfiguration);

        DeactivateProfile(profileConfiguration);
        SaveProfileCategory(profileConfiguration.Category);

        profileConfiguration.Dispose();
    }

    /// <inheritdoc />
    public void SaveProfileCategory(ProfileCategory profileCategory)
    {
        profileCategory.Save();
        _profileCategoryRepository.SaveChanges();

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

        SaveProfileCategory(profile.Configuration.Category);

        // If the provided profile is external (cloned or from the workshop?) but it is loaded locally too, reload the local instance
        // A bit dodge but it ensures local instances always represent the latest stored version
        ProfileConfiguration? localInstance = ProfileCategories
            .SelectMany(c => c.ProfileConfigurations)
            .FirstOrDefault(p => p.Profile != null && p.Profile != profile && p.ProfileId == profile.ProfileEntity.Id);
        if (localInstance == null)
            return;
        DeactivateProfile(localInstance);
        ActivateProfile(localInstance);
    }

    /// <inheritdoc />
    public async Task<Stream> ExportProfile(ProfileConfiguration profileConfiguration)
    {
        string configurationJson = CoreJson.Serialize(profileConfiguration.Entity.ProfileConfiguration);
        string profileJson = CoreJson.Serialize(profileConfiguration.Entity.Profile);

        MemoryStream archiveStream = new();

        // Create a ZIP archive
        using (ZipArchive archive = new(archiveStream, ZipArchiveMode.Create, true))
        {
            ZipArchiveEntry configurationEntry = archive.CreateEntry("configuration.json");
            await using (Stream entryStream = configurationEntry.Open())
            {
                await entryStream.WriteAsync(Encoding.Default.GetBytes(configurationJson));
            }

            ZipArchiveEntry profileEntry = archive.CreateEntry("profile.json");
            await using (Stream entryStream = profileEntry.Open())
            {
                await entryStream.WriteAsync(Encoding.Default.GetBytes(profileJson));
            }

            if (profileConfiguration.Icon.IconBytes != null)
            {
                ZipArchiveEntry iconEntry = archive.CreateEntry("icon.png");
                await using Stream entryStream = iconEntry.Open();
                await entryStream.WriteAsync(profileConfiguration.Icon.IconBytes);
            }
        }

        archiveStream.Seek(0, SeekOrigin.Begin);
        return archiveStream;
    }

    /// <inheritdoc />
    public async Task<ProfileConfiguration> ImportProfile(Stream archiveStream, ProfileCategory category, bool makeUnique, bool markAsFreshImport, string? nameAffix, int targetIndex = 0)
    {
        using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read, true);

        // There should be a configuration.json and profile.json
        ZipArchiveEntry? configurationEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith("configuration.json"));
        ZipArchiveEntry? profileEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith("profile.json"));
        ZipArchiveEntry? iconEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith("icon.png"));

        if (configurationEntry == null)
            throw new ArtemisCoreException("Could not import profile, configuration.json missing");
        if (profileEntry == null)
            throw new ArtemisCoreException("Could not import profile, profile.json missing");

        // Deserialize profile configuration to JObject
        await using Stream configurationStream = configurationEntry.Open();
        using StreamReader configurationReader = new(configurationStream);
        JsonObject? configurationJson = CoreJson.Deserialize<JsonObject>(await configurationReader.ReadToEndAsync());
        // Deserialize profile to JObject
        await using Stream profileStream = profileEntry.Open();
        using StreamReader profileReader = new(profileStream);
        JsonObject? profileJson = CoreJson.Deserialize<JsonObject>(await profileReader.ReadToEndAsync());

        // Before deserializing, apply any pending migrations
        MigrateProfile(configurationJson, profileJson);

        // Deserialize profile configuration to ProfileConfigurationEntity
        ProfileConfigurationEntity? configurationEntity = configurationJson?.Deserialize<ProfileConfigurationEntity>(Constants.JsonConvertSettings);
        if (configurationEntity == null)
            throw new ArtemisCoreException("Could not import profile, failed to deserialize configuration.json");
        // Deserialize profile to ProfileEntity
        ProfileEntity? profileEntity = profileJson?.Deserialize<ProfileEntity>(Constants.JsonConvertSettings);
        if (profileEntity == null)
            throw new ArtemisCoreException("Could not import profile, failed to deserialize profile.json");

        // Assign a new GUID to make sure it is unique in case of a previous import of the same content
        if (makeUnique)
            profileEntity.UpdateGuid(Guid.NewGuid());
        else
        {
            // If the profile already exists and this one is not to be made unique, return the existing profile
            ProfileConfiguration? existing = ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(p => p.ProfileId == profileEntity.Id);
            if (existing != null)
                return existing;
        }

        if (nameAffix != null)
            profileEntity.Name = $"{profileEntity.Name} - {nameAffix}";
        if (markAsFreshImport)
            profileEntity.IsFreshImport = true;

        if (makeUnique && ProfileCategories.SelectMany(c => c.ProfileConfigurations).Any(c => c.ProfileId == profileEntity.Id))
            throw new ArtemisCoreException($"Cannot import this profile without {nameof(makeUnique)} being true");

        ProfileContainerEntity containerEntity = new() {ProfileConfiguration = configurationEntity, Profile = profileEntity};
        // If an icon was provided, import that as well
        if (iconEntry != null)
        {
            await using Stream iconStream = iconEntry.Open();
            using MemoryStream ms = new();
            await iconStream.CopyToAsync(ms);
            containerEntity.Icon = ms.ToArray();
        }

        // A new GUID will be given on save
        configurationEntity.FileIconId = Guid.Empty;
        ProfileConfiguration profileConfiguration = new(category, containerEntity);
        if (nameAffix != null)
            profileConfiguration.Name = $"{profileConfiguration.Name} - {nameAffix}";

        profileConfiguration.Entity.ProfileConfiguration.ProfileId = profileEntity.Id;
        category.AddProfileConfiguration(profileConfiguration, targetIndex);

        List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>();
        profileConfiguration.LoadModules(modules);
        SaveProfileCategory(category);

        return profileConfiguration;
    }

    /// <inheritdoc />
    public async Task<ProfileConfiguration> OverwriteProfile(MemoryStream archiveStream, ProfileConfiguration profileConfiguration)
    {
        ProfileConfiguration imported = await ImportProfile(archiveStream, profileConfiguration.Category, true, true, null, profileConfiguration.Order + 1);

        DeleteProfile(profileConfiguration);
        SaveProfileCategory(imported.Category);

        return imported;
    }

    /// <inheritdoc />
    public void AdaptProfile(Profile profile)
    {
        List<ArtemisDevice> devices = _deviceService.EnabledDevices.ToList();
        foreach (Layer layer in profile.GetAllLayers())
            layer.Adapter.Adapt(devices);

        profile.Save();

        foreach (RenderProfileElement renderProfileElement in profile.GetAllRenderElements())
            renderProfileElement.Save();

        _logger.Debug("Adapt profile - Saving " + profile);
        SaveProfileCategory(profile.Configuration.Category);
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        lock (_profileCategories)
        {
            _pendingKeyboardEvents.Add(e);
        }
    }

    private void MigrateProfile(JsonObject? configurationJson, JsonObject? profileJson)
    {
        if (configurationJson == null || profileJson == null)
            return;

        configurationJson["Version"] ??= 0;

        foreach (IProfileMigration profileMigrator in _profileMigrators.OrderBy(m => m.Version))
        {
            if (profileMigrator.Version <= configurationJson["Version"]!.GetValue<int>())
                continue;

            profileMigrator.Migrate(configurationJson, profileJson);
            configurationJson["Version"] = profileMigrator.Version;
        }
    }

    /// <summary>
    ///     Populates all missing LEDs on all currently active profiles
    /// </summary>
    private void ActiveProfilesPopulateLeds()
    {
        foreach (ProfileCategory profileCategory in ProfileCategories)
        {
            foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
            {
                if (profileConfiguration.Profile == null) continue;
                profileConfiguration.Profile.PopulateLeds(_deviceService.EnabledDevices);

                if (!profileConfiguration.Profile.IsFreshImport) continue;
                _logger.Debug("Profile is a fresh import, adapting to surface - {profile}", profileConfiguration.Profile);
                AdaptProfile(profileConfiguration.Profile);
            }
        }
    }

    private void UpdateModules()
    {
        List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>();
        foreach (ProfileCategory profileCategory in ProfileCategories)
        {
            foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                profileConfiguration.LoadModules(modules);
        }
    }

    private void DeviceServiceOnLedsChanged(object? sender, EventArgs e)
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