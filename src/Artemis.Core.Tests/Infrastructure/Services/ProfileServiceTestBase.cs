using Artemis.Core.Internal;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Entities.Profile;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     Base class for tests on the profile service, with helpers to build profiles the way the editor does.
/// </summary>
public abstract class ProfileServiceTestBase : ServiceTestBase
{
    protected IProfileService ProfileService => Resolve<IProfileService>();

    /// <summary>
    ///     Gets the category the default categories end with
    /// </summary>
    protected ProfileCategory General => ProfileService.ProfileCategories.Single(c => c.Name == "General");

    /// <summary>
    ///     Gets the categories as stored in the database, ordered by their order
    /// </summary>
    protected List<ProfileCategoryEntity> GetStoredCategories()
    {
        using var context = Services.CreateDbContext();
        return context.ProfileCategories.Include(c => c.ProfileConfigurations).OrderBy(c => c.Order).ToList();
    }

    /// <summary>
    ///     Gets the profile configurations as stored in the database
    /// </summary>
    protected List<ProfileContainerEntity> GetStoredProfiles()
    {
        using var context = Services.CreateDbContext();
        return context.ProfileContainers.ToList();
    }

    /// <summary>
    ///     Adds a layer covering the provided LEDs to the profile, using the test brush of the provided type.
    /// </summary>
    protected static Layer AddLayer<TBrush>(Profile profile, TestLayerBrushProvider brushes, IEnumerable<ArtemisLed> leds, string name = "Test layer") where TBrush : BaseLayerBrush
    {
        Layer layer = new(profile.GetRootFolder(), name);
        profile.GetRootFolder().AddChild(layer);
        layer.AddLeds(leds);
        LayerBrushDescriptor descriptor = brushes.LayerBrushDescriptors.Single(d => d.LayerBrushType == typeof(TBrush));
        layer.ChangeLayerBrush(descriptor.CreateInstance(layer, null));
        return layer;
    }

    /// <summary>
    ///     Adds the test effect of the provided type to the element.
    /// </summary>
    protected static TEffect AddEffect<TEffect>(RenderProfileElement element, TestLayerEffectProvider effects) where TEffect : BaseLayerEffect
    {
        LayerEffectDescriptor descriptor = effects.LayerEffectDescriptors.Single(d => d.LayerEffectType == typeof(TEffect));
        TEffect effect = (TEffect) descriptor.CreateInstance(element, null);
        element.AddLayerEffect(effect);
        return effect;
    }

    /// <summary>
    ///     Makes the activation condition of the profile configuration follow the value of the returned node
    /// </summary>
    protected static TestBooleanNode UseActivationCondition(ProfileConfiguration configuration, TestNodeProvider nodes, bool isMet)
    {
        TestBooleanNode node = nodes.AddNode<TestBooleanNode>(configuration.ActivationCondition);
        node.Value = isMet;
        configuration.ActivationCondition.Nodes.OfType<ExitNode<bool>>().Single().Input.ConnectTo(node.Output);
        return node;
    }
}
