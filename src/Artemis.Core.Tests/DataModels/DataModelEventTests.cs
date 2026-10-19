using Artemis.Core.Tests.Infrastructure.Plugins.Modules;

namespace Artemis.Core.Tests.DataModels;

public class DataModelEventTests
{
    #region Events without arguments

    [Fact]
    public void Event_StartsOutNeverTriggered()
    {
        DataModelEvent dataModelEvent = new();

        dataModelEvent.TriggerCount.ShouldBe(0);
        dataModelEvent.LastTrigger.ShouldBe(default);
        dataModelEvent.LastTriggerArguments.ShouldBeNull();
        dataModelEvent.LastEventArgumentsUntyped.ShouldBeNull();
        dataModelEvent.EventArgumentsHistory.ShouldBeEmpty();
        dataModelEvent.TrackHistory.ShouldBeFalse();
    }

    [Fact]
    public void Event_DescribesItself()
    {
        DataModelEvent dataModelEvent = new();

        dataModelEvent.ArgumentsType.ShouldBe(typeof(DataModelEventArgs));
        dataModelEvent.TriggerPastParticiple.ShouldBe("triggered");
    }

    [Fact]
    public void TriggeringAnEvent_CountsTheTriggerAndRemembersWhen()
    {
        DataModelEvent dataModelEvent = new();
        int raised = 0;
        dataModelEvent.EventTriggered += (_, _) => raised++;
        DateTime before = DateTime.Now;

        dataModelEvent.Trigger();
        dataModelEvent.Trigger();

        raised.ShouldBe(2);
        dataModelEvent.TriggerCount.ShouldBe(2);
        dataModelEvent.LastTrigger.ShouldBeInRange(before, DateTime.Now);
        dataModelEvent.TimeSinceLastTrigger.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
        DataModelEventArgs arguments = dataModelEvent.LastTriggerArguments.ShouldNotBeNull();
        arguments.TriggerTime.ShouldBeInRange(before, DateTime.Now);
        dataModelEvent.LastEventArgumentsUntyped.ShouldBeSameAs(arguments);
    }

    [Fact]
    public void Event_OnlyKeepsHistoryWhenAskedTo()
    {
        DataModelEvent withoutHistory = new();
        DataModelEvent withHistory = new(true);

        withoutHistory.Trigger();
        withHistory.Trigger();
        withHistory.Trigger();

        withoutHistory.EventArgumentsHistory.ShouldBeEmpty();
        withHistory.TrackHistory.ShouldBeTrue();
        withHistory.EventArgumentsHistory.Count.ShouldBe(2);
        withHistory.EventArgumentsHistoryUntyped.Last().ShouldBeSameAs(withHistory.LastTriggerArguments);
    }

    [Fact]
    public void EventHistory_KeepsTheLastTwentyTriggers()
    {
        DataModelEvent dataModelEvent = new(true);
        for (int i = 0; i < 20; i++)
            dataModelEvent.Trigger();
        DataModelEventArgs oldest = dataModelEvent.EventArgumentsHistory.Peek();

        dataModelEvent.Trigger();

        dataModelEvent.EventArgumentsHistory.Count.ShouldBe(20);
        dataModelEvent.EventArgumentsHistory.ShouldNotContain(oldest);
    }

    [Fact]
    public void ChangingHistoryTracking_ClearsTheHistory()
    {
        DataModelEvent dataModelEvent = new(true);
        dataModelEvent.Trigger();

        dataModelEvent.TrackHistory = true;

        dataModelEvent.EventArgumentsHistory.ShouldBeEmpty();
    }

    [Fact]
    public void ResettingAnEvent_ClearsTheCountAndHistoryButNotTheLastTrigger()
    {
        DataModelEvent dataModelEvent = new(true);
        dataModelEvent.Trigger();
        DateTime lastTrigger = dataModelEvent.LastTrigger;

        dataModelEvent.Reset();
        dataModelEvent.Update();

        dataModelEvent.TriggerCount.ShouldBe(0);
        dataModelEvent.EventArgumentsHistory.ShouldBeEmpty();
        dataModelEvent.LastTrigger.ShouldBe(lastTrigger);
        dataModelEvent.LastTriggerArguments.ShouldNotBeNull();
    }

    #endregion

    #region Events with arguments

    [Fact]
    public void EventWithArguments_StartsOutNeverTriggered()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new();

        dataModelEvent.TriggerCount.ShouldBe(0);
        dataModelEvent.LastTrigger.ShouldBe(default);
        dataModelEvent.LastEventArguments.ShouldBeNull();
        dataModelEvent.LastEventArgumentsUntyped.ShouldBeNull();
        dataModelEvent.TrackHistory.ShouldBeFalse();
        dataModelEvent.ArgumentsType.ShouldBe(typeof(TestEventArgs));
        dataModelEvent.TriggerPastParticiple.ShouldBe("triggered");
    }

    [Fact]
    public void TriggeringAnEventWithArguments_RemembersTheArguments()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new();
        int raised = 0;
        dataModelEvent.EventTriggered += (_, _) => raised++;
        TestEventArgs arguments = new() {Value = 5};
        DateTime before = DateTime.Now;

        dataModelEvent.Trigger(arguments);

        raised.ShouldBe(1);
        dataModelEvent.TriggerCount.ShouldBe(1);
        dataModelEvent.LastEventArguments.ShouldBeSameAs(arguments);
        dataModelEvent.LastEventArgumentsUntyped.ShouldBeSameAs(arguments);
        dataModelEvent.LastTrigger.ShouldBeInRange(before, DateTime.Now);
        dataModelEvent.TimeSinceLastTrigger.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
        arguments.TriggerTime.ShouldBeInRange(before, DateTime.Now);
    }

    [Fact]
    public void EventWithArguments_RequiresArguments()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new();

        Should.Throw<ArgumentNullException>(() => dataModelEvent.Trigger(null!));
        dataModelEvent.TriggerCount.ShouldBe(0);
    }

    [Fact]
    public void EventWithArguments_OnlyKeepsHistoryWhenAskedTo()
    {
        DataModelEvent<TestEventArgs> withoutHistory = new();
        DataModelEvent<TestEventArgs> withHistory = new(true);

        withoutHistory.Trigger(new TestEventArgs());
        withHistory.Trigger(new TestEventArgs {Value = 1});
        withHistory.Trigger(new TestEventArgs {Value = 2});

        withoutHistory.EventArgumentsHistory.ShouldBeEmpty();
        withHistory.EventArgumentsHistory.Select(a => a.Value).ShouldBe([1, 2]);
        withHistory.EventArgumentsHistoryUntyped.Cast<TestEventArgs>().Select(a => a.Value).ShouldBe([1, 2]);
    }

    [Fact]
    public void EventWithArgumentsHistory_KeepsTheLastTwentyTriggers()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new(true);
        for (int i = 1; i <= 21; i++)
            dataModelEvent.Trigger(new TestEventArgs {Value = i});

        dataModelEvent.EventArgumentsHistory.Count.ShouldBe(20);
        dataModelEvent.EventArgumentsHistory.Peek().Value.ShouldBe(2);
    }

    [Fact]
    public void ChangingHistoryTrackingOfAnEventWithArguments_ClearsTheHistory()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new(true);
        dataModelEvent.Trigger(new TestEventArgs());

        dataModelEvent.TrackHistory = false;

        dataModelEvent.TrackHistory.ShouldBeFalse();
        dataModelEvent.EventArgumentsHistory.ShouldBeEmpty();
    }

    [Fact]
    public void ResettingAnEventWithArguments_ClearsTheCountAndHistoryButNotTheLastArguments()
    {
        DataModelEvent<TestEventArgs> dataModelEvent = new(true);
        TestEventArgs arguments = new() {Value = 3};
        dataModelEvent.Trigger(arguments);

        dataModelEvent.Reset();
        dataModelEvent.Update();

        dataModelEvent.TriggerCount.ShouldBe(0);
        dataModelEvent.EventArgumentsHistory.ShouldBeEmpty();
        dataModelEvent.LastEventArguments.ShouldBeSameAs(arguments);
    }

    #endregion
}
