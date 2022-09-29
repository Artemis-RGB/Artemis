using System.Globalization;
using System.Reflection;

namespace Artemis.Core.Services.Core;

/// <summary>
///     Represents build information related to the currently running Artemis build
/// </summary>
public class BuildInfo
{
    /// <summary>
    ///     Gets the build number. This contains the date and the build count for that day.
    ///     <para>Per example <c>20210108.4</c></para>
    /// </summary>
    public double BuildNumber { get; internal set; }

    /// <summary>
    ///     Gets the build number formatted as a string. This contains the date and the build count for that day.
    ///     <para>Per example <c>20210108.4</c></para>
    /// </summary>
    public string BuildNumberDisplay => BuildNumber.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    ///     Gets the branch of the triggering repo the build was created for.
    /// </summary>
    public string SourceBranch { get; internal set; } = null!;

    /// <summary>
    ///     Gets a boolean indicating whether the current build is a local build
    /// </summary>
    public bool IsLocalBuild { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildInfo"/> class.
    /// </summary>
    /// <param name="attribute">The attribute to extract version information from.</param>
    public BuildInfo(AssemblyInformationalVersionAttribute? attribute)
    {
        void SetLocalBuildInformation()
        {
            BuildNumber = -1;
            SourceBranch = "local";
            IsLocalBuild = true;
        }

        if (attribute is null)
        {
            SetLocalBuildInformation();
            return;
        }

        string version = attribute.InformationalVersion;
        string pluginApiVersion = Constants.PluginApi.ToString();
        if (version == pluginApiVersion)
        {
            SetLocalBuildInformation();
            return;
        }

        try
        {
            //1.0.0-master.20220920.3 -> master.20220920.3
            string versionMetadata = version.Remove(0, pluginApiVersion.Length + 1);

            //master | 20220920.3
            string[] parts = versionMetadata.Split('.', 2);

            SourceBranch = parts[0];
            BuildNumber = double.Parse(parts[1]);
            IsLocalBuild = false;
        }
        catch
        {
            SetLocalBuildInformation();
        }
    }
}