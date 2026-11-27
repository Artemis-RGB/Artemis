using Artemis.Core.Internal;
using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.DataBindings;

public class DataBindingTests : RenderTestBase
{
    private readonly TestModule _module;
    private readonly TestNodeProvider _nodes;
    private readonly DataModelTestNodeProvider _dataModelNodes;

    public DataBindingTests()
    {
        TestPlugin plugin = Track(new TestPlugin());
        _module = plugin.AddFeature(new TestModule());
        _nodes = plugin.AddFeature(new TestNodeProvider());
        _dataModelNodes = plugin.AddFeature(new DataModelTestNodeProvider());
    }

    #region Driving layer properties

    [Fact]
    public void LayerProperty_OffersItsValueToDataBindings()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;

        layer.Transform.Opacity.DataBindingsSupported.ShouldBeTrue();
        layer.Transform.Opacity.HasDataBinding.ShouldBeFalse();
        layer.Transform.Opacity.BaseDataBinding.ShouldBeSameAs(dataBinding);
        dataBinding.LayerProperty.ShouldBeSameAs(layer.Transform.Opacity);
        dataBinding.BaseLayerProperty.ShouldBeSameAs(layer.Transform.Opacity);
        IDataBindingProperty property = dataBinding.Properties.ShouldHaveSingleItem();
        property.DisplayName.ShouldBe("Value");
        property.ValueType.ShouldBe(typeof(float));
    }

    [Fact]
    public void LayerProperty_WithSeveralValues_OffersEachOfThem()
    {
        Layer layer = AddLayer();

        layer.Transform.Position.DataBinding.Properties.Select(p => p.DisplayName).ShouldBe(["X", "Y"]);
    }

    [Fact]
    public void ExitNode_HasAnInputForEachValue()
    {
        Layer layer = AddLayer();

        DataBindingExitNode<SKPoint> exitNode = ExitNode(layer.Transform.Position.DataBinding);

        exitNode.IsExitNode.ShouldBeTrue();
        // Numbers come in as Numeric, like everywhere else in node scripts
        exitNode.Pins.Select(p => p.Name).ShouldBe(["X", "Y"]);
        exitNode.Pins.ShouldAllBe(p => p.Type == typeof(Numeric));
    }

    [Fact]
    public void EnabledDataBinding_SetsTheValueFromItsScript()
    {
        Layer layer = AddLayer();
        BindNumber(layer.Transform.Opacity, 50);

        using RenderFrame frame = Render(layer.Profile);

        layer.Transform.Opacity.HasDataBinding.ShouldBeTrue();
        layer.Transform.Opacity.CurrentValue.ShouldBe(50);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
    }

    [Fact]
    public void DisabledDataBinding_LeavesTheValueAlone()
    {
        Layer layer = AddLayer();
        BindNumber(layer.Transform.Opacity, 50);
        layer.Transform.Opacity.DataBinding.IsEnabled = false;

        using RenderFrame frame = Render(layer.Profile);

        layer.Transform.Opacity.CurrentValue.ShouldBe(100);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void EnabledDataBinding_WithNothingConnected_LeavesTheValueAlone()
    {
        Layer layer = AddLayer();
        layer.Transform.Opacity.DataBinding.IsEnabled = true;

        using RenderFrame frame = Render(layer.Profile);

        layer.Transform.Opacity.CurrentValue.ShouldBe(100);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void DisconnectingTheScript_GivesBackTheBaseValue()
    {
        Layer layer = AddLayer();
        TestNumericNode node = BindNumber(layer.Transform.Opacity, 50);
        using (Render(layer.Profile))
        {
        }

        node.Output.DisconnectAll();
        using RenderFrame frame = Render(layer.Profile);

        layer.Transform.Opacity.CurrentValue.ShouldBe(100);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void DataBinding_FollowsItsScriptEveryFrame()
    {
        Layer layer = AddLayer();
        TestNumericNode node = BindNumber(layer.Transform.Opacity, 50);

        using RenderFrame first = Render(layer.Profile);
        node.Value = 0;
        using RenderFrame second = Render(layer.Profile);

        first.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
        second.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
        node.Evaluations.ShouldBe(2);
    }

    [Fact]
    public void DataBinding_CanDriveOneOfSeveralValues()
    {
        Layer layer = AddLayer();
        layer.Transform.Position.BaseValue = new SKPoint(1, 2);
        DataBinding<SKPoint> dataBinding = layer.Transform.Position.DataBinding;
        TestNumericNode node = _nodes.AddNode<TestNumericNode>(dataBinding.Script);
        node.Value = 7;
        node.Output.ConnectTo(ExitNode(dataBinding).Pins.Single(p => p.Name == "X"));
        dataBinding.IsEnabled = true;

        using (Render(layer.Profile))
        {
        }

        layer.Transform.Position.CurrentValue.ShouldBe(new SKPoint(7, 2));
    }

    [Fact]
    public void DataBinding_CanDriveAColor()
    {
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(Harness.CreateProfile().GetRootFolder(), [Led(LedId.Keyboard_A)]);
        DataBinding<SKColor> dataBinding = brush.Properties.Color.DataBinding;
        TestColorNode node = _dataModelNodes.AddNode<TestColorNode>(dataBinding.Script);
        node.Value = SKColors.Lime;
        node.Output.ConnectTo(ExitNode(dataBinding).Pins.Single());
        dataBinding.IsEnabled = true;

        using RenderFrame frame = Render(layer.Profile);

        ExitNode(dataBinding).Pins.Single().Type.ShouldBe(typeof(SKColor));
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Lime);
    }

    [Fact]
    public void DataBinding_CanFollowTheDataModel()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        TestDataModelNumberNode node = _dataModelNodes.AddNode<TestDataModelNumberNode>(dataBinding.Script);
        using (DataModelPath path = new(_module.DataModel, "Float"))
            node.UsePath(path);
        node.Output.ConnectTo(ExitNode(dataBinding).Pins.Single());
        dataBinding.IsEnabled = true;

        _module.DataModel.Float = 50;
        using RenderFrame half = Render(layer.Profile);
        _module.DataModel.Float = 100;
        using RenderFrame full = Render(layer.Profile);

        half.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
        full.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        dataBinding.GetFeatureDependencies().ShouldContain(_module);
        dataBinding.GetFeatureDependencies().ShouldContain(_dataModelNodes);
    }

    #endregion

    #region Keyframes

    [Fact]
    public void EnabledDataBinding_WinsFromKeyframes()
    {
        Layer layer = AddLayer();
        AddOpacityKeyframes(layer);
        BindNumber(layer.Transform.Opacity, 25);

        using RenderFrame frame = Render(layer.Profile, 0.5);

        layer.Transform.Opacity.CurrentValue.ShouldBe(25);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(63, 0, 0));
    }

    [Fact]
    public void DisabledDataBinding_LetsKeyframesThrough()
    {
        Layer layer = AddLayer();
        AddOpacityKeyframes(layer);
        BindNumber(layer.Transform.Opacity, 25);
        layer.Transform.Opacity.DataBinding.IsEnabled = false;

        using RenderFrame frame = Render(layer.Profile, 0.5);

        layer.Transform.Opacity.CurrentValue.ShouldBe(50);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
    }

    [Fact]
    public void EnabledDataBinding_WithNothingConnected_LetsKeyframesThrough()
    {
        Layer layer = AddLayer();
        AddOpacityKeyframes(layer);
        layer.Transform.Opacity.DataBinding.IsEnabled = true;

        using (Render(layer.Profile, 0.5))
        {
        }

        layer.Transform.Opacity.CurrentValue.ShouldBe(50);
    }

    [Fact]
    public void DataBinding_LeavesTheValuesItDoesNotDriveToTheKeyframes()
    {
        Layer layer = AddLayer();
        SKPointLayerProperty position = layer.Transform.Position;
        position.KeyframesEnabled = true;
        position.AddKeyframe(new LayerPropertyKeyframe<SKPoint>(new SKPoint(0, 0), TimeSpan.Zero, Easings.Functions.Linear, position));
        position.AddKeyframe(new LayerPropertyKeyframe<SKPoint>(new SKPoint(10, 10), TimeSpan.FromSeconds(1), Easings.Functions.Linear, position));
        TestNumericNode node = _nodes.AddNode<TestNumericNode>(position.DataBinding.Script);
        node.Value = 7;
        node.Output.ConnectTo(ExitNode(position.DataBinding).Pins.Single(p => p.Name == "Y"));
        position.DataBinding.IsEnabled = true;

        using (Render(layer.Profile, 0.5))
        {
        }

        position.CurrentValue.ShouldBe(new SKPoint(5, 7));
    }

    #endregion

    #region Saving and loading

    [Fact]
    public void DataBinding_SurvivesSavingAndLoading()
    {
        Layer layer = AddLayer();
        BindNumber(layer.Transform.Opacity, 50);

        Profile reloaded = Harness.SaveAndReload(layer.Profile);
        Layer reloadedLayer = reloaded.GetAllLayers().Single();
        using RenderFrame frame = Render(reloaded);

        DataBinding<float> dataBinding = reloadedLayer.Transform.Opacity.DataBinding;
        dataBinding.IsEnabled.ShouldBeTrue();
        dataBinding.Script.Nodes.OfType<TestNumericNode>().Single().Value.ShouldBe(50);
        ExitNode(dataBinding).Pins.Single().ConnectedTo.ShouldNotBeEmpty();
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
    }

    [Fact]
    public void DataBindingFollowingTheDataModel_SurvivesSavingAndLoading()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        TestDataModelNumberNode node = _dataModelNodes.AddNode<TestDataModelNumberNode>(dataBinding.Script);
        using (DataModelPath path = new(_module.DataModel, "Float"))
            node.UsePath(path);
        node.Output.ConnectTo(ExitNode(dataBinding).Pins.Single());
        dataBinding.IsEnabled = true;

        Profile reloaded = Harness.SaveAndReload(layer.Profile);
        _module.DataModel.Float = 50;
        using RenderFrame frame = Render(reloaded);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
    }

    [Fact]
    public void DisabledDataBinding_StaysDisabledAfterSavingAndLoading()
    {
        Layer layer = AddLayer();
        BindNumber(layer.Transform.Opacity, 50);
        layer.Transform.Opacity.DataBinding.IsEnabled = false;

        Profile reloaded = Harness.SaveAndReload(layer.Profile);
        Layer reloadedLayer = reloaded.GetAllLayers().Single();
        using RenderFrame frame = Render(reloaded);

        reloadedLayer.Transform.Opacity.DataBinding.IsEnabled.ShouldBeFalse();
        reloadedLayer.Transform.Opacity.DataBinding.Script.Nodes.OfType<TestNumericNode>().ShouldHaveSingleItem();
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void EmptyScript_IsNotSaved()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        dataBinding.IsEnabled = true;

        dataBinding.Save();

        dataBinding.Entity.IsEnabled.ShouldBeTrue();
        dataBinding.Entity.NodeScript.ShouldBeNull();
    }

    [Fact]
    public void ScriptWithUnconnectedNodes_IsSaved()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        _nodes.AddNode<TestNumericNode>(dataBinding.Script);

        dataBinding.Save();

        dataBinding.Entity.NodeScript.ShouldNotBeNull();
    }

    [Fact]
    public void LoadingTheNodeScript_ReplacesTheScript()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        _nodes.AddNode<TestNumericNode>(dataBinding.Script);
        dataBinding.Save();
        INodeScript before = dataBinding.Script;

        dataBinding.LoadNodeScript();

        dataBinding.Script.ShouldNotBeSameAs(before);
        dataBinding.Script.Nodes.OfType<TestNumericNode>().ShouldHaveSingleItem();
    }

    #endregion

    #region The data binding itself

    [Fact]
    public void EnablingAndDisabling_RaisesEvents()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        List<string> raised = [];
        dataBinding.DataBindingEnabled += (_, e) => raised.Add("enabled " + (e.DataBinding == dataBinding));
        dataBinding.DataBindingDisabled += (_, e) => raised.Add("disabled " + (e.DataBinding == dataBinding));

        dataBinding.IsEnabled = true;
        dataBinding.IsEnabled = false;

        raised.ShouldBe(["enabled True", "disabled True"]);
    }

    [Fact]
    public void RegisteringAProperty_AddsAnInputToTheExitNode()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        int registered = 0;
        dataBinding.DataBindingPropertyRegistered += (_, _) => registered++;

        DataBindingProperty<string> property = dataBinding.RegisterDataBindingProperty(() => "text", _ => { }, "Text");

        registered.ShouldBe(1);
        dataBinding.Properties.ShouldContain(property);
        ExitNode(dataBinding).Pins.Select(p => p.Name).ShouldBe(["Value", "Text"]);
        ExitNode(dataBinding).Pins.Last().Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void RegisteringAPropertyTwice_Throws()
    {
        Layer layer = AddLayer();

        Should.Throw<ArtemisCoreException>(() => layer.Transform.Opacity.DataBinding.RegisterDataBindingProperty(() => 1f, _ => { }, "Value"));
    }

    [Fact]
    public void ClearingProperties_RemovesTheInputsOfTheExitNode()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        int cleared = 0;
        dataBinding.DataBindingPropertiesCleared += (_, _) => cleared++;

        dataBinding.ClearDataBindingProperties();

        cleared.ShouldBe(1);
        dataBinding.Properties.ShouldBeEmpty();
        layer.Transform.Opacity.DataBindingsSupported.ShouldBeFalse();
        ExitNode(dataBinding).Pins.ShouldBeEmpty();
    }

    [Fact]
    public void DisposedDataBinding_CannotBeUsed()
    {
        Layer layer = AddLayer();
        DataBinding<float> dataBinding = layer.Transform.Opacity.DataBinding;
        dataBinding.IsEnabled = true;

        dataBinding.Dispose();

        dataBinding.IsEnabled.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(() => dataBinding.Update());
        Should.Throw<ObjectDisposedException>(() => dataBinding.Apply());
        Should.Throw<ObjectDisposedException>(() => dataBinding.Save());
        Should.Throw<ObjectDisposedException>(() => dataBinding.Load());
        Should.Throw<ObjectDisposedException>(() => dataBinding.ClearDataBindingProperties());
        Should.Throw<ObjectDisposedException>(() => dataBinding.RegisterDataBindingProperty(() => 1f, _ => { }, "Other"));
    }

    [Fact]
    public void Script_IsNamedAfterTheLayerProperty()
    {
        Layer layer = AddLayer();

        layer.Transform.Opacity.DataBinding.Script.Name.ShouldBe(layer.Transform.Opacity.PropertyDescription.Name);
    }

    #endregion

    #region Data binding properties

    [Fact]
    public void Property_ReadsAndWritesThroughItsGetterAndSetter()
    {
        float value = 1;
        DataBindingProperty<float> property = new(() => value, v => value = v, "Value");

        property.GetValue().ShouldBe(1f);
        property.SetValue(2f);

        value.ShouldBe(2);
        property.DisplayName.ShouldBe("Value");
        property.ValueType.ShouldBe(typeof(float));
    }

    [Fact]
    public void Property_TurnsNumericsIntoItsOwnNumberType()
    {
        float floatValue = 0;
        int intValue = 0;
        double doubleValue = 0;
        byte byteValue = 0;

        new DataBindingProperty<float>(() => floatValue, v => floatValue = v, "Float").SetValue(new Numeric(1.5f));
        new DataBindingProperty<int>(() => intValue, v => intValue = v, "Int").SetValue(new Numeric(2));
        new DataBindingProperty<double>(() => doubleValue, v => doubleValue = v, "Double").SetValue(new Numeric(3.5f));
        new DataBindingProperty<byte>(() => byteValue, v => byteValue = v, "Byte").SetValue(new Numeric(4));

        floatValue.ShouldBe(1.5f);
        intValue.ShouldBe(2);
        doubleValue.ShouldBe(3.5);
        byteValue.ShouldBe((byte) 4);
    }

    [Fact]
    public void Property_RejectsValuesOfAnotherType()
    {
        DataBindingProperty<float> numberProperty = new(() => 0, _ => { }, "Value");
        DataBindingProperty<string> textProperty = new(() => "", _ => { }, "Text");

        Should.Throw<ArgumentException>(() => numberProperty.SetValue("text"));
        Should.Throw<ArgumentException>(() => textProperty.SetValue(new Numeric(1)));
    }

    [Fact]
    public void Property_RequiresAGetterSetterAndName()
    {
        Should.Throw<ArgumentNullException>(() => new DataBindingProperty<float>(null!, _ => { }, "Value"));
        Should.Throw<ArgumentNullException>(() => new DataBindingProperty<float>(() => 0, null!, "Value"));
        Should.Throw<ArgumentNullException>(() => new DataBindingProperty<float>(() => 0, _ => { }, null!));
    }

    #endregion

    private Layer AddLayer()
    {
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(Harness.CreateProfile().GetRootFolder(), [Led(LedId.Keyboard_A)]);
        return layer;
    }

    private TestNumericNode BindNumber(FloatLayerProperty property, float value)
    {
        TestNumericNode node = _nodes.AddNode<TestNumericNode>(property.DataBinding.Script);
        node.Value = value;
        node.Output.ConnectTo(ExitNode(property.DataBinding).Pins.Single());
        property.DataBinding.IsEnabled = true;
        return node;
    }

    private static void AddOpacityKeyframes(Layer layer)
    {
        FloatLayerProperty opacity = layer.Transform.Opacity;
        opacity.KeyframesEnabled = true;
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(0, TimeSpan.Zero, Easings.Functions.Linear, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity));
    }

    private static DataBindingExitNode<T> ExitNode<T>(DataBinding<T> dataBinding)
    {
        return dataBinding.Script.Nodes.OfType<DataBindingExitNode<T>>().Single();
    }
}
