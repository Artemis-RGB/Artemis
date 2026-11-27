using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core.Tests.VisualScripting;

public class NodeDataTests : ScriptTestBase
{
    [Fact]
    public void Registering_ReadsTheNodeAttribute()
    {
        NodeData sum = Data<TestSumNode>();
        sum.Provider.ShouldBe(ScriptNodes);
        sum.Type.ShouldBe(typeof(TestSumNode));
        sum.Name.ShouldBe("Test sum");
        sum.Description.ShouldBe("Adds up its inputs");
        sum.Category.ShouldBe("Tests");
        sum.HelpUrl.ShouldBeEmpty();
        sum.InputType.ShouldBe(typeof(Numeric));
        sum.OutputType.ShouldBe(typeof(Numeric));

        NodeData settings = Data<TestSettingsNode>();
        settings.HelpUrl.ShouldBe("https://example.com/node-from-attribute");
        settings.InputType.ShouldBeNull();
        settings.OutputType.ShouldBeNull();

        NodeData text = Data<TestTextNode>();
        text.Name.ShouldBe("Test text");
        text.Description.ShouldBe("Outputs text controlled by the test");
        text.Category.ShouldBeEmpty();

        NodeData viewModel = Data<TestViewModelNode>();
        viewModel.Name.ShouldBe("Test view model");
        viewModel.Description.ShouldBeEmpty();
    }

    [Fact]
    public void Registering_AddsTheNodeToTheNodeTypeStore()
    {
        NodeTypeStore.GetAll().ShouldContain(Data<TestSumNode>());
    }

    [Fact]
    public void DisablingTheProvider_RemovesItsNodesFromTheNodeTypeStore()
    {
        TestPlugin otherPlugin = new();
        ScriptTestNodeProvider otherNodes = otherPlugin.AddFeature(new ScriptTestNodeProvider());
        NodeData sum = otherNodes.NodeDescriptors.Single(d => d.Type == typeof(TestSumNode));
        NodeTypeStore.GetAll().ShouldContain(sum);

        otherPlugin.Dispose();

        NodeTypeStore.GetAll().ShouldNotContain(sum);
        otherNodes.NodeDescriptors.ShouldBeEmpty();
    }

    [Fact]
    public void CreateNode_FillsInWhatTheNodeDoesNotSetItself()
    {
        NodeScript<bool> script = CreateScript<bool>();

        TestSettingsNode settings = (TestSettingsNode) Data<TestSettingsNode>().CreateNode(script, null);
        TestSumNode sum = (TestSumNode) Data<TestSumNode>().CreateNode(script, null);

        settings.Name.ShouldBe("Own name");
        settings.Description.ShouldBe("Stores settings");
        settings.HelpUrl.ShouldBe("https://example.com/own-help");
        settings.NodeData.ShouldBe(Data<TestSettingsNode>());
        sum.Name.ShouldBe("Test sum");
        sum.Description.ShouldBe("Adds up its inputs");
    }

    [Fact]
    public void CreateNode_DoesNotAddTheNodeToTheScript()
    {
        NodeScript<bool> script = CreateScript<bool>();

        INode node = Data<TestSumNode>().CreateNode(script, null);

        script.Nodes.ShouldNotContain(node);
        node.IsLoading.ShouldBeFalse();
    }

    [Fact]
    public void CreateNode_FromAnEntity_AppliesPositionAndStorage()
    {
        NodeScript<bool> script = CreateScript<bool>();
        NodeEntity entity = new() {X = 3, Y = 4, Storage = "\"Hello\""};

        TestTextNode node = (TestTextNode) Data<TestTextNode>().CreateNode(script, entity);

        node.X.ShouldBe(3);
        node.Y.ShouldBe(4);
        node.Value.ShouldBe("Hello");
        node.IsLoading.ShouldBeTrue();
    }

    [Fact]
    public void CreateNode_FromAnEntity_DoesNotTakeTheIdOfTheEntity()
    {
        // The script applies the ID afterwards
        NodeEntity entity = new() {Id = Guid.NewGuid()};

        INode node = Data<TestSumNode>().CreateNode(CreateScript<bool>(), entity);

        node.Id.ShouldNotBe(entity.Id);
    }

    [Fact]
    public void IsCompatibleWithPin_WithoutAPin_IsAlwaysTrue()
    {
        Data<TestSettingsNode>().IsCompatibleWithPin(null).ShouldBeTrue();
    }

    [Fact]
    public void IsCompatibleWithPin_ComparesTheOutputOfTheNodeWithAnInputPin()
    {
        TestNode node = new();
        InputPin<Numeric> numericInput = node.CreateInputPin<Numeric>();
        InputPin<string> textInput = node.CreateInputPin<string>();

        Data<TestNumericNode>().IsCompatibleWithPin(numericInput).ShouldBeTrue();
        Data<TestNumericNode>().IsCompatibleWithPin(textInput).ShouldBeFalse();
        Data<TestSettingsNode>().IsCompatibleWithPin(numericInput).ShouldBeFalse();
    }

    [Fact]
    public void IsCompatibleWithPin_ComparesTheInputOfTheNodeWithAnOutputPin()
    {
        TestNode node = new();
        OutputPin<Numeric> numericOutput = node.CreateOutputPin<Numeric>();
        OutputPin<string> textOutput = node.CreateOutputPin<string>();

        Data<TestSumNode>().IsCompatibleWithPin(numericOutput).ShouldBeTrue();
        Data<TestSumNode>().IsCompatibleWithPin(textOutput).ShouldBeFalse();
        Data<TestNumericNode>().IsCompatibleWithPin(numericOutput).ShouldBeFalse();
    }

    [Theory]
    [InlineData("sum", true)]
    [InlineData("SUM", true)]
    [InlineData("  sum  ", true)]
    [InlineData("adds up", true)]
    [InlineData("tests", true)]
    [InlineData("multiply", false)]
    [InlineData("!test", true)]
    [InlineData("!sum", false)]
    [InlineData("!!test sum", true)]
    [InlineData("!!test", false)]
    public void MatchesSearch_LooksAtNameDescriptionAndCategory(string search, bool matches)
    {
        // A single ! only matches the start of the name, !! only the whole name
        Data<TestSumNode>().MatchesSearch(search).ShouldBe(matches);
    }

    private NodeData Data<T>()
    {
        return NodeTypeStore.GetAll().Single(d => d.Type == typeof(T) && (d.Provider == ScriptNodes || d.Provider == Nodes));
    }
}
