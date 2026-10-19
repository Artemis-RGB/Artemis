using Artemis.Core.Modules;

namespace Artemis.Core.Tests.Infrastructure.Plugins.Modules;

/// <summary>
///     A module exposing a <see cref="TestDataModel" />. Add it with <see cref="TestPlugin.AddFeature{T}" />, which also
///     registers the data model the way the data model service does.
/// </summary>
public class TestModule : Module<TestDataModel>
{
    /// <summary>
    ///     Gets or sets the activation requirements, none by default
    /// </summary>
    public List<IModuleActivationRequirement>? Requirements { get; set; }

    /// <summary>
    ///     Gets the total time the module was updated for
    /// </summary>
    public double UpdatedFor { get; private set; }

    public override List<IModuleActivationRequirement>? ActivationRequirements => Requirements;

    public override void Enable()
    {
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
        UpdatedFor += deltaTime;
    }
}
