using System;

namespace Artemis.Core;

/// <summary>
///     Provides data for profile configuration events.
/// </summary>
public class ProfileCategoryEventArgs : EventArgs
{
    internal ProfileCategoryEventArgs(ProfileCategory profileCategory)
    {
        ProfileCategory = profileCategory;
    }

    /// <summary>
    ///     Gets the profile category this event is related to
    /// </summary>
    public ProfileCategory ProfileCategory { get; }
}