using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using SkiaSharp;

namespace Artemis.Core.Tests.VisualScripting;

/// <summary>
///     The basics of the scripts behind data bindings. Data bindings themselves are tested with the rest of the profile.
/// </summary>
public class DataBindingNodeScriptTests : ScriptTestBase
{
    private readonly RenderHarness _harness = new();
    private readonly Profile _profile;
    private readonly Layer _layer;

    public DataBindingNodeScriptTests()
    {
        _profile = _harness.CreateProfile();
        (_layer, _) = _harness.AddLayer<SolidColorTestBrush>(_profile.GetRootFolder(), []);
    }

    private DataBinding<SKPoint> PositionBinding => _layer.Transform.Position.DataBinding;

    [Fact]
    public void Script_HasAnInputPerDataBindingProperty()
    {
        INodeScript script = PositionBinding.Script;

        script.ShouldBeOfType<DataBindingNodeScript<SKPoint>>();
        script.ResultType.ShouldBe(typeof(object));
        script.Name.ShouldBe(_layer.Transform.Position.PropertyDescription.Name);
        script.Context.ShouldBe(_profile);

        INode exitNode = ExitNode(script);
        exitNode.Pins.Select(p => (p.Name, p.Type, p.Direction)).ShouldBe([
            ("X", typeof(Numeric), PinDirection.Input),
            ("Y", typeof(Numeric), PinDirection.Input)
        ]);
    }

    [Fact]
    public void Script_ForAPropertyThatIsNotANumber_UsesThePropertyType()
    {
        SolidColorTestBrush brush = (SolidColorTestBrush) _layer.LayerBrush!;

        INode exitNode = ExitNode(brush.Properties.Color.DataBinding.Script);

        exitNode.Pins.ShouldHaveSingleItem().Type.ShouldBe(typeof(SKColor));
    }

    [Fact]
    public void ExitNodeConnected_IsTrueOnceAnyInputIsConnected()
    {
        NodeScript script = (NodeScript) PositionBinding.Script;
        script.ExitNodeConnected.ShouldBeFalse();

        TestNumericNode number = Nodes.AddNode<TestNumericNode>(script);
        ExitNode(script).Pins.Last().ConnectTo(number.Output);

        script.ExitNodeConnected.ShouldBeTrue();
    }

    [Fact]
    public void RunningTheScript_SetsOnlyTheConnectedProperties()
    {
        _layer.Transform.Position.CurrentValue = new SKPoint(1, 2);
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(PositionBinding.Script);
        number.Value = 42;
        ExitNode(PositionBinding.Script).Pins.First().ConnectTo(number.Output);
        PositionBinding.IsEnabled = true;

        PositionBinding.Update();
        PositionBinding.Apply();

        _layer.Transform.Position.CurrentValue.ShouldBe(new SKPoint(42, 2));
    }

    [Fact]
    public void DisabledDataBinding_DoesNotRunItsScript()
    {
        _layer.Transform.Position.CurrentValue = new SKPoint(1, 2);
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(PositionBinding.Script);
        number.Value = 42;
        ExitNode(PositionBinding.Script).Pins.First().ConnectTo(number.Output);

        PositionBinding.Update();
        PositionBinding.Apply();

        number.Evaluations.ShouldBe(0);
        _layer.Transform.Position.CurrentValue.ShouldBe(new SKPoint(1, 2));
    }

    [Fact]
    public void ClearingTheDataBindingProperties_RemovesTheInputs()
    {
        INode exitNode = ExitNode(PositionBinding.Script);
        IPin x = exitNode.Pins.First();
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(PositionBinding.Script);
        x.ConnectTo(number.Output);

        PositionBinding.ClearDataBindingProperties();

        exitNode.Pins.ShouldBeEmpty();
        number.Output.ConnectedTo.ShouldBeEmpty();
        PositionBinding.Script.ShouldBeAssignableTo<NodeScript>()!.ExitNodeConnected.ShouldBeFalse();
    }

    [Fact]
    public void RegisteringADataBindingProperty_RecreatesTheInputs()
    {
        INode exitNode = ExitNode(PositionBinding.Script);

        PositionBinding.RegisterDataBindingProperty(() => 1, _ => { }, "Extra");

        exitNode.Pins.Select(p => p.Name).ShouldBe(["X", "Y", "Extra"]);
        exitNode.Pins.Last().Type.ShouldBe(typeof(Numeric));
    }

    [Fact]
    public void Script_SurvivesSavingAndLoadingTheProfile()
    {
        TestNumericNode number = Nodes.AddNode<TestNumericNode>(PositionBinding.Script);
        number.Value = 42;
        number.X = 10;
        ExitNode(PositionBinding.Script).Pins.Last().ConnectTo(number.Output);
        PositionBinding.IsEnabled = true;

        Profile loaded = _harness.SaveAndReload(_profile);
        Layer loadedLayer = loaded.GetAllLayers().Single();
        DataBinding<SKPoint> loadedBinding = loadedLayer.Transform.Position.DataBinding;
        loadedBinding.Update();
        loadedBinding.Apply();

        TestNumericNode loadedNumber = loadedBinding.Script.Nodes.OfType<TestNumericNode>().Single();
        loadedNumber.Value.ShouldBe(42);
        loadedNumber.X.ShouldBe(10);
        loadedBinding.IsEnabled.ShouldBeTrue();
        loadedLayer.Transform.Position.CurrentValue.Y.ShouldBe(42);
    }

    public override void Dispose()
    {
        _harness.Dispose();
        base.Dispose();
    }

    private static INode ExitNode(INodeScript script)
    {
        return script.Nodes.Single(n => n.IsExitNode);
    }
}
