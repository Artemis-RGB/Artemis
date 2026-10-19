using Artemis.Core.Internal;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using SkiaSharp;

namespace Artemis.Core.Tests.Services;

public class NodeServiceTests : ServiceTestBase
{
    private INodeService Nodes => Resolve<INodeService>();

    [Fact]
    public void AvailableNodes_ContainsTheNodesOfEnabledProviders()
    {
        TestNodeProvider provider = EnableFeature(new TestNodeProvider());

        List<NodeData> nodes = Nodes.AvailableNodes.Where(n => n.Provider == provider).ToList();

        nodes.Select(n => n.Type).ShouldBe([typeof(TestBooleanNode), typeof(TestNumericNode)], ignoreOrder: true);
        NodeData booleanNode = nodes.Single(n => n.Type == typeof(TestBooleanNode));
        booleanNode.Name.ShouldBe("Test boolean");
        booleanNode.Category.ShouldBe("Tests");
        booleanNode.OutputType.ShouldBe(typeof(bool));
    }

    [Fact]
    public void AvailableNodes_NoLongerContainsTheNodesOfADisabledProvider()
    {
        TestNodeProvider provider = EnableFeature(new TestNodeProvider());

        DisableFeature(provider);

        Nodes.AvailableNodes.ShouldNotContain(n => n.Provider == provider);
    }

    [Fact]
    public void GetRegisteredTypes_ContainsTypesThatWereGivenAColor()
    {
        EnableFeature(new TestNodeProvider());
        EnableFeature(new TypeColorTestNodeProvider());

        // Only types with a registered color count, the pin types of registered nodes don't
        List<Type> types = Nodes.GetRegisteredTypes();
        types.ShouldContain(typeof(Numeric));
        types.ShouldContain(typeof(string));
        types.ShouldNotContain(typeof(bool));
    }

    [Fact]
    public void GetTypeColorRegistration_ForARegisteredType_ReturnsItsColor()
    {
        TypeColorTestNodeProvider provider = EnableFeature(new TypeColorTestNodeProvider());

        TypeColorRegistration registration = Nodes.GetTypeColorRegistration(typeof(string));

        registration.Type.ShouldBe(typeof(string));
        registration.Color.ShouldBe(TypeColorTestNodeProvider.TextColor);
        registration.PluginFeature.ShouldBeSameAs(provider);
    }

    [Fact]
    public void GetTypeColorRegistration_ForATypeThatConvertsToARegisteredType_StillMakesUpAColor()
    {
        EnableFeature(new TypeColorTestNodeProvider());

        // Numeric can be created from an int, but only conversions declared on the requested type itself are considered
        TypeColorRegistration registration = Nodes.GetTypeColorRegistration(typeof(int));

        registration.Type.ShouldBe(typeof(int));
        registration.Color.ShouldNotBe(TypeColorTestNodeProvider.NumberColor);
        registration.PluginFeature.ShouldBeSameAs(Constants.CorePluginFeature);
    }

    [Fact]
    public void GetTypeColorRegistration_NoLongerUsesTheColorsOfADisabledProvider()
    {
        TypeColorTestNodeProvider provider = EnableFeature(new TypeColorTestNodeProvider());

        DisableFeature(provider);

        Nodes.GetRegisteredTypes().ShouldNotContain(typeof(string));
        Nodes.GetTypeColorRegistration(typeof(string)).PluginFeature.ShouldBeSameAs(Constants.CorePluginFeature);
    }

    [Fact]
    public void GetTypeColorRegistration_ForObject_IsWhite()
    {
        TypeColorRegistration registration = Nodes.GetTypeColorRegistration(typeof(object));

        registration.Color.ShouldBe(SKColors.White);
        registration.PluginFeature.ShouldBeSameAs(Constants.CorePluginFeature);
    }

    [Fact]
    public void GetTypeColorRegistration_ForAnUnregisteredType_MakesUpTheSameColorEveryTime()
    {
        TypeColorRegistration first = Nodes.GetTypeColorRegistration(typeof(NodeServiceTests));
        TypeColorRegistration second = Nodes.GetTypeColorRegistration(typeof(NodeServiceTests));

        first.ShouldNotBeSameAs(second);
        first.Type.ShouldBe(typeof(NodeServiceTests));
        first.Color.ShouldBe(second.Color);
        first.PluginFeature.ShouldBeSameAs(Constants.CorePluginFeature);
    }

    [Fact]
    public void ExportScript_ThenImportScript_RestoresTheScript()
    {
        TestNodeProvider provider = EnableFeature(new TestNodeProvider());
        NodeScript<bool> script = new("Test", "A test script");
        TestBooleanNode node = provider.AddNode<TestBooleanNode>(script);
        node.Value = true;
        node.X = 12;
        script.Nodes.OfType<ExitNode<bool>>().Single().Input.ConnectTo(node.Output);

        string json = Nodes.ExportScript(script);
        NodeScript<bool> target = new("Target", "Another script");
        Nodes.ImportScript(json, target);
        target.Run();

        TestBooleanNode imported = target.Nodes.OfType<TestBooleanNode>().Single();
        imported.Id.ShouldBe(node.Id);
        imported.X.ShouldBe(12);
        target.ExitNodeConnected.ShouldBeTrue();
        target.Result.ShouldBeTrue();
    }

    [Fact]
    public void ImportScript_ReplacesWhatWasInTheTarget()
    {
        TestNodeProvider provider = EnableFeature(new TestNodeProvider());
        NodeScript<bool> source = new("Source", "Script with a node");
        provider.AddNode<TestNumericNode>(source);
        NodeScript<bool> target = new("Target", "Script with another node");
        provider.AddNode<TestBooleanNode>(target);

        Nodes.ImportScript(Nodes.ExportScript(source), target);

        target.Nodes.OfType<TestBooleanNode>().ShouldBeEmpty();
        target.Nodes.OfType<TestNumericNode>().Count().ShouldBe(1);
    }

    [Fact]
    public void ImportScript_WithNull_Throws()
    {
        NodeScript<bool> target = new("Target", "A script");

        Should.Throw<ArtemisCoreException>(() => Nodes.ImportScript("null", target));
    }
}
