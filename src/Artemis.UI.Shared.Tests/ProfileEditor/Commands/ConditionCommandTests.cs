using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Changing display conditions and their settings
/// </summary>
public class ConditionCommandTests : ProfileEditorTestBase
{
    private readonly Layer _layer;

    public ConditionCommandTests()
    {
        _layer = AddLayer("Layer", null, LedId.Keyboard_A);
    }

    public static TheoryData<string> ConditionTypeCommands => ["type", "mode"];

    // ChangeConditionType and ChangeElementDisplayCondition do the same thing, only their names differ
    private static IProfileEditorCommand ChangeCondition(string command, RenderProfileElement element, ICondition condition)
    {
        return command == "type" ? new ChangeConditionType(element, condition) : new ChangeElementDisplayCondition(element, condition);
    }

    [Fact]
    public void ChangeCondition_DisplayNames()
    {
        new ChangeConditionType(_layer, new TestCondition(_layer)).DisplayName.ShouldBe("Change element display condition type");
        new ChangeElementDisplayCondition(_layer, new TestCondition(_layer)).DisplayName.ShouldBe("Change display condition mode");
    }

    [Theory]
    [MemberData(nameof(ConditionTypeCommands))]
    public void ChangeCondition_ReplacesTheConditionAndUndoPutsTheSameOneBack(string commandType)
    {
        ICondition original = _layer.DisplayCondition;
        string before = Snapshot();
        StaticCondition condition = new(_layer);
        IProfileEditorCommand command = ChangeCondition(commandType, _layer, condition);

        command.Execute();
        _layer.DisplayCondition.ShouldBe(condition);

        command.Undo();
        _layer.DisplayCondition.ShouldBe(original);
        Snapshot().ShouldBe(before);
    }

    [Theory]
    [MemberData(nameof(ConditionTypeCommands))]
    public void ChangeCondition_Dispose_DisposesTheConditionNoLongerInUse(string commandType)
    {
        TestCondition original = new(_layer);
        _layer.DisplayCondition = original;
        TestCondition executedCondition = new(_layer);
        IProfileEditorCommand executed = ChangeCondition(commandType, _layer, executedCondition);
        executed.Execute();

        ((IDisposable) executed).Dispose();

        original.Disposed.ShouldBeTrue();
        executedCondition.Disposed.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(ConditionTypeCommands))]
    public void ChangeCondition_Dispose_AfterUndo_DisposesTheNewCondition(string commandType)
    {
        TestCondition original = new(_layer);
        _layer.DisplayCondition = original;
        TestCondition undoneCondition = new(_layer);
        IProfileEditorCommand undone = ChangeCondition(commandType, _layer, undoneCondition);
        undone.Execute();
        undone.Undo();

        ((IDisposable) undone).Dispose();

        original.Disposed.ShouldBeFalse();
        undoneCondition.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStaticPlayModeAndStopMode_ChangeAndUndoRestores()
    {
        StaticCondition condition = new(_layer);
        _layer.DisplayCondition = condition;
        string before = Snapshot();
        UpdateStaticPlayMode playMode = new(condition, StaticPlayMode.Once);
        UpdateStaticStopMode stopMode = new(condition, StaticStopMode.SkipToEnd);

        playMode.Execute();
        stopMode.Execute();
        playMode.DisplayName.ShouldBe("Update condition play mode");
        stopMode.DisplayName.ShouldBe("Update condition stop mode");
        condition.PlayMode.ShouldBe(StaticPlayMode.Once);
        condition.StopMode.ShouldBe(StaticStopMode.SkipToEnd);

        stopMode.Undo();
        playMode.Undo();
        condition.PlayMode.ShouldBe(StaticPlayMode.Repeat);
        condition.StopMode.ShouldBe(StaticStopMode.Finish);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void UpdateEventModes_ChangeAndUndoRestores()
    {
        EventCondition condition = new(_layer);
        _layer.DisplayCondition = condition;
        EventTriggerMode triggerMode = condition.TriggerMode;
        EventOverlapMode overlapMode = condition.OverlapMode;
        EventToggleOffMode toggleOffMode = condition.ToggleOffMode;
        string before = Snapshot();
        UpdateEventTriggerMode triggerCommand = new(condition, EventTriggerMode.Toggle);
        UpdateEventOverlapMode overlapCommand = new(condition, EventOverlapMode.Copy);
        UpdateEventToggleOffMode toggleOffCommand = new(condition, EventToggleOffMode.SkipToEnd);

        triggerCommand.Execute();
        overlapCommand.Execute();
        toggleOffCommand.Execute();
        triggerCommand.DisplayName.ShouldBe("Update event trigger mode");
        overlapCommand.DisplayName.ShouldBe("Update event overlap mode");
        toggleOffCommand.DisplayName.ShouldBe("Update event toggle off mode");
        condition.TriggerMode.ShouldBe(EventTriggerMode.Toggle);
        condition.OverlapMode.ShouldBe(EventOverlapMode.Copy);
        condition.ToggleOffMode.ShouldBe(EventToggleOffMode.SkipToEnd);

        toggleOffCommand.Undo();
        overlapCommand.Undo();
        triggerCommand.Undo();
        condition.TriggerMode.ShouldBe(triggerMode);
        condition.OverlapMode.ShouldBe(overlapMode);
        condition.ToggleOffMode.ShouldBe(toggleOffMode);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void UpdateEventConditionPath_ChangesThePathAndTheStartNode()
    {
        TestModule module = AddTestModule();
        EventCondition condition = new(_layer);
        _layer.DisplayCondition = condition;
        DataModelPath path = new(module.DataModel, "Integer");
        UpdateEventConditionPath command = new(condition, path);

        command.Execute();

        command.DisplayName.ShouldBe("Update event path");
        condition.EventPath.ShouldBe(path);
        // A regular value gets a start node that fires when the value changes
        condition.GetStartNode().GetType().Name.ShouldBe("EventConditionValueChangedStartNode");
        condition.Script.Nodes.ShouldContain(condition.GetStartNode());
    }

    [Fact]
    public void UpdateEventConditionPath_Undo_RestoresThePreviousPathAndStartNode()
    {
        TestModule module = AddTestModule();
        EventCondition condition = new(_layer);
        _layer.DisplayCondition = condition;
        DataModelPath integerPath = new(module.DataModel, "Integer");
        new UpdateEventConditionPath(condition, integerPath).Execute();
        string before = Snapshot();
        DataModelPath eventPath = new(module.DataModel, "Event");
        UpdateEventConditionPath command = new(condition, eventPath);

        command.Execute();
        condition.GetStartNode().GetType().Name.ShouldBe("EventConditionEventStartNode");

        command.Undo();
        condition.EventPath.ShouldBe(integerPath);
        condition.GetStartNode().GetType().Name.ShouldBe("EventConditionValueChangedStartNode");
        condition.Script.Nodes.Count(n => n.GetType().Name.StartsWith("EventCondition")).ShouldBe(1);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void UpdateEventConditionPath_Undo_ToNoPath_KeepsTheNewStartNode()
    {
        TestModule module = AddTestModule();
        EventCondition condition = new(_layer);
        _layer.DisplayCondition = condition;
        INode originalStartNode = condition.GetStartNode();
        UpdateEventConditionPath command = new(condition, new DataModelPath(module.DataModel, "Integer"));

        command.Execute();
        command.Undo();

        // Pinned as-is: UpdateEventNode does nothing without a path, so undoing back to "no path" keeps the start node
        // made for the path that was undone
        condition.EventPath.ShouldBeNull();
        condition.GetStartNode().ShouldNotBe(originalStartNode);
        condition.GetStartNode().GetType().Name.ShouldBe("EventConditionValueChangedStartNode");
    }

    [Fact]
    public void UpdateEventConditionPath_Dispose_DisposesThePathNoLongerInUse()
    {
        TestModule module = AddTestModule();
        EventCondition condition = new(_layer);
        DataModelPath oldPath = new(module.DataModel, "Integer");
        new UpdateEventConditionPath(condition, oldPath).Execute();
        DataModelPath executedPath = new(module.DataModel, "Float");
        UpdateEventConditionPath executed = new(condition, executedPath);
        executed.Execute();
        DataModelPath undonePath = new(module.DataModel, "Double");
        UpdateEventConditionPath undone = new(condition, undonePath);
        undone.Execute();
        undone.Undo();

        executed.Dispose();
        undone.Dispose();

        IsDisposed(oldPath).ShouldBeTrue();
        IsDisposed(executedPath).ShouldBeFalse();
        IsDisposed(undonePath).ShouldBeTrue();
    }

    private TestModule AddTestModule()
    {
        TestPlugin plugin = Track(new TestPlugin());
        return plugin.AddFeature(new TestModule());
    }

    private static bool IsDisposed(DataModelPath path)
    {
        try
        {
            path.GetValue();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }
}
