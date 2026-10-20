using Artemis.Core.Internal;
using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Entities.Profile.Conditions;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Conditions;

/// <summary>
///     Event conditions on a layer on the A key, with a main segment of one second and no start or end segment
/// </summary>
public class EventConditionTests : RenderTestBase
{
    private readonly TestModule _module;
    private readonly DataModelTestNodeProvider _nodes;

    public EventConditionTests()
    {
        TestPlugin plugin = Track(new TestPlugin());
        _module = plugin.AddFeature(new TestModule());
        _nodes = plugin.AddFeature(new DataModelTestNodeProvider());
    }

    private TestDataModel DataModel => _module.DataModel;

    #region Play

    [Fact]
    public void Layer_StaysOffUntilTheEventTriggers()
    {
        (Layer layer, _) = AddLayer();

        using RenderFrame frame = Render(layer.Profile);

        layer.DisplayConditionMet.ShouldBeFalse();
        ShouldBeOff(frame);
    }

    [Fact]
    public void Play_PlaysTheTimelineOnceWhenTheEventTriggers()
    {
        (Layer layer, EventCondition condition) = AddLayer();

        DataModel.Event.Trigger();
        using RenderFrame triggered = Render(layer.Profile);
        bool metWhenTriggered = condition.IsMet;
        using RenderFrame playing = Render(layer.Profile, 0.5);
        bool metWhilePlaying = condition.IsMet;
        using RenderFrame finished = Render(layer.Profile, 0.5);

        // The condition is only met in the frame the event triggered, the timeline then plays to the end
        metWhenTriggered.ShouldBeTrue();
        metWhilePlaying.ShouldBeFalse();
        ShouldBeOn(triggered);
        ShouldBeOn(playing);
        ShouldBeOff(finished);
        layer.Timeline.IsFinished.ShouldBeTrue();
    }

    [Fact]
    public void Play_TriggeringTwiceBetweenFrames_PlaysOnce()
    {
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Restart);
        DataModel.Event.Trigger();
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.5))
        {
        }

        using RenderFrame frame = Render(layer.Profile, 0.6);

        ShouldBeOff(frame);
    }

    [Fact]
    public void Play_EventTriggeredBeforeTheConditionExisted_DoesNotPlayTheLayer()
    {
        DataModel.Event.Trigger();
        (Layer layer, _) = AddLayer();

        using RenderFrame frame = Render(layer.Profile);

        ShouldBeOff(frame);
    }

    [Fact]
    public void Play_EventTriggeredAfterTheConditionExisted_PlaysTheLayer_EvenBeforeTheFirstFrame()
    {
        (Layer layer, _) = AddLayer();
        DataModel.Event.Trigger();

        using RenderFrame frame = Render(layer.Profile);

        ShouldBeOn(frame);
    }

    [Fact]
    public void Play_Restart_StartsOverWhenTheEventTriggersAgain()
    {
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Restart);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.6))
        {
        }

        DataModel.Event.Trigger();
        using (Render(layer.Profile))
        {
        }

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.1));
        using RenderFrame frame = Render(layer.Profile, 0.5);
        ShouldBeOn(frame);
    }

    [Fact]
    public void Play_Ignore_KeepsPlayingWhenTheEventTriggersAgain()
    {
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Ignore);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.6))
        {
        }

        DataModel.Event.Trigger();
        using (Render(layer.Profile))
        {
        }

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.7));
        using RenderFrame frame = Render(layer.Profile, 0.4);
        ShouldBeOff(frame);
    }

    [Fact]
    public void Play_Ignore_PlaysAgainOnceTheTimelineFinished()
    {
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Ignore);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 1.5))
        {
        }

        DataModel.Event.Trigger();
        using RenderFrame frame = Render(layer.Profile);

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.1));
        ShouldBeOn(frame);
    }

    [Fact]
    public void Play_Copy_PlaysACopyOnTopWhenTheEventTriggersAgain()
    {
        // The color goes from red to blue over the second, so the color tells how far along a timeline is
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Copy, fadeToBlue: true);
        // Copies are created from what was last saved
        Harness.Save(layer.Profile);
        DataModel.Event.Trigger();
        using RenderFrame first = Render(layer.Profile, 0.8);

        DataModel.Event.Trigger();
        using RenderFrame copied = Render(layer.Profile);
        TimeSpan positionWhenCopied = layer.Timeline.Position;
        using RenderFrame originalFinished = Render(layer.Profile, 0.2);
        using RenderFrame copyFinished = Render(layer.Profile, 0.8);

        first.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(ColorAt(0.8));
        // The original is at 0.9 seconds, the copy is drawn on top at 0.1 seconds
        positionWhenCopied.ShouldBe(TimeSpan.FromSeconds(0.9));
        copied.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(ColorAt(0.1));
        originalFinished.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(ColorAt(0.3));
        ShouldBeOff(copyFinished);
    }

    [Fact]
    public void Play_Copy_KeepsTheLayerEnabledWhileACopyPlays()
    {
        (Layer layer, _) = AddLayer(overlapMode: EventOverlapMode.Copy);
        Harness.Save(layer.Profile);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.8))
        {
        }

        DataModel.Event.Trigger();
        using (Render(layer.Profile))
        {
        }

        using (Render(layer.Profile, 0.5))
        {
        }

        layer.Timeline.IsFinished.ShouldBeTrue();
        layer.Enabled.ShouldBeTrue();

        // Once the copy finished it is removed on the next frame and the layer is disabled the frame after
        using (Render(layer.Profile, 0.5))
        {
        }

        using (Render(layer.Profile))
        {
        }

        using (Render(layer.Profile))
        {
        }

        layer.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void Play_Copy_OnAFolder_KeepsPlaying()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        UseEventCondition(folder, "Event", overlapMode: EventOverlapMode.Copy);
        DataModel.Event.Trigger();
        using (Render(profile, 0.6))
        {
        }

        DataModel.Event.Trigger();
        using (Render(profile))
        {
        }

        // Only layers can be copied, a folder neither copies nor restarts
        folder.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.7));
    }

    #endregion

    #region Toggle

    [Fact]
    public void Toggle_KeepsRepeatingUntilTheEventTriggersAgain()
    {
        (Layer layer, EventCondition condition) = AddLayer(EventTriggerMode.Toggle);

        DataModel.Event.Trigger();
        using RenderFrame on = Render(layer.Profile);
        using RenderFrame stillOn = Render(layer.Profile, 1.5);

        condition.IsMet.ShouldBeTrue();
        ShouldBeOn(on);
        ShouldBeOn(stillOn);
        // Wrapped around the main segment rather than finishing
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.5));
    }

    [Fact]
    public void Toggle_Finish_FinishesTheCurrentRunWhenToggledOff()
    {
        (Layer layer, EventCondition condition) = AddLayer(EventTriggerMode.Toggle, toggleOffMode: EventToggleOffMode.Finish);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.5))
        {
        }

        DataModel.Event.Trigger();
        using RenderFrame toggledOff = Render(layer.Profile);
        using RenderFrame finished = Render(layer.Profile, 0.5);

        condition.IsMet.ShouldBeFalse();
        ShouldBeOn(toggledOff);
        ShouldBeOff(finished);
    }

    [Fact]
    public void Toggle_SkipToEnd_StopsRightAwayWhenToggledOff()
    {
        (Layer layer, _) = AddLayer(EventTriggerMode.Toggle, toggleOffMode: EventToggleOffMode.SkipToEnd);
        DataModel.Event.Trigger();
        using (Render(layer.Profile, 0.5))
        {
        }

        DataModel.Event.Trigger();
        using RenderFrame toggledOff = Render(layer.Profile);

        ShouldBeOff(toggledOff);
        layer.Timeline.IsFinished.ShouldBeTrue();
    }

    [Fact]
    public void Toggle_TriggeringTwiceBetweenFrames_TogglesOnce()
    {
        (Layer layer, EventCondition condition) = AddLayer(EventTriggerMode.Toggle);
        DataModel.Event.Trigger();
        DataModel.Event.Trigger();

        using RenderFrame frame = Render(layer.Profile);

        condition.IsMet.ShouldBeTrue();
        ShouldBeOn(frame);
    }

    [Fact]
    public void Toggle_TogglesBackOnAfterBeingToggledOff()
    {
        (Layer layer, EventCondition condition) = AddLayer(EventTriggerMode.Toggle);
        DataModel.Event.Trigger();
        using (Render(layer.Profile))
        {
        }

        DataModel.Event.Trigger();
        using (Render(layer.Profile, 1.5))
        {
        }

        DataModel.Event.Trigger();
        using RenderFrame frame = Render(layer.Profile);

        condition.IsMet.ShouldBeTrue();
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.1));
        ShouldBeOn(frame);
    }

    #endregion

    #region Event arguments

    [Fact]
    public void StartNode_OffersTheEventArguments()
    {
        (_, EventCondition condition) = AddLayer(eventPath: "EventWithArgs");

        INode startNode = condition.GetStartNode();

        startNode.ShouldBeOfType<EventConditionEventStartNode>();
        IPin value = startNode.Pins.ShouldHaveSingleItem();
        value.Name.ShouldBe("Value");
        value.Type.ShouldBe(typeof(Numeric));
        condition.Script.Nodes.ShouldContain(startNode);
    }

    [Fact]
    public void Script_DecidesWhetherATriggerCounts()
    {
        (Layer layer, EventCondition condition) = AddLayer(eventPath: "EventWithArgs");
        TestGreaterThanNode greaterThan = RequireValueAbove(condition, 3);

        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 1});
        using RenderFrame ignored = Render(layer.Profile);
        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 5});
        using RenderFrame counted = Render(layer.Profile);

        ShouldBeOff(ignored);
        ShouldBeOn(counted);
        greaterThan.LastInput.ShouldBe(5);
    }

    [Fact]
    public void Script_IsNotRunWithoutANewTrigger()
    {
        (Layer layer, EventCondition condition) = AddLayer(eventPath: "EventWithArgs");
        TestGreaterThanNode greaterThan = RequireValueAbove(condition, 3);
        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 1});
        using (Render(layer.Profile))
        {
        }

        // Changing the arguments of the last trigger doesn't make it count
        DataModel.EventWithArgs.LastEventArguments!.Value = 5;
        using RenderFrame frame = Render(layer.Profile);

        ShouldBeOff(frame);
        greaterThan.LastInput.ShouldBe(1);
    }

    [Fact]
    public void WithoutAScript_EveryTriggerCounts()
    {
        (Layer layer, _) = AddLayer(eventPath: "EventWithArgs");

        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 1});
        using RenderFrame frame = Render(layer.Profile);

        ShouldBeOn(frame);
    }

    [Fact]
    public void WithoutAnEvent_TheLayerNeverPlays()
    {
        (Layer layer, EventCondition condition) = AddLayer();
        condition.EventPath = null;

        DataModel.Event.Trigger();
        using RenderFrame frame = Render(layer.Profile);

        condition.IsMet.ShouldBeFalse();
        ShouldBeOff(frame);
    }

    #endregion

    #region Values instead of events

    [Fact]
    public void PathToAValue_PlaysWhenTheValueChanges()
    {
        (Layer layer, EventCondition condition) = AddLayer(eventPath: "Integer");
        using (Render(layer.Profile, 1.5))
        {
        }

        using RenderFrame unchanged = Render(layer.Profile);
        DataModel.Integer = 5;
        using RenderFrame changed = Render(layer.Profile);

        condition.GetStartNode().ShouldBeOfType<EventConditionValueChangedStartNode>();
        ShouldBeOff(unchanged);
        ShouldBeOn(changed);
    }

    [Fact]
    public void PathToAValue_DoesNotPlayOnTheFirstFrame()
    {
        (Layer layer, _) = AddLayer(eventPath: "Integer");

        using RenderFrame frame = Render(layer.Profile);

        // The first frame only learns the starting value, nothing changed yet
        ShouldBeOff(frame);
    }

    [Fact]
    public void PathToAValue_OffersTheNewAndOldValue()
    {
        (_, EventCondition condition) = AddLayer(eventPath: "Integer");

        EventConditionValueChangedStartNode startNode = condition.GetStartNode().ShouldBeOfType<EventConditionValueChangedStartNode>();

        startNode.NewValue.Type.ShouldBe(typeof(Numeric));
        startNode.OldValue.Type.ShouldBe(typeof(Numeric));
        condition.Script.Nodes.ShouldContain(startNode);
        condition.Script.Nodes.OfType<EventConditionEventStartNode>().ShouldBeEmpty();
    }

    [Fact]
    public void PathToAValue_LetsTheScriptLookAtTheNewValue()
    {
        (Layer layer, EventCondition condition) = AddLayer(eventPath: "Integer");
        EventConditionValueChangedStartNode startNode = (EventConditionValueChangedStartNode) condition.GetStartNode();
        TestGreaterThanNode greaterThan = _nodes.AddNode<TestGreaterThanNode>(condition.Script);
        greaterThan.Threshold = 3;
        startNode.NewValue.ConnectTo(greaterThan.Input);
        condition.Script.Nodes.OfType<ExitNode<bool>>().Single().Input.ConnectTo(greaterThan.Output);
        using (Render(layer.Profile, 1.5))
        {
        }

        DataModel.Integer = 1;
        using RenderFrame ignored = Render(layer.Profile);
        DataModel.Integer = 5;
        using RenderFrame counted = Render(layer.Profile);

        ShouldBeOff(ignored);
        ShouldBeOn(counted);
    }

    #endregion

    #region Saving and loading

    [Fact]
    public void Condition_SurvivesSavingAndLoading()
    {
        (Layer layer, _) = AddLayer(EventTriggerMode.Toggle, EventOverlapMode.Ignore, EventToggleOffMode.SkipToEnd, eventPath: "EventWithArgs");

        Profile reloaded = Harness.SaveAndReload(layer.Profile);

        EventCondition condition = reloaded.GetAllLayers().Single().DisplayCondition.ShouldBeOfType<EventCondition>();
        condition.TriggerMode.ShouldBe(EventTriggerMode.Toggle);
        condition.OverlapMode.ShouldBe(EventOverlapMode.Ignore);
        condition.ToggleOffMode.ShouldBe(EventToggleOffMode.SkipToEnd);
        DataModelPath eventPath = condition.EventPath.ShouldNotBeNull();
        eventPath.Path.ShouldBe("EventWithArgs");
        eventPath.Target.ShouldBeSameAs(DataModel);
        condition.GetStartNode().Pins.ShouldHaveSingleItem().Name.ShouldBe("Value");
    }

    [Fact]
    public void ScriptLookingAtTheEventArguments_SurvivesSavingAndLoading()
    {
        (Layer layer, EventCondition condition) = AddLayer(eventPath: "EventWithArgs");
        RequireValueAbove(condition, 3);

        Profile reloaded = Harness.SaveAndReload(layer.Profile);
        EventCondition reloadedCondition = (EventCondition) reloaded.GetAllLayers().Single().DisplayCondition;

        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 1});
        using RenderFrame ignored = Render(reloaded);
        DataModel.EventWithArgs.Trigger(new TestEventArgs {Value = 5});
        using RenderFrame counted = Render(reloaded);

        reloadedCondition.Script.Nodes.OfType<TestGreaterThanNode>().Single().Threshold.ShouldBe(3);
        ShouldBeOff(ignored);
        ShouldBeOn(counted);
    }

    [Fact]
    public void ScriptOfAValuePath_SurvivesSavingAndLoading()
    {
        (Layer layer, _) = AddLayer(eventPath: "Integer");

        Profile reloaded = Harness.SaveAndReload(layer.Profile);

        EventCondition condition = (EventCondition) reloaded.GetAllLayers().Single().DisplayCondition;
        condition.GetStartNode().ShouldBeOfType<EventConditionValueChangedStartNode>();
        condition.Script.Nodes.ShouldContain(condition.GetStartNode());
    }

    [Fact]
    public void EmptyScript_IsNotSaved()
    {
        (_, EventCondition condition) = AddLayer();

        condition.Save();

        EventConditionEntity entity = condition.Entity.ShouldBeOfType<EventConditionEntity>();
        entity.Script.ShouldBeNull();
        entity.EventPath.ShouldNotBeNull().Path.ShouldBe("Event");
    }

    [Fact]
    public void ScriptWithOtherNodes_IsSaved()
    {
        (_, EventCondition condition) = AddLayer();
        _nodes.AddNode<TestGreaterThanNode>(condition.Script);

        condition.Save();

        ((EventConditionEntity) condition.Entity).Script.ShouldNotBeNull();
    }

    [Fact]
    public void ConditionWithoutAnEvent_SurvivesSavingAndLoading()
    {
        (Layer layer, EventCondition condition) = AddLayer();
        condition.EventPath = null;

        Profile reloaded = Harness.SaveAndReload(layer.Profile);

        EventCondition reloadedCondition = reloaded.GetAllLayers().Single().DisplayCondition.ShouldBeOfType<EventCondition>();
        reloadedCondition.EventPath.ShouldBeNull();
        reloadedCondition.GetStartNode().ShouldBeOfType<EventConditionEventStartNode>();
    }

    #endregion

    #region Other

    [Fact]
    public void OverridingTheTimeline_MovesItToThePosition()
    {
        (Layer layer, EventCondition condition) = AddLayer();

        condition.OverrideTimeline(TimeSpan.FromSeconds(1.5));

        layer.Timeline.IsOverridden.ShouldBeTrue();
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1.5));
    }

    [Fact]
    public void OverridingTheTimeline_InToggleMode_WrapsAroundTheMainSegment()
    {
        (Layer layer, EventCondition condition) = AddLayer(EventTriggerMode.Toggle);

        condition.OverrideTimeline(TimeSpan.FromSeconds(1.5));

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.5));
    }

    [Fact]
    public void Condition_DependsOnTheModuleOfTheEventAndTheNodesInItsScript()
    {
        (_, EventCondition condition) = AddLayer(eventPath: "EventWithArgs");
        RequireValueAbove(condition, 3);

        condition.GetFeatureDependencies().ShouldContain(_module);
        condition.GetFeatureDependencies().ShouldContain(_nodes);
    }

    [Fact]
    public void Condition_UsesItsScriptAsNodeScript()
    {
        (Layer layer, EventCondition condition) = AddLayer();

        condition.NodeScript.ShouldBeSameAs(condition.Script);
        condition.ProfileElement.ShouldBeSameAs(layer);
        condition.Script.Name.ShouldBe("Activate Layer");
    }

    [Fact]
    public void DisposingTheCondition_DisposesTheEventPath()
    {
        (_, EventCondition condition) = AddLayer();
        DataModelPath eventPath = condition.EventPath!;

        condition.Dispose();

        eventPath.IsValid.ShouldBeFalse();
        DataModel.ActivePaths.ShouldNotContain(eventPath);
    }

    [Fact]
    public void ChangingASetting_RaisesPropertyChanged()
    {
        (_, EventCondition condition) = AddLayer();
        List<string?> changed = [];
        condition.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        condition.TriggerMode = EventTriggerMode.Toggle;
        condition.OverlapMode = EventOverlapMode.Copy;
        condition.ToggleOffMode = EventToggleOffMode.SkipToEnd;

        changed.ShouldBe([nameof(EventCondition.TriggerMode), nameof(EventCondition.OverlapMode), nameof(EventCondition.ToggleOffMode)]);
    }

    #endregion

    private (Layer Layer, EventCondition Condition) AddLayer(EventTriggerMode triggerMode = EventTriggerMode.Play,
        EventOverlapMode overlapMode = EventOverlapMode.Restart,
        EventToggleOffMode toggleOffMode = EventToggleOffMode.Finish,
        string eventPath = "Event",
        bool fadeToBlue = false)
    {
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(Harness.CreateProfile().GetRootFolder(), [Led(LedId.Keyboard_A)]);
        if (fadeToBlue)
        {
            SKColorLayerProperty color = brush.Properties.Color;
            color.KeyframesEnabled = true;
            color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.Red, TimeSpan.Zero, Easings.Functions.Linear, color));
            color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.Blue, TimeSpan.FromSeconds(1), Easings.Functions.Linear, color));
        }

        EventCondition condition = UseEventCondition(layer, eventPath, triggerMode, overlapMode, toggleOffMode);
        return (layer, condition);
    }

    private EventCondition UseEventCondition(RenderProfileElement element, string eventPath,
        EventTriggerMode triggerMode = EventTriggerMode.Play,
        EventOverlapMode overlapMode = EventOverlapMode.Restart,
        EventToggleOffMode toggleOffMode = EventToggleOffMode.Finish)
    {
        element.Timeline.StartSegmentLength = TimeSpan.Zero;
        element.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        element.Timeline.EndSegmentLength = TimeSpan.Zero;

        EventCondition condition = new(element)
        {
            EventPath = new DataModelPath(DataModel, eventPath),
            TriggerMode = triggerMode,
            OverlapMode = overlapMode,
            ToggleOffMode = toggleOffMode
        };
        // Like the editor does after picking an event
        condition.UpdateEventNode(true);
        element.DisplayCondition = condition;
        return condition;
    }

    private TestGreaterThanNode RequireValueAbove(EventCondition condition, float threshold)
    {
        TestGreaterThanNode greaterThan = _nodes.AddNode<TestGreaterThanNode>(condition.Script);
        greaterThan.Threshold = threshold;
        condition.GetStartNode().Pins.Single(p => p.Name == "Value").ConnectTo(greaterThan.Input);
        condition.Script.Nodes.OfType<ExitNode<bool>>().Single().Input.ConnectTo(greaterThan.Output);
        return greaterThan;
    }

    private static SKColor ColorAt(double seconds)
    {
        return new SKColor((byte) Math.Round(255 * (1 - seconds)), 0, (byte) Math.Round(255 * seconds));
    }

    private void ShouldBeOn(RenderFrame frame)
    {
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    private void ShouldBeOff(RenderFrame frame)
    {
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }
}
