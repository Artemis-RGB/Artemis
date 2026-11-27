using Artemis.Core.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;

namespace Artemis.Core.Tests.DataModels;

public class DataModelTests : IDisposable
{
    private readonly TestPlugin _plugin = new();
    private readonly TestModule _module;

    public DataModelTests()
    {
        _module = _plugin.AddFeature(new TestModule());
    }

    private TestDataModel DataModel => _module.DataModel;

    public void Dispose()
    {
        _plugin.Dispose();
    }

    #region Dynamic children

    [Fact]
    public void DynamicChild_CanBeFoundByItsKey()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5);

        DataModel.DynamicChildren.ShouldContainKey("Extra");
        DataModel.GetDynamicChild("Extra").ShouldBeSameAs(child);
        DataModel.GetDynamicChild<int>("Extra").ShouldBeSameAs(child);
        DataModel.TryGetDynamicChild("Extra", out DynamicChild? untyped).ShouldBeTrue();
        untyped.ShouldBeSameAs(child);
        DataModel.TryGetDynamicChild("Extra", out DynamicChild<int>? typed).ShouldBeTrue();
        typed.ShouldBeSameAs(child);
    }

    [Fact]
    public void DynamicChild_KeepsItsKeyTypeAndValue()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5);

        child.Key.ShouldBe("Extra");
        child.Type.ShouldBe(typeof(int));
        child.Value.ShouldBe(5);
        child.BaseValue.ShouldBe(5);

        child.Value = 6;
        child.BaseValue.ShouldBe(6);
    }

    [Fact]
    public void DynamicChild_WithoutAName_IsNamedAfterItsKey()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("currentScore", 5);

        child.Attribute.Name.ShouldBe("Current score");
        child.Attribute.Description.ShouldBeNull();
    }

    [Fact]
    public void DynamicChild_UsesTheProvidedNameAndDescription()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5, "Extra points", "Points scored in overtime");

        child.Attribute.Name.ShouldBe("Extra points");
        child.Attribute.Description.ShouldBe("Points scored in overtime");
    }

    [Fact]
    public void DynamicChild_UsesTheProvidedAttribute()
    {
        DataModelPropertyAttribute attribute = new() {Name = "Extra points", Affix = "pts"};

        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5, attribute);

        child.Attribute.ShouldBeSameAs(attribute);
    }

    [Fact]
    public void DynamicChild_ThatIsADataModel_BelongsToTheSameModule()
    {
        TestChildDataModel childDataModel = new();

        DynamicChild<TestChildDataModel> child = DataModel.AddDynamicChild("Extra", childDataModel, "Extra data");

        childDataModel.Module.ShouldBeSameAs(_module);
        childDataModel.DataModelDescription.ShouldBeSameAs(child.Attribute);
    }

    [Fact]
    public void AddingADynamicChild_RaisesDynamicChildAdded()
    {
        List<DynamicDataModelChildEventArgs> added = [];
        DataModel.DynamicChildAdded += (_, e) => added.Add(e);

        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5);

        added.ShouldHaveSingleItem();
        added[0].Key.ShouldBe("Extra");
        added[0].DynamicChild.ShouldBeSameAs(child);
    }

    [Fact]
    public void DynamicChild_KeyCannotContainADot()
    {
        Should.Throw<ArtemisCoreException>(() => DataModel.AddDynamicChild("Extra.Points", 5));
    }

    [Fact]
    public void DynamicChild_KeyCannotBeUsedTwice()
    {
        DataModel.AddDynamicChild("Extra", 5);

        Should.Throw<ArtemisCoreException>(() => DataModel.AddDynamicChild("Extra", 6));
    }

    [Fact]
    public void DynamicChild_KeyCannotBeTheNameOfARegularProperty()
    {
        Should.Throw<ArtemisCoreException>(() => DataModel.AddDynamicChild("Integer", 5));
    }

    [Fact]
    public void DynamicChild_CannotStartOutEmpty()
    {
        Should.Throw<ArgumentNullException>(() => DataModel.AddDynamicChild<string?>("Extra", null));
    }

    [Fact]
    public void GettingAMissingDynamicChild_Throws()
    {
        Should.Throw<KeyNotFoundException>(() => DataModel.GetDynamicChild("Missing"));
        Should.Throw<KeyNotFoundException>(() => DataModel.GetDynamicChild<int>("Missing"));
    }

    [Fact]
    public void GettingADynamicChildAsTheWrongType_Throws()
    {
        DataModel.AddDynamicChild("Extra", 5);

        Should.Throw<InvalidCastException>(() => DataModel.GetDynamicChild<string>("Extra"));
    }

    [Fact]
    public void TryingToGetAMissingOrWronglyTypedDynamicChild_ReturnsFalse()
    {
        DataModel.AddDynamicChild("Extra", 5);

        DataModel.TryGetDynamicChild("Missing", out DynamicChild? missing).ShouldBeFalse();
        missing.ShouldBeNull();
        DataModel.TryGetDynamicChild("Extra", out DynamicChild<string>? wrongType).ShouldBeFalse();
        wrongType.ShouldBeNull();
    }

    [Fact]
    public void RemovingADynamicChildByKey_RemovesItAndRaisesDynamicChildRemoved()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5);
        List<DynamicDataModelChildEventArgs> removed = [];
        DataModel.DynamicChildRemoved += (_, e) => removed.Add(e);

        DataModel.RemoveDynamicChildByKey("Extra");

        DataModel.DynamicChildren.ShouldBeEmpty();
        removed.ShouldHaveSingleItem();
        removed[0].Key.ShouldBe("Extra");
        removed[0].DynamicChild.ShouldBeSameAs(child);
    }

    [Fact]
    public void RemovingAMissingDynamicChildByKey_DoesNothing()
    {
        DataModel.AddDynamicChild("Extra", 5);
        int removed = 0;
        DataModel.DynamicChildRemoved += (_, _) => removed++;

        DataModel.RemoveDynamicChildByKey("Missing");

        DataModel.DynamicChildren.Count.ShouldBe(1);
        removed.ShouldBe(0);
    }

    [Fact]
    public void RemovingADynamicChild_RemovesIt()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5);
        int removed = 0;
        DataModel.DynamicChildRemoved += (_, _) => removed++;

        DataModel.RemoveDynamicChild(child);

        DataModel.DynamicChildren.ShouldNotContainKey("Extra");
        removed.ShouldBe(1);
    }

    [Fact]
    public void ClearingDynamicChildren_RemovesAllOfThemOneByOne()
    {
        DataModel.AddDynamicChild("First", 1);
        DataModel.AddDynamicChild("Second", "two");
        List<string> removed = [];
        DataModel.DynamicChildRemoved += (_, e) => removed.Add(e.Key);

        DataModel.ClearDynamicChildren();

        DataModel.DynamicChildren.ShouldBeEmpty();
        removed.ShouldBe(["First", "Second"], ignoreOrder: true);
    }

    #endregion

    #region Hidden properties

    [Fact]
    public void HiddenProperties_AreEmptyByDefault()
    {
        DataModel.GetHiddenProperties().ShouldBeEmpty();
    }

    [Fact]
    public void HidingAProperty_AddsItToTheHiddenPropertiesOnce()
    {
        _module.HideProperty(dm => dm.Child.Integer);
        _module.HideProperty(dm => dm.Child.Integer);

        DataModel.GetHiddenProperties().ShouldHaveSingleItem().ShouldBe(typeof(TestChildDataModel).GetProperty(nameof(TestChildDataModel.Integer)));
    }

    [Fact]
    public void ShowingAProperty_RemovesItFromTheHiddenProperties()
    {
        _module.HideProperty(dm => dm.Integer);
        _module.HideProperty(dm => dm.Text);

        _module.ShowProperty(dm => dm.Integer);

        DataModel.GetHiddenProperties().ShouldHaveSingleItem().Name.ShouldBe(nameof(TestDataModel.Text));
    }

    #endregion

    #region Property descriptions

    [Fact]
    public void PropertyWithoutADescription_HasNoPropertyDescription()
    {
        DataModel.GetPropertyDescription(typeof(TestDataModel).GetProperty(nameof(TestDataModel.Integer))!).ShouldBeNull();
    }

    [Fact]
    public void DataModel_IsDescribedByItsModule()
    {
        DataModel.Module.ShouldBeSameAs(_module);
        DataModel.DataModelDescription.ShouldNotBeNull();
    }

    #endregion

    #region Paths in use

    [Fact]
    public void Property_IsNotInUseWithoutPaths()
    {
        _module.IsPropertyInUse("Integer", false).ShouldBeFalse();
        _module.IsPropertyInUse("Integer", true).ShouldBeFalse();
    }

    [Fact]
    public void Property_IsInUseWhileAPathPointsToIt()
    {
        DataModelPath path = new(DataModel, "Child.Integer");

        _module.IsPropertyInUse("Child.Integer", false).ShouldBeTrue();
        _module.IsPropertyInUse(dm => dm.Child.Integer, false).ShouldBeTrue();
        _module.IsPropertyInUse("Child.Text", false).ShouldBeFalse();

        path.Dispose();

        _module.IsPropertyInUse("Child.Integer", false).ShouldBeFalse();
    }

    [Fact]
    public void Property_IsInUseIgnoringCase()
    {
        using DataModelPath path = new(DataModel, "Child.Integer");

        _module.IsPropertyInUse("child.integer", false).ShouldBeTrue();
    }

    [Fact]
    public void ParentProperty_IsOnlyInUseWhenIncludingChildren()
    {
        using DataModelPath path = new(DataModel, "Child.Integer");

        _module.IsPropertyInUse("Child", false).ShouldBeFalse();
        _module.IsPropertyInUse("Child", true).ShouldBeTrue();
        _module.IsPropertyInUse(dm => dm.Child, true).ShouldBeTrue();
    }

    [Fact]
    public void IncludingChildren_DoesNotMatchPropertiesThatStartWithTheSameName()
    {
        using DataModelPath path = new(DataModel, "Integers");

        // Integers is a different property than Integer, not a child of it
        _module.IsPropertyInUse("Integer", true).ShouldBeFalse();
        _module.IsPropertyInUse("Integer", false).ShouldBeFalse();
        _module.IsPropertyInUse("Integers", true).ShouldBeTrue();
    }

    [Fact]
    public void Property_StaysInUseUntilTheLastPathToItIsGone()
    {
        DataModelPath first = new(DataModel, "Integer");
        DataModelPath second = new(DataModel, "Integer");

        first.Dispose();
        _module.IsPropertyInUse("Integer", false).ShouldBeTrue();

        second.Dispose();
        _module.IsPropertyInUse("Integer", false).ShouldBeFalse();
    }

    [Fact]
    public void ActivePaths_ListThePathsTargetingTheDataModel()
    {
        List<DataModelPath> added = [];
        List<DataModelPath> removed = [];
        DataModel.ActivePathAdded += (_, e) => added.Add(e.DataModelPath);
        DataModel.ActivePathRemoved += (_, e) => removed.Add(e.DataModelPath);

        DataModelPath path = new(DataModel, "Integer");

        DataModel.ActivePaths.ShouldHaveSingleItem().ShouldBeSameAs(path);
        added.ShouldHaveSingleItem().ShouldBeSameAs(path);

        path.Dispose();

        DataModel.ActivePaths.ShouldBeEmpty();
        removed.ShouldHaveSingleItem().ShouldBeSameAs(path);
    }

    [Fact]
    public void InvalidPath_IsStillAnActivePath()
    {
        using DataModelPath path = new(DataModel, "DoesNotExist");

        DataModel.ActivePaths.ShouldContain(path);
        _module.IsPropertyInUse("DoesNotExist", false).ShouldBeTrue();
    }

    #endregion
}
