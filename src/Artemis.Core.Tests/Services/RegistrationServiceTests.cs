using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Services;

namespace Artemis.Core.Tests.Services;

public class DataModelServiceTests : ServiceTestBase
{
    [Fact]
    public void ModulesEnabledBeforeTheServiceStarts_HaveTheirDataModelRegistered()
    {
        TestModule module = EnableFeature(new TestModule());

        IDataModelService dataModels = Resolve<IDataModelService>();

        dataModels.GetDataModels().ShouldContain(module.DataModel);
        dataModels.GetPluginDataModel(module).ShouldBeSameAs(module.DataModel);
    }

    [Fact]
    public void ModulesEnabledLater_HaveTheirDataModelRegistered()
    {
        IDataModelService dataModels = Resolve<IDataModelService>();

        TestModule module = EnableFeature(new TestModule());

        dataModels.GetDataModels().ShouldContain(module.DataModel);
        dataModels.GetDataModel<TestDataModel>().ShouldBeSameAs(module.DataModel);
    }

    [Fact]
    public void DisablingAModule_RemovesItsDataModel()
    {
        IDataModelService dataModels = Resolve<IDataModelService>();
        TestModule module = EnableFeature(new TestModule());
        TestDataModel dataModel = module.DataModel;

        DisableFeature(module);

        dataModels.GetDataModels().ShouldNotContain(dataModel);
        dataModels.GetPluginDataModel(module).ShouldBeNull();
        dataModels.GetDataModel<TestDataModel>().ShouldBeNull();
    }

    [Fact]
    public void AModuleWithoutRequirements_IsAnExpansion()
    {
        Resolve<IDataModelService>();

        TestModule alwaysAvailable = EnableFeature(new TestModule());
        TestModule withRequirements = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement()]});

        // Expansions extend the data model of every profile rather than only of profiles targeting the module
        alwaysAvailable.DataModel.IsExpansion.ShouldBeTrue();
        withRequirements.DataModel.IsExpansion.ShouldBeFalse();
    }

    [Fact]
    public void DataModel_DescribesTheModule()
    {
        Resolve<IDataModelService>();

        TestModule module = EnableFeature(new TestModule());

        module.DataModel.DataModelDescription.ShouldNotBeNull();
        module.DataModel.DataModelDescription.Name.ShouldBe(module.Info.Name);
        module.DataModel.Module.ShouldBeSameAs(module);
    }

    [Fact]
    public void RegisterDataModel_TheSameDataModelTwice_Throws()
    {
        IDataModelService dataModels = Resolve<IDataModelService>();
        TestModule module = EnableFeature(new TestModule());

        Should.Throw<ArtemisCoreException>(() => dataModels.RegisterDataModel(module.DataModel));
    }

    [Fact]
    public void RemoveDataModel_RemovesTheRegistration()
    {
        IDataModelService dataModels = Resolve<IDataModelService>();
        TestModule module = EnableFeature(new TestModule());
        DataModelRegistration registration = DataModelStore.GetAll().Single(r => r.DataModel == module.DataModel);

        dataModels.RemoveDataModel(registration);

        registration.IsInStore.ShouldBeFalse();
        dataModels.GetDataModels().ShouldNotContain(module.DataModel);
        // Removing it twice is not allowed
        Should.Throw<ArtemisCoreException>(() => dataModels.RemoveDataModel(registration));
    }

    [Fact]
    public void NullArguments_Throw()
    {
        IDataModelService dataModels = Resolve<IDataModelService>();

        Should.Throw<ArgumentNullException>(() => dataModels.RegisterDataModel(null!));
        Should.Throw<ArgumentNullException>(() => dataModels.RemoveDataModel(null!));
    }
}

public class LayerBrushServiceTests : ServiceTestBase
{
    private const string DefaultBrushSetting = "ProfileEditor.DefaultLayerBrushDescriptor";

    [Fact]
    public void GetLayerBrushes_ContainsTheBrushesOfEnabledProviders()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();

        TestLayerBrushProvider provider = EnableFeature(new TestLayerBrushProvider());

        brushes.GetLayerBrushes().Where(d => d.Provider == provider).Select(d => d.LayerBrushType)
            .ShouldBe([typeof(SolidColorTestBrush), typeof(GradientTestBrush), typeof(PerLedTestBrush)]);
    }

    [Fact]
    public void GetLayerBrushes_NoLongerContainsTheBrushesOfADisabledProvider()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        TestLayerBrushProvider provider = EnableFeature(new TestLayerBrushProvider());

        DisableFeature(provider);

        brushes.GetLayerBrushes().ShouldNotContain(d => d.Provider == provider);
    }

    [Fact]
    public void RegisterAndRemoveLayerBrush_AddsAndRemovesTheBrush()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        TestLayerBrushProvider provider = EnableFeature(new TestLayerBrushProvider());
        LayerBrushDescriptor descriptor = provider.LayerBrushDescriptors.First();
        LayerBrushRegistration registration = LayerBrushStore.Get(provider.Id, descriptor.LayerBrushType.Name)!;

        brushes.RemoveLayerBrush(registration);
        brushes.GetLayerBrushes().ShouldNotContain(descriptor);

        LayerBrushRegistration added = brushes.RegisterLayerBrush(descriptor);
        added.LayerBrushDescriptor.ShouldBeSameAs(descriptor);
        brushes.GetLayerBrushes().ShouldContain(descriptor);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();

        Should.Throw<ArgumentNullException>(() => brushes.RegisterLayerBrush(null!));
        Should.Throw<ArgumentNullException>(() => brushes.RemoveLayerBrush(null!));
    }

    [Fact]
    public void GetDefaultLayerBrush_WithoutTheColorBrushPlugin_IsNull()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        EnableFeature(new TestLayerBrushProvider());

        // By default this is the solid brush of the Color brush plugin in the Artemis.Plugins repository
        brushes.GetDefaultLayerBrush().ShouldBeNull();

        LayerBrushReference reference = Resolve<ISettingsService>().GetSetting<LayerBrushReference>(DefaultBrushSetting).Value!;
        reference.LayerBrushProviderId.ShouldBe("Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba");
        reference.BrushType.ShouldBe("SolidBrush");
    }

    [Fact]
    public void GetDefaultLayerBrush_FollowsTheSetting()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        TestLayerBrushProvider provider = EnableFeature(new TestLayerBrushProvider());
        LayerBrushDescriptor gradient = provider.LayerBrushDescriptors.Single(d => d.LayerBrushType == typeof(GradientTestBrush));

        Resolve<ISettingsService>().GetSetting<LayerBrushReference>(DefaultBrushSetting).Value = new LayerBrushReference(gradient);

        brushes.GetDefaultLayerBrush().ShouldBeSameAs(gradient);
    }

    [Fact]
    public void GetDefaultLayerBrush_FillsInAnEmptySetting()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        PluginSetting<LayerBrushReference> setting = Resolve<ISettingsService>().GetSetting<LayerBrushReference>(DefaultBrushSetting);
        setting.Value = new LayerBrushReference();

        brushes.GetDefaultLayerBrush();

        setting.Value!.LayerBrushProviderId.ShouldBe("Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba");
        setting.Value.BrushType.ShouldBe("SolidBrush");
    }

    [Fact]
    public void ApplyDefaultBrush_GivesTheLayerTheDefaultBrush()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        TestLayerBrushProvider provider = EnableFeature(new TestLayerBrushProvider());
        LayerBrushDescriptor solid = provider.LayerBrushDescriptors.Single(d => d.LayerBrushType == typeof(SolidColorTestBrush));
        Resolve<ISettingsService>().GetSetting<LayerBrushReference>(DefaultBrushSetting).Value = new LayerBrushReference(solid);
        Layer layer = CreateLayer();

        brushes.ApplyDefaultBrush(layer);

        layer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>();
    }

    [Fact]
    public void ApplyDefaultBrush_WithoutTheDefaultBrush_LeavesTheLayerAlone()
    {
        ILayerBrushService brushes = Resolve<ILayerBrushService>();
        Layer layer = CreateLayer();

        brushes.ApplyDefaultBrush(layer);

        layer.LayerBrush.ShouldBeNull();
    }

    private static Layer CreateLayer()
    {
        ProfileConfiguration configuration = new(new ProfileCategory("Tests", 0), "Test profile", "TestTube");
        Profile profile = new(configuration, configuration.Entity.Profile);
        Layer layer = new(profile.GetRootFolder(), "Test layer");
        profile.GetRootFolder().AddChild(layer);
        return layer;
    }
}

public class LayerEffectServiceTests : ServiceTestBase
{
    [Fact]
    public void GetLayerEffects_ContainsTheEffectsOfEnabledProviders()
    {
        ILayerEffectService effects = Resolve<ILayerEffectService>();

        TestLayerEffectProvider provider = EnableFeature(new TestLayerEffectProvider());

        effects.GetLayerEffects().Where(d => d.Provider == provider).Select(d => d.LayerEffectType)
            .ShouldBe([typeof(OpacityTestEffect), typeof(ChannelSwapTestEffect), typeof(TintTestEffect), typeof(GlowTestEffect)]);
    }

    [Fact]
    public void GetLayerEffects_NoLongerContainsTheEffectsOfADisabledProvider()
    {
        ILayerEffectService effects = Resolve<ILayerEffectService>();
        TestLayerEffectProvider provider = EnableFeature(new TestLayerEffectProvider());

        DisableFeature(provider);

        effects.GetLayerEffects().ShouldNotContain(d => d.Provider == provider);
    }

    [Fact]
    public void RegisterAndRemoveLayerEffect_AddsAndRemovesTheEffect()
    {
        ILayerEffectService effects = Resolve<ILayerEffectService>();
        TestLayerEffectProvider provider = EnableFeature(new TestLayerEffectProvider());
        LayerEffectDescriptor descriptor = provider.LayerEffectDescriptors.First();
        // Unlike brushes, effects are looked up by the full name of their type
        LayerEffectRegistration registration = LayerEffectStore.Get(provider.Id, descriptor.LayerEffectType!.FullName!)!;

        effects.RemoveLayerEffect(registration);
        effects.GetLayerEffects().ShouldNotContain(descriptor);

        LayerEffectRegistration added = effects.RegisterLayerEffect(descriptor);
        added.LayerEffectDescriptor.ShouldBeSameAs(descriptor);
        effects.GetLayerEffects().ShouldContain(descriptor);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        ILayerEffectService effects = Resolve<ILayerEffectService>();

        Should.Throw<ArgumentNullException>(() => effects.RegisterLayerEffect(null!));
        Should.Throw<ArgumentNullException>(() => effects.RemoveLayerEffect(null!));
    }
}
