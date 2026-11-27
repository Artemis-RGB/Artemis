using Artemis.Core.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Tests.DataModels;

public class DataModelPathTests : IDisposable
{
    private readonly TestPlugin _plugin = new();
    private readonly TestModule _module;

    public DataModelPathTests()
    {
        _module = _plugin.AddFeature(new TestModule());
    }

    private TestDataModel DataModel => _module.DataModel;

    [Fact]
    public void Path_ReadsTheCurrentValue()
    {
        _module.DataModel.Integer = 5;
        using DataModelPath path = new(_module.DataModel, "Integer");

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(5);

        _module.DataModel.Integer = 6;
        path.GetValue().ShouldBe(6);
    }

    [Fact]
    public void Path_IsRestoredFromItsEntityThroughTheDataModelStore()
    {
        _module.DataModel.Child.Integer = 7;
        using DataModelPath path = new(_module.DataModel, "Child.Integer");
        path.Save();

        using DataModelPath restored = new(path.Entity);

        restored.IsValid.ShouldBeTrue();
        restored.Target.ShouldBe(_module.DataModel);
        restored.GetValue().ShouldBe(7);
    }

    public void Dispose()
    {
        _plugin.Dispose();
    }

    #region Reading values

    [Fact]
    public void Path_ReadsEveryKindOfProperty()
    {
        DataModel.Float = 1.5f;
        DataModel.Double = 2.5;
        DataModel.Boolean = true;
        DataModel.Text = "Hello";
        DataModel.Color = SKColors.Lime;
        DataModel.Enum = TestEnum.Third;

        ValueAt("Float").ShouldBe(1.5f);
        ValueAt("Double").ShouldBe(2.5);
        ValueAt("Boolean").ShouldBe(true);
        ValueAt("Text").ShouldBe("Hello");
        ValueAt("Color").ShouldBe(SKColors.Lime);
        ValueAt("Enum").ShouldBe(TestEnum.Third);
    }

    [Fact]
    public void EmptyPath_PointsToTheDataModelItself()
    {
        using DataModelPath path = new(DataModel);

        path.IsValid.ShouldBeTrue();
        path.Path.ShouldBe("");
        path.ToString().ShouldBe("this");
        path.GetValue().ShouldBeSameAs(DataModel);
        path.GetPropertyType().ShouldBe(typeof(TestDataModel));
        path.GetPropertyInfo().ShouldBeNull();
        path.Segments.ShouldHaveSingleItem().IsStartSegment.ShouldBeTrue();
    }

    [Fact]
    public void Path_ReadsANestedDataModel()
    {
        DataModel.Child.Text = "Nested";

        ValueAt("Child.Text").ShouldBe("Nested");
        ValueAt("Child").ShouldBeSameAs(DataModel.Child);
    }

    [Fact]
    public void Path_ReadsAPlainObject()
    {
        DataModel.Item = new TestItem {Integer = 3};

        ValueAt("Item.Integer").ShouldBe(3);
    }

    [Fact]
    public void Path_ThroughAnEmptyPlainObject_ReadsTheDefaultValue()
    {
        DataModel.Item = null!;
        using DataModelPath integerPath = new(DataModel, "Item.Integer");
        using DataModelPath textPath = new(DataModel, "Item.Text");

        integerPath.IsValid.ShouldBeTrue();
        integerPath.GetValue().ShouldBe(0);
        textPath.IsValid.ShouldBeTrue();
        textPath.GetValue().ShouldBeNull();
    }

    [Fact]
    public void Path_ReadsAList()
    {
        DataModel.Integers = [1, 2, 3];

        ValueAt("Integers").ShouldBe(new List<int> {1, 2, 3});
        ValueAt("Integers.Count").ShouldBe(3);
    }

    [Fact]
    public void Path_CannotPointToAListItem()
    {
        DataModel.Integers = [1, 2, 3];
        using DataModelPath path = new(DataModel, "Integers.0");

        path.IsValid.ShouldBeFalse();
        path.GetValue().ShouldBeNull();
    }

    [Fact]
    public void Path_CanPointToAnIgnoredProperty()
    {
        // The ignore attribute only hides the property from the editor, paths still work
        DataModel.Ignored = 4;

        ValueAt("Ignored").ShouldBe(4);
    }

    [Fact]
    public void Path_ReadsTheArgumentsOfAnEvent()
    {
        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 9});

        ValueAt("EventWithArgs.LastEventArguments.Value").ShouldBe(9);
        ValueAt("EventWithArgs.TriggerCount").ShouldBe(1);
    }

    [Fact]
    public void Path_IsCaseSensitive()
    {
        using DataModelPath path = new(DataModel, "integer");

        path.IsValid.ShouldBeFalse();
    }

    #endregion

    #region Invalid paths

    [Fact]
    public void PathToAMissingProperty_IsInvalid()
    {
        using DataModelPath path = new(DataModel, "DoesNotExist");

        path.IsValid.ShouldBeFalse();
        path.GetValue().ShouldBeNull();
        path.GetPropertyType().ShouldBeNull();
        path.GetPropertyInfo().ShouldBeNull();
    }

    [Fact]
    public void PathThroughAMissingProperty_StopsAtTheFirstInvalidSegment()
    {
        using DataModelPath path = new(DataModel, "DoesNotExist.Integer");

        path.Segments.Select(s => s.Type).ShouldBe([DataModelPathSegmentType.Static, DataModelPathSegmentType.Invalid, DataModelPathSegmentType.Invalid]);
        path.Segments.Last().GetValue().ShouldBeNull();
    }

    [Fact]
    public void Path_RequiresATarget()
    {
        Should.Throw<ArgumentNullException>(() => new DataModelPath((Artemis.Core.Modules.DataModel) null!));
        Should.Throw<ArgumentNullException>(() => new DataModelPath(null!, "Integer"));
        Should.Throw<ArgumentNullException>(() => new DataModelPath(DataModel, null!));
        Should.Throw<ArgumentNullException>(() => new DataModelPath((DataModelPath) null!));
    }

    #endregion

    #region Segments

    [Fact]
    public void Segments_DescribeEachStepOfThePath()
    {
        DataModel.Child.Integer = 8;
        using DataModelPath path = new(DataModel, "Child.Integer");

        DataModelPathSegment[] segments = path.Segments.ToArray();
        segments.Length.ShouldBe(3);

        segments[0].IsStartSegment.ShouldBeTrue();
        segments[0].Identifier.ShouldBe("target");
        segments[0].Previous.ShouldBeNull();
        segments[0].Next.ShouldBeSameAs(segments[1]);
        segments[0].GetValue().ShouldBeSameAs(DataModel);

        segments[1].Identifier.ShouldBe("Child");
        segments[1].Path.ShouldBe("Child");
        segments[1].GetValue().ShouldBeSameAs(DataModel.Child);

        segments[2].Identifier.ShouldBe("Integer");
        segments[2].Path.ShouldBe("Child.Integer");
        segments[2].IsStartSegment.ShouldBeFalse();
        segments[2].Previous.ShouldBeSameAs(segments[1]);
        segments[2].Next.ShouldBeNull();
        segments[2].Type.ShouldBe(DataModelPathSegmentType.Static);
        segments[2].GetValue().ShouldBe(8);
        segments[2].ToString().ShouldBe("[Static] Child.Integer");
    }

    [Fact]
    public void StaticSegment_KnowsItsProperty()
    {
        using DataModelPath path = new(DataModel, "Child.Integer");

        path.GetPropertyInfo().ShouldBe(typeof(TestChildDataModel).GetProperty(nameof(TestChildDataModel.Integer)));
        path.GetPropertyType().ShouldBe(typeof(int));
    }

    [Fact]
    public void StaticSegment_WithoutADescription_IsNamedAfterItsProperty()
    {
        using DataModelPath path = new(DataModel, "Child.Integer");

        DataModelPropertyAttribute description = path.GetPropertyDescription().ShouldNotBeNull();
        description.Name.ShouldBe("Integer");
        description.ResetsDepth.ShouldBeFalse();
    }

    [Fact]
    public void StaticSegment_UsesTheDescriptionOfItsProperty()
    {
        using DataModelPath path = new(DataModel, "Event.TriggerCount");

        path.GetPropertyDescription().ShouldNotBeNull().Description.ShouldBe("The total amount of times this event has triggered since the module was activated");
    }

    [Fact]
    public void StartSegment_UsesTheDescriptionOfTheDataModel()
    {
        using DataModelPath path = new(DataModel);

        path.GetPropertyDescription().ShouldBeSameAs(DataModel.DataModelDescription);
    }

    #endregion

    #region Dynamic children

    [Fact]
    public void Path_ReadsADynamicChild()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5, "Extra points");
        using DataModelPath path = new(DataModel, "Extra");

        path.IsValid.ShouldBeTrue();
        path.Segments.Last().Type.ShouldBe(DataModelPathSegmentType.Dynamic);
        path.GetValue().ShouldBe(5);

        child.Value = 6;
        path.GetValue().ShouldBe(6);
    }

    [Fact]
    public void DynamicSegment_TakesItsTypeFromTheValueAndItsDescriptionFromTheChild()
    {
        DynamicChild<int> child = DataModel.AddDynamicChild("Extra", 5, "Extra points");
        using DataModelPath path = new(DataModel, "Extra");

        path.GetPropertyInfo().ShouldBeNull();
        path.GetPropertyType().ShouldBe(typeof(int));
        path.GetPropertyDescription().ShouldBeSameAs(child.Attribute);
    }

    [Fact]
    public void Path_ReadsThroughADynamicDataModel()
    {
        TestChildDataModel childDataModel = new() {Integer = 4};
        DataModel.AddDynamicChild("Extra", childDataModel, "Extra data");

        using DataModelPath path = new(DataModel, "Extra.Integer");

        path.IsValid.ShouldBeTrue();
        path.Segments.Select(s => s.Type).ShouldBe([DataModelPathSegmentType.Static, DataModelPathSegmentType.Dynamic, DataModelPathSegmentType.Static]);
        path.GetValue().ShouldBe(4);
    }

    [Fact]
    public void Path_ReadsADynamicChildOfANestedDataModel()
    {
        DataModel.Child.AddDynamicChild("Extra", "Deep");

        ValueAt("Child.Extra").ShouldBe("Deep");
    }

    [Fact]
    public void PathToAMissingDynamicChild_BecomesValidOnceTheChildIsAdded()
    {
        using DataModelPath path = new(DataModel, "Extra");
        int validated = 0;
        path.PathValidated += (_, _) => validated++;

        path.IsValid.ShouldBeFalse();

        DataModel.AddDynamicChild("Extra", 5);

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(5);
        validated.ShouldBe(1);
    }

    [Fact]
    public void AddingAnotherDynamicChild_DoesNotAffectThePath()
    {
        using DataModelPath path = new(DataModel, "Extra");
        int validated = 0;
        path.PathValidated += (_, _) => validated++;

        DataModel.AddDynamicChild("Other", 5);

        path.IsValid.ShouldBeFalse();
        validated.ShouldBe(0);
    }

    [Fact]
    public void RemovingTheDynamicChild_MakesThePathInvalid()
    {
        DataModel.AddDynamicChild("Extra", 5);
        using DataModelPath path = new(DataModel, "Extra");
        int invalidated = 0;
        path.PathInvalidated += (_, _) => invalidated++;

        DataModel.RemoveDynamicChildByKey("Extra");

        path.IsValid.ShouldBeFalse();
        path.GetValue().ShouldBeNull();
        invalidated.ShouldBe(1);
    }

    [Fact]
    public void RemovingTheDynamicChild_AndAddingItAgain_MakesThePathValidAgain()
    {
        DataModel.AddDynamicChild("Extra", 5);
        using DataModelPath path = new(DataModel, "Extra");
        DataModel.RemoveDynamicChildByKey("Extra");

        DataModel.AddDynamicChild("Extra", 6);

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(6);
    }

    [Fact]
    public void RemovingADynamicDataModel_MakesThePathThroughItInvalid()
    {
        DataModel.AddDynamicChild("Extra", new TestChildDataModel {Integer = 4}, "Extra data");
        using DataModelPath path = new(DataModel, "Extra.Integer");

        DataModel.RemoveDynamicChildByKey("Extra");

        path.IsValid.ShouldBeFalse();
        path.GetValue().ShouldBeNull();
    }

    [Fact]
    public void ReplacingADynamicChild_MakesThePathReadTheNewChild()
    {
        DataModel.AddDynamicChild("Extra", 5);
        using DataModelPath path = new(DataModel, "Extra");

        DataModel.RemoveDynamicChildByKey("Extra");
        DataModel.AddDynamicChild("Extra", 7);

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(7);
    }

    #endregion

    #region The data model store

    [Fact]
    public void SavedPath_BecomesInvalidWhenItsDataModelIsRemovedFromTheStore()
    {
        using DataModelPath path = new(DataModel, "Integer");
        path.Save();
        int invalidated = 0;
        path.PathInvalidated += (_, _) => invalidated++;

        RemoveFromStore();

        path.IsValid.ShouldBeFalse();
        path.Target.ShouldBeNull();
        path.GetValue().ShouldBeNull();
        invalidated.ShouldBe(1);
        DataModel.ActivePaths.ShouldNotContain(path);
    }

    [Fact]
    public void SavedPath_BecomesInvalidWhenItsModuleIsDisabled()
    {
        using DataModelPath path = new(DataModel, "Integer");
        path.Save();

        _plugin.Dispose();

        path.IsValid.ShouldBeFalse();
        path.Target.ShouldBeNull();
    }

    [Fact]
    public void SavedPath_BecomesValidAgainWhenItsDataModelReturns()
    {
        DataModel.Integer = 3;
        using DataModelPath path = new(DataModel, "Integer");
        path.Save();
        int validated = 0;
        path.PathValidated += (_, _) => validated++;

        RemoveFromStore();
        DataModelStore.Add(DataModel);

        path.IsValid.ShouldBeTrue();
        path.Target.ShouldBeSameAs(DataModel);
        path.GetValue().ShouldBe(3);
        validated.ShouldBe(1);
    }

    [Fact]
    public void SavedPath_MovesToANewInstanceOfItsDataModel()
    {
        using DataModelPath path = new(DataModel, "Integer");
        path.Save();

        // Like a module being enabled again, which creates a new data model
        RemoveFromStore();
        TestDataModel newDataModel = new() {Module = _module, Integer = 11};
        DataModelStore.Add(newDataModel);

        path.Target.ShouldBeSameAs(newDataModel);
        path.GetValue().ShouldBe(11);
    }

    [Fact]
    public void NewPath_FollowsTheStoreWithoutBeingSaved()
    {
        using DataModelPath path = new(DataModel, "Integer");

        path.Entity.DataModelId.ShouldBe(_module.Id);
        path.Entity.Path.ShouldBe("Integer");

        RemoveFromStore();

        path.IsValid.ShouldBeFalse();
        path.Target.ShouldBeNull();
    }

    [Fact]
    public void PathFromAnEntity_WaitsForItsDataModelToBeAdded()
    {
        RemoveFromStore();
        DataModel.Integer = 12;
        DataModelPathEntity entity = new() {Path = "Integer", DataModelId = _module.Id};

        using DataModelPath path = new(entity);

        path.IsValid.ShouldBeFalse();
        path.Target.ShouldBeNull();
        path.GetValue().ShouldBeNull();
        path.GetFeatureDependencies().ShouldBeEmpty();

        DataModelStore.Add(DataModel);

        path.IsValid.ShouldBeTrue();
        path.GetValue().ShouldBe(12);
    }

    [Fact]
    public void PathFromAnEntity_IgnoresOtherDataModels()
    {
        DataModelPathEntity entity = new() {Path = "Integer", DataModelId = "Some.Other.Module-12345678"};
        using DataModelPath path = new(entity);

        RemoveFromStore();
        DataModelStore.Add(DataModel);

        path.IsValid.ShouldBeFalse();
        path.Target.ShouldBeNull();
    }

    #endregion

    #region Saving and loading

    [Fact]
    public void Saving_StoresThePathItsDataModelAndItsType()
    {
        using DataModelPath path = new(DataModel, "Child.Integer");

        path.Save();

        path.Entity.Path.ShouldBe("Child.Integer");
        path.Entity.DataModelId.ShouldBe(_module.Id);
        path.Entity.Type.ShouldBe(typeof(int).FullName);
        path.DataModelId.ShouldBe(_module.Id);
    }

    [Fact]
    public void SavingAnInvalidPath_KeepsWhatWasSavedBefore()
    {
        DataModelPathEntity entity = new() {Path = "DoesNotExist", DataModelId = _module.Id, Type = "System.Int32"};
        using DataModelPath path = new(entity);

        path.Save();

        path.IsValid.ShouldBeFalse();
        entity.Path.ShouldBe("DoesNotExist");
        entity.DataModelId.ShouldBe(_module.Id);
        entity.Type.ShouldBe("System.Int32");
    }

    [Fact]
    public void InvalidPath_FallsBackToTheSavedType()
    {
        using DataModelPath path = new(new DataModelPathEntity {Path = "Integer", DataModelId = "Missing-12345678", Type = typeof(int).FullName});

        path.IsValid.ShouldBeFalse();
        path.GetPropertyType().ShouldBe(typeof(int));
    }

    [Fact]
    public void InvalidPath_FallsBackToATypeOutsideTheCoreLibraries()
    {
        using DataModelPath valid = new(DataModel, "Enum");
        valid.Save();
        valid.Entity.Type.ShouldBe(typeof(TestEnum).FullName);

        using DataModelPath path = new(new DataModelPathEntity {Path = "Enum", DataModelId = "Missing-12345678", Type = valid.Entity.Type});

        path.GetPropertyType().ShouldBe(typeof(TestEnum));
    }

    [Fact]
    public void InvalidPath_FallsBackToASkiaSharpType()
    {
        using DataModelPath path = new(new DataModelPathEntity {Path = "Color", DataModelId = "Missing-12345678", Type = typeof(SKColor).FullName});

        path.GetPropertyType().ShouldBe(typeof(SKColor));
    }

    [Fact]
    public void CopiedPath_PointsToTheSameProperty()
    {
        DataModel.Integer = 13;
        using DataModelPath original = new(DataModel, "Integer");

        using DataModelPath copy = new(original);

        copy.Target.ShouldBeSameAs(DataModel);
        copy.Path.ShouldBe("Integer");
        copy.GetValue().ShouldBe(13);
        copy.Entity.ShouldNotBeSameAs(original.Entity);
        DataModel.ActivePaths.Count.ShouldBe(2);
    }

    #endregion

    #region Disposing

    [Fact]
    public void DisposedPath_IsInvalidAndCannotBeRead()
    {
        DataModelPath path = new(DataModel, "Integer");
        int invalidated = 0;
        path.PathInvalidated += (_, _) => invalidated++;

        path.Dispose();

        invalidated.ShouldBe(1);
        path.IsValid.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(() => path.GetValue());
        Should.Throw<ObjectDisposedException>(() => path.GetPropertyType());
        Should.Throw<ObjectDisposedException>(() => path.GetPropertyInfo());
        Should.Throw<ObjectDisposedException>(() => path.GetPropertyDescription());
    }

    [Fact]
    public void DisposedPath_NoLongerFollowsTheStore()
    {
        DataModelPath path = new(DataModel, "Integer");
        path.Save();
        path.Dispose();
        int validated = 0;
        path.PathValidated += (_, _) => validated++;

        RemoveFromStore();
        DataModelStore.Add(DataModel);

        validated.ShouldBe(0);
        path.IsValid.ShouldBeFalse();
    }

    #endregion

    [Fact]
    public void Path_DependsOnTheModuleOfItsDataModel()
    {
        using DataModelPath path = new(DataModel, "Integer");

        path.GetFeatureDependencies().ShouldBe([_module]);
    }

    private object? ValueAt(string path)
    {
        using DataModelPath dataModelPath = new(DataModel, path);
        dataModelPath.IsValid.ShouldBeTrue();
        return dataModelPath.GetValue();
    }

    private void RemoveFromStore()
    {
        DataModelStore.Remove(DataModelStore.Get(_module.Id).ShouldNotBeNull());
    }
}
