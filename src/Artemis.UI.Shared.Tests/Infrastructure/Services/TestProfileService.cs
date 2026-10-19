using System.Collections.ObjectModel;
using Artemis.Core.Services;
using SkiaSharp;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     Records what the profile editor asks of the profile service, without a database or render loop.
/// </summary>
public class TestProfileService : IProfileService
{
    private ProfileElement? _focusProfileElement;

    /// <summary>
    ///     Gets every value <see cref="FocusProfileElement" /> was set to, in order
    /// </summary>
    public List<ProfileElement?> FocusProfileElementChanges { get; } = [];

    /// <summary>
    ///     Gets every profile configuration passed to <see cref="ActivateProfile" />, in order
    /// </summary>
    public List<ProfileConfiguration> ActivatedProfiles { get; } = [];

    /// <summary>
    ///     Gets every call to <see cref="SaveProfile" />, in order. Saving may happen on another thread.
    /// </summary>
    public List<(Profile Profile, bool IncludeChildren)> SavedProfiles { get; } = [];

    /// <summary>
    ///     Gets or sets an exception <see cref="SaveProfile" /> should throw
    /// </summary>
    public Exception? SaveException { get; set; }

    public ReadOnlyCollection<ProfileCategory> ProfileCategories { get; } = new([]);
    public ProfileConfiguration? FocusProfile { get; set; }

    public ProfileElement? FocusProfileElement
    {
        get => _focusProfileElement;
        set
        {
            _focusProfileElement = value;
            FocusProfileElementChanges.Add(value);
        }
    }

    public bool UpdateFocusProfile { get; set; }
    public bool ProfileRenderingDisabled { get; set; }

    public Profile ActivateProfile(ProfileConfiguration profileConfiguration)
    {
        // Like ProfileService, a profile that is already active is returned as-is
        ActivatedProfiles.Add(profileConfiguration);
        return profileConfiguration.Profile ?? throw new InvalidOperationException("Test profile configurations come with a profile");
    }

    public void SaveProfile(Profile profile, bool includeChildren)
    {
        lock (SavedProfiles)
            SavedProfiles.Add((profile, includeChildren));
        if (SaveException != null)
            throw SaveException;
    }

    public ProfileConfiguration CloneProfileConfiguration(ProfileConfiguration profileConfiguration) => throw new NotSupportedException();
    public void DeactivateProfile(ProfileConfiguration profileConfiguration) => throw new NotSupportedException();
    public void SaveProfileCategory(ProfileCategory profileCategory) => throw new NotSupportedException();
    public ProfileCategory CreateProfileCategory(string name, bool addToTop = false) => throw new NotSupportedException();
    public void DeleteProfileCategory(ProfileCategory profileCategory) => throw new NotSupportedException();
    public ProfileConfiguration CreateProfileConfiguration(ProfileCategory category, string name, string icon) => throw new NotSupportedException();
    public void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration) => throw new NotSupportedException();
    public Task<Stream> ExportProfile(ProfileConfiguration profileConfiguration) => throw new NotSupportedException();

    public Task<ProfileConfiguration> ImportProfile(Stream archiveStream, ProfileCategory category, bool makeUnique, bool markAsFreshImport, string? nameAffix = "imported",
        ProfileConfiguration? target = null) => throw new NotSupportedException();

    public void AdaptProfile(Profile profile) => throw new NotSupportedException();
    public void UpdateProfiles(double deltaTime) => throw new NotSupportedException();
    public void RenderProfiles(SKCanvas canvas) => throw new NotSupportedException();

#pragma warning disable CS0067 // Events are part of the interface but never raised here
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileActivated;
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileDeactivated;
    public event EventHandler<ProfileCategoryEventArgs>? ProfileCategoryAdded;
    public event EventHandler<ProfileCategoryEventArgs>? ProfileCategoryRemoved;
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileRemoved;
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileAdded;
#pragma warning restore CS0067
}
