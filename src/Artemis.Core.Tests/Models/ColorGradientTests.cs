using System.Collections;
using System.Collections.Specialized;
using SkiaSharp;

namespace Artemis.Core.Tests.Models;

public class ColorGradientTests
{
    #region GetColor

    [Theory]
    [InlineData(0f, 255, 0, 0)]
    [InlineData(0.5f, 127, 0, 127)]
    [InlineData(1f, 0, 0, 255)]
    public void GetColor_InterpolatesBetweenStops(float position, byte red, byte green, byte blue)
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        gradient.GetColor(position).ShouldBe(new SKColor(red, green, blue));
    }

    [Fact]
    public void GetColor_ClampsOutsideOfStops()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.25f), new ColorGradientStop(SKColors.Blue, 0.75f)];

        gradient.GetColor(0f).ShouldBe(SKColors.Red);
        gradient.GetColor(1f).ShouldBe(SKColors.Blue);
    }

    [Theory]
    [InlineData(0.1f)]
    [InlineData(0.9f)]
    [InlineData(-1f)]
    [InlineData(2f)]
    public void GetColor_UsesTheEdgeColorsBeforeTheFirstAndAfterTheLastStop(float position)
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.25f), new ColorGradientStop(SKColors.Blue, 0.75f)];

        gradient.GetColor(position).ShouldBe(position < 0.5f ? SKColors.Red : SKColors.Blue);
    }

    [Fact]
    public void GetColor_OnAnEmptyGradientIsTransparent()
    {
        new ColorGradient().GetColor(0.5f).ShouldBe(SKColors.Transparent);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    public void GetColor_WithASingleStopIsThatStopsColorEverywhere(float position)
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Lime, 0.3f)];

        gradient.GetColor(position).ShouldBe(SKColors.Lime);
    }

    [Fact]
    public void GetColor_InterpolatesBetweenTheTwoStopsAroundThePosition()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(new SKColor(0, 200, 0), 0.5f),
            new ColorGradientStop(SKColors.Blue, 1)
        ];

        gradient.GetColor(0.5f).ShouldBe(new SKColor(0, 200, 0));
        gradient.GetColor(0.75f).ShouldBe(new SKColor(0, 100, 127));
        gradient.GetColor(0.25f).ShouldBe(new SKColor(127, 100, 0));
    }

    [Fact]
    public void GetColor_InterpolatesAlpha()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.White, 0), new ColorGradientStop(SKColors.White.WithAlpha(0), 1)];

        gradient.GetColor(0.5f).ShouldBe(SKColors.White.WithAlpha(127));
    }

    #endregion

    #region Colors and positions

    [Fact]
    public void ColorsAndPositions_FollowTheStopsInOrder()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Blue, 1),
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(SKColors.Lime, 0.5f)
        ];

        gradient.Colors.ShouldBe([SKColors.Red, SKColors.Lime, SKColors.Blue]);
        gradient.Positions.ShouldBe([0f, 0.5f, 1f]);
    }

    [Fact]
    public void ColorsAndPositions_ReflectLaterChanges()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        _ = gradient.Colors;
        _ = gradient.Positions;

        gradient[1].Color = SKColors.Lime;
        gradient.Add(new ColorGradientStop(SKColors.White, 0.5f));

        gradient.Colors.ShouldBe([SKColors.Red, SKColors.White, SKColors.Lime]);
        gradient.Positions.ShouldBe([0f, 0.5f, 1f]);

        gradient.Clear();

        gradient.Colors.ShouldBeEmpty();
        gradient.Positions.ShouldBeEmpty();
    }

    #endregion

    #region Adding, removing and ordering stops

    [Fact]
    public void Add_KeepsStopsOrderedByPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradientStop middle = new(SKColors.Lime, 0.5f);

        gradient.Add(middle);

        gradient.IndexOf(middle).ShouldBe(1);
        gradient.Select(s => s.Position).ShouldBe([0f, 0.5f, 1f]);
    }

    [Fact]
    public void Add_RaisesAnAddAndThenAMoveIntoPlace()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);
        ColorGradientStop middle = new(SKColors.Lime, 0.5f);

        gradient.Add(middle);

        events.Count.ShouldBe(2);
        events[0].Action.ShouldBe(NotifyCollectionChangedAction.Add);
        events[0].NewStartingIndex.ShouldBe(2);
        events[0].NewItems![0].ShouldBeSameAs(middle);
        events[1].Action.ShouldBe(NotifyCollectionChangedAction.Move);
        events[1].OldStartingIndex.ShouldBe(2);
        events[1].NewStartingIndex.ShouldBe(1);
    }

    [Fact]
    public void Add_MovesAStopOnAnOccupiedPositionSlightlyDown()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 0.5f)];
        ColorGradientStop stop = new(SKColors.Lime, 0.5f);

        gradient.Add(stop);

        // Pinned as-is: stops step down 0.001 at a time until they are at least 0.001 away from other stops, but 0.5 - 0.499
        // comes out a hair under 0.001 in floating point, so the stop takes a second step and ends up 0.002 lower
        stop.Position.ShouldBe(0.498f, 0.0001f);
        gradient.IndexOf(stop).ShouldBe(1);
    }

    [Fact]
    public void Add_MovesAStopOnAnOccupiedStartSlightlyUp()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0)];
        ColorGradientStop stop = new(SKColors.Lime, 0);

        gradient.Add(stop);

        stop.Position.ShouldBe(0.001f, 0.0001f);
    }

    [Fact]
    public void Add_SkipsOverEveryOccupiedPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.5f), new ColorGradientStop(SKColors.Red, 0.499f)];
        ColorGradientStop stop = new(SKColors.Lime, 0.5f);

        gradient.Add(stop);

        stop.Position.ShouldBe(0.496f, 0.0001f);
    }

    [Fact]
    public void Insert_KeepsStopsOrderedByPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradientStop middle = new(SKColors.Lime, 0.5f);

        gradient.Insert(0, middle);

        gradient.IndexOf(middle).ShouldBe(1);
    }

    [Fact]
    public void Insert_RaisesTheMoveBeforeTheAdd()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);

        gradient.Insert(0, new ColorGradientStop(SKColors.Lime, 0.5f));

        // Pinned as-is: Insert reorders before reporting the add, so listeners hear about a move of a stop they
        // were never told about, and the add then reports the index the stop ended up at. Add does it the other way around
        events.Select(e => e.Action).ShouldBe([NotifyCollectionChangedAction.Move, NotifyCollectionChangedAction.Add]);
        events[1].NewStartingIndex.ShouldBe(1);
    }

    [Fact]
    public void Insert_DoesNotMoveAStopOffAnOccupiedPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 0.5f)];
        ColorGradientStop stop = new(SKColors.Lime, 0.5f);

        gradient.Insert(0, stop);

        // Pinned as-is: unlike Add, Insert doesn't link the stop to the gradient, so two stops can share a position.
        // Moving the stop later doesn't avoid other stops either
        stop.Position.ShouldBe(0.5f);
        stop.Position = 0f;
        stop.Position.ShouldBe(0f);
        gradient.Count(s => s.Position == 0f).ShouldBe(2);
    }

    [Fact]
    public void Indexer_ReplacesTheStopAndRaisesAReplace()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);
        ColorGradientStop lime = new(SKColors.Lime, 0.2f);

        gradient[0] = lime;

        gradient[0].ShouldBeSameAs(lime);
        events.Single().Action.ShouldBe(NotifyCollectionChangedAction.Replace);
        events.Single().NewItems![0].ShouldBeSameAs(lime);
        events.Single().OldItems![0].ShouldBeSameAs(red);
    }

    [Fact]
    public void Indexer_ReordersAReplacementOnAnotherPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 0.5f)];
        ColorGradientStop last = new(SKColors.Lime, 1f);

        gradient[0] = last;

        gradient.Select(s => s.Color).ShouldBe([SKColors.Blue, SKColors.Lime]);
    }

    [Fact]
    public void Indexer_StopsListeningToTheReplacedStop()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Blue, 1)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient[0] = new ColorGradientStop(SKColors.Lime, 0);
        red.Color = SKColors.White;
        gradient[0].Color = SKColors.Black;

        changes.ShouldBe(1);
    }

    [Fact]
    public void Remove_RemovesTheStopAndRaisesARemove()
    {
        ColorGradientStop blue = new(SKColors.Blue, 1);
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), blue];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);

        gradient.Remove(blue).ShouldBeTrue();

        gradient.Count.ShouldBe(1);
        events.Single().Action.ShouldBe(NotifyCollectionChangedAction.Remove);
        events.Single().OldStartingIndex.ShouldBe(1);
        events.Single().OldItems![0].ShouldBeSameAs(blue);
    }

    [Fact]
    public void Remove_OfAStopThatIsNotInTheGradientDoesNothing()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);

        gradient.Remove(new ColorGradientStop(SKColors.Blue, 0)).ShouldBeFalse();

        gradient.Count.ShouldBe(1);
        events.ShouldBeEmpty();
    }

    [Fact]
    public void Remove_OfAnEqualStopRemovesTheGradientsStopButKeepsListeningToIt()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Blue, 1)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient.Remove(new ColorGradientStop(SKColors.Red, 0)).ShouldBeTrue();
        red.Color = SKColors.White;

        // Pinned as-is: stops are equal when their color and position match, so removing a look-alike removes the
        // gradient's own stop. Only the look-alike is unsubscribed though, the gradient keeps reacting to the removed stop
        gradient.Count.ShouldBe(1);
        changes.ShouldBe(1);
    }

    [Fact]
    public void Remove_StopsListeningToTheStop()
    {
        ColorGradientStop blue = new(SKColors.Blue, 1);
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), blue];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient.Remove(blue);
        blue.Color = SKColors.White;

        changes.ShouldBe(0);
    }

    [Fact]
    public void Remove_LeavesTheStopLinkedToTheGradient()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradientStop blue = new(SKColors.Blue, 1);
        ColorGradient gradient = [red, blue];

        gradient.Remove(blue);
        blue.Position = 0f;

        // Pinned as-is: a removed stop still avoids the positions of the stops in the gradient it was removed from
        blue.Position.ShouldBe(0.001f, 0.0001f);
    }

    [Fact]
    public void RemoveAt_RemovesTheStop()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Blue, 1)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient.RemoveAt(0);
        red.Color = SKColors.White;

        gradient.Single().Color.ShouldBe(SKColors.Blue);
        changes.ShouldBe(0);
    }

    [Fact]
    public void RemoveAt_ReportsTheIndexAsTheRemovedStop()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);

        gradient.RemoveAt(1);

        // Pinned as-is: the index is passed where the event expects the removed item, so listeners get the number 1 as
        // the removed stop and no index. Remove reports the stop and its index properly
        events.Single().Action.ShouldBe(NotifyCollectionChangedAction.Remove);
        events.Single().OldItems![0].ShouldBe(1);
        events.Single().OldStartingIndex.ShouldBe(-1);
    }

    [Fact]
    public void Clear_RemovesAllStopsAndRaisesAReset()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient.Clear();
        red.Color = SKColors.White;

        gradient.Count.ShouldBe(0);
        events.Single().Action.ShouldBe(NotifyCollectionChangedAction.Reset);
        changes.ShouldBe(0);
    }

    [Fact]
    public void ListMembers_WorkLikeAList()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradientStop blue = new(SKColors.Blue, 1);
        ColorGradient gradient = [red, blue];

        gradient.Contains(red).ShouldBeTrue();
        gradient.Contains(new ColorGradientStop(SKColors.Red, 0)).ShouldBeTrue("stops are compared by color and position");
        gradient.IndexOf(blue).ShouldBe(1);
        gradient.IsReadOnly.ShouldBeFalse();

        ColorGradientStop[] array = new ColorGradientStop[3];
        gradient.CopyTo(array, 1);
        array.ShouldBe([null!, red, blue]);
    }

    [Fact]
    public void NonGenericListMembers_OnlyAcceptStops()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        IList list = new ColorGradient {red};

        list.Add("not a stop").ShouldBe(-1);
        list.Insert(0, "not a stop");
        list.Remove("not a stop");
        list.Count.ShouldBe(1);

        ColorGradientStop blue = new(SKColors.Blue, 1);
        list.Add(blue).ShouldBe(1);
        list.Contains(blue).ShouldBeTrue();
        list.IndexOf(blue).ShouldBe(1);
        list[1].ShouldBeSameAs(blue);

        ColorGradientStop lime = new(SKColors.Lime, 0.5f);
        list.Insert(0, lime);
        list.IndexOf(lime).ShouldBe(1);

        ColorGradientStop white = new(SKColors.White, 0);
        list[0] = white;
        list[0].ShouldBeSameAs(white);

        list.Remove(lime);
        list.Count.ShouldBe(2);

        ColorGradientStop[] array = new ColorGradientStop[2];
        list.CopyTo(array, 0);
        array.ShouldBe([white, blue]);

        list.IsFixedSize.ShouldBeFalse();
        list.IsSynchronized.ShouldBeFalse();
        list.SyncRoot.ShouldBeSameAs(list);
    }

    #endregion

    #region Stop changes

    [Fact]
    public void StopChanged_IsRaisedWhenAStopChangesColor()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient[0].Color = SKColors.Lime;

        changes.ShouldBe(1);
    }

    [Fact]
    public void StopChanged_IsNotRaisedWhenNothingChanges()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient[0].Color = SKColors.Red;
        gradient[0].Position = 0;

        changes.ShouldBe(0);
    }

    [Fact]
    public void MovingAStop_KeepsStopsOrderedAndRaisesStopChanged()
    {
        ColorGradientStop red = new(SKColors.Red, 0);
        ColorGradient gradient = [red, new ColorGradientStop(SKColors.Lime, 0.5f), new ColorGradientStop(SKColors.Blue, 1)];
        List<NotifyCollectionChangedEventArgs> events = RecordCollectionChanges(gradient);
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        red.Position = 0.75f;

        gradient.Select(s => s.Color).ShouldBe([SKColors.Lime, SKColors.Red, SKColors.Blue]);
        changes.ShouldBe(1);
        events.ShouldAllBe(e => e.Action == NotifyCollectionChangedAction.Move);
    }

    #endregion

    #region Seamless

    [Fact]
    public void IsSeamless_WhenTheFirstAndLastColorMatch()
    {
        ColorGradient seamless = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 0.5f), new ColorGradientStop(SKColors.Red, 1)];
        ColorGradient notSeamless = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        seamless.IsSeamless().ShouldBeTrue();
        notSeamless.IsSeamless().ShouldBeFalse();
        new ColorGradient().IsSeamless().ShouldBeTrue();
    }

    [Fact]
    public void ToggleSeamless_SqueezesTheStopsAndRepeatsTheFirstColorAtTheEnd()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        gradient.ToggleSeamless();

        gradient.Select(s => s.Color).ShouldBe([SKColors.Red, SKColors.Blue, SKColors.Red]);
        gradient.Select(s => s.Position).ShouldBe([0f, 0.5f, 1f]);
        gradient.IsSeamless().ShouldBeTrue();
    }

    [Fact]
    public void ToggleSeamless_TwiceRestoresTheGradient()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(SKColors.Lime, 0.5f),
            new ColorGradientStop(SKColors.Blue, 1)
        ];
        ColorGradient original = new(gradient);

        gradient.ToggleSeamless();
        gradient.Select(s => s.Position).ShouldBe([0f, 0.333f, 0.667f, 1f]);
        gradient.ToggleSeamless();

        gradient.Select(s => s.Color).ShouldBe(original.Select(s => s.Color));
        gradient.Select(s => s.Position).ShouldBe([0f, 0.5f, 1f], 0.001f);
    }

    [Fact]
    public void ToggleSeamless_OnAGradientThatWasMadeSeamlessByHand()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(SKColors.Lime, 0.5f),
            new ColorGradientStop(SKColors.Blue, 0.9f),
            new ColorGradientStop(SKColors.Red, 1)
        ];

        gradient.ToggleSeamless();

        // Pinned as-is: the remaining stops are stretched as if they were squeezed by an earlier toggle. Blue would land
        // past the end (1.35) so it stays where it was, the cap of 100 in ToggleSeamless looks like it was meant to be 1
        gradient.Select(s => s.Color).ShouldBe([SKColors.Red, SKColors.Lime, SKColors.Blue]);
        gradient.Select(s => s.Position).ShouldBe([0f, 0.75f, 0.9f]);
    }

    [Fact]
    public void ToggleSeamless_OnAnEmptyGradientThrows()
    {
        // Pinned as-is: an empty gradient counts as seamless, so toggling tries to remove a last stop that isn't there
        Should.Throw<InvalidOperationException>(() => new ColorGradient().ToggleSeamless());
    }

    #endregion

    #region Spreading, flipping and rotating

    [Fact]
    public void SpreadStops_SpacesTheStopsEvenly()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0.1f),
            new ColorGradientStop(SKColors.Lime, 0.2f),
            new ColorGradientStop(SKColors.Blue, 0.3f),
            new ColorGradientStop(SKColors.White, 0.4f)
        ];

        gradient.SpreadStops();

        gradient.Select(s => s.Position).ShouldBe([0f, 0.333f, 0.667f, 1f]);
        gradient.Select(s => s.Color).ShouldBe([SKColors.Red, SKColors.Lime, SKColors.Blue, SKColors.White]);
    }

    [Fact]
    public void SpreadStops_AvoidsAStopThatIsStillInTheWay()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(SKColors.Lime, 0.1f),
            new ColorGradientStop(SKColors.Blue, 0.333f),
            new ColorGradientStop(SKColors.White, 0.5f)
        ];

        gradient.SpreadStops();

        // Pinned as-is: stops are moved one by one, Lime can't take 0.333 while Blue still sits there and moves down instead
        gradient.Select(s => s.Position).ShouldBe([0f, 0.331f, 0.667f, 1f], 0.0001f);
    }

    [Fact]
    public void SpreadStops_WithASingleStopGivesItNoPosition()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.5f)];

        gradient.SpreadStops();

        // Pinned as-is: spreading divides by the number of gaps, which is zero for a single stop
        float.IsNaN(gradient[0].Position).ShouldBeTrue();
    }

    [Fact]
    public void FlipStops_MirrorsThePositions()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0.1f),
            new ColorGradientStop(SKColors.Lime, 0.25f),
            new ColorGradientStop(SKColors.Blue, 0.8f)
        ];

        gradient.FlipStops();

        gradient.Select(s => s.Color).ShouldBe([SKColors.Blue, SKColors.Lime, SKColors.Red]);
        gradient.Select(s => s.Position).ShouldBe([0.2f, 0.75f, 0.9f], 0.0001f);
    }

    [Fact]
    public void FlipStops_WithStopsOnMirroredPositions()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0),
            new ColorGradientStop(SKColors.Lime, 0.25f),
            new ColorGradientStop(SKColors.Blue, 1)
        ];

        gradient.FlipStops();

        // Pinned as-is: stops are moved one by one, Red can't take 1 while Blue still sits there so it moves down (by
        // 0.002, see Add_MovesAStopOnAnOccupiedPositionSlightlyDown).
        // The same happens to any pair of stops on mirrored positions, like 0.1 and 0.9
        gradient.Select(s => s.Color).ShouldBe([SKColors.Blue, SKColors.Lime, SKColors.Red]);
        gradient.Select(s => s.Position).ShouldBe([0f, 0.75f, 0.998f], 0.0001f);
    }

    [Fact]
    public void RotateStops_MovesEveryStopToTheNextPosition()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0.1f),
            new ColorGradientStop(SKColors.Lime, 0.5f),
            new ColorGradientStop(SKColors.Blue, 0.9f)
        ];

        gradient.RotateStops(false);

        // Pinned as-is: stops are moved one by one, starting with the last stop which goes to the position of the first.
        // The first stop is still there at that moment, so the last stop lands just below it
        gradient.Select(s => s.Color).ShouldBe([SKColors.Blue, SKColors.Red, SKColors.Lime]);
        gradient.Select(s => s.Position).ShouldBe([0.099f, 0.5f, 0.9f], 0.0001f);
    }

    [Fact]
    public void RotateStops_InverseMovesEveryStopToThePreviousPosition()
    {
        ColorGradient gradient =
        [
            new ColorGradientStop(SKColors.Red, 0.1f),
            new ColorGradientStop(SKColors.Lime, 0.5f),
            new ColorGradientStop(SKColors.Blue, 0.9f)
        ];

        gradient.RotateStops(true);

        // Pinned as-is: like rotating forwards, the first stop to move lands just below the stop that still occupies its
        // new position
        gradient.Select(s => s.Color).ShouldBe([SKColors.Lime, SKColors.Blue, SKColors.Red]);
        gradient.Select(s => s.Position).ShouldBe([0.1f, 0.5f, 0.898f], 0.0001f);
    }

    [Fact]
    public void RotateStops_WithStopsOnBothEnds()
    {
        ColorGradient forward = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Lime, 0.5f), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient backward = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Lime, 0.5f), new ColorGradientStop(SKColors.Blue, 1)];

        forward.RotateStops(false);
        backward.RotateStops(true);

        // Pinned as-is: see above, at the start of the gradient there is no room below so the stop lands just above 0
        forward.Select(s => s.Color).ShouldBe([SKColors.Blue, SKColors.Red, SKColors.Lime]);
        forward.Select(s => s.Position).ShouldBe([0.001f, 0.5f, 1f], 0.0001f);
        backward.Select(s => s.Color).ShouldBe([SKColors.Lime, SKColors.Blue, SKColors.Red]);
        backward.Select(s => s.Position).ShouldBe([0f, 0.5f, 0.998f], 0.0001f);
    }

    [Fact]
    public void BulkChanges_RaiseStopChangedForEveryMovedStop()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.1f), new ColorGradientStop(SKColors.Blue, 0.8f)];
        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;

        gradient.FlipStops();

        changes.ShouldBe(2);
    }

    #endregion

    #region Random and preset gradients

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void Randomize_ReplacesTheStopsWithEvenlySpacedVividColors(int stops)
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Black, 0.5f)];

        gradient.Randomize(stops);

        gradient.Count.ShouldBe(stops);
        gradient.Select(s => s.Position).ShouldBe(Enumerable.Range(0, stops).Select(i => i / (stops - 1f)), 0.0001f);
        foreach (ColorGradientStop stop in gradient)
        {
            stop.Color.ToHsv(out _, out float saturation, out float value);
            saturation.ShouldBe(100f, 0.5f);
            value.ShouldBe(100f, 0.5f);
        }
    }

    [Fact]
    public void Randomize_WithASingleStopGivesItNoPosition()
    {
        ColorGradient gradient = [];

        gradient.Randomize(1);

        // Pinned as-is: like SpreadStops, the spacing divides by the number of gaps, which is zero for a single stop
        float.IsNaN(gradient.Single().Position).ShouldBeTrue();
    }

    [Fact]
    public void GetRandom_CreatesAGradientWithTheGivenAmountOfStops()
    {
        ColorGradient gradient = ColorGradient.GetRandom(4);

        gradient.Count.ShouldBe(4);
        gradient.First().Position.ShouldBe(0f);
        gradient.Last().Position.ShouldBe(1f, 0.0001f);
    }

    [Fact]
    public void GetUnicornBarf_IsASeamlessRainbow()
    {
        ColorGradient gradient = ColorGradient.GetUnicornBarf();

        gradient.Count.ShouldBe(9);
        gradient.IsSeamless().ShouldBeTrue();
        gradient.Select(s => s.Position).ShouldBe(Enumerable.Range(0, 9).Select(i => i / 8f), 0.0001f);
        gradient.Colors.ShouldBe([
            new SKColor(0xFFFF0000), new SKColor(0xFFFF9900), new SKColor(0xFFFFFF00), new SKColor(0xFF00FF00), new SKColor(0xFF00FF7E),
            new SKColor(0xFF0078FF), new SKColor(0xFF9E22FF), new SKColor(0xFFFF34AE), new SKColor(0xFFFF0000)
        ]);
    }

    [Fact]
    public void GetUnicornBarf_ReturnsANewGradientEachTime()
    {
        ColorGradient first = ColorGradient.GetUnicornBarf();
        first[0].Color = SKColors.Black;

        ColorGradient.GetUnicornBarf()[0].Color.ShouldBe(SKColors.Red);
    }

    #endregion

    #region Interpolating gradients

    [Fact]
    public void Interpolate_BlendsEveryStopTowardsTheTarget()
    {
        ColorGradient from = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient to = [new ColorGradientStop(SKColors.Black, 0.5f), new ColorGradientStop(SKColors.White, 1)];

        ColorGradient halfway = from.Interpolate(to, 0.5f);

        halfway.Colors.ShouldBe([new SKColor(127, 0, 0), new SKColor(127, 127, 255)]);
        halfway.Positions.ShouldBe([0.25f, 1f]);
    }

    [Fact]
    public void Interpolate_LeavesBothGradientsAlone()
    {
        ColorGradient from = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient to = [new ColorGradientStop(SKColors.Black, 0.5f), new ColorGradientStop(SKColors.White, 1)];

        ColorGradient result = from.Interpolate(to, 0.5f);

        result.ShouldNotBeSameAs(from);
        from.Colors.ShouldBe([SKColors.Red, SKColors.Blue]);
        from.Positions.ShouldBe([0f, 1f]);
        to.Colors.ShouldBe([SKColors.Black, SKColors.White]);
        to.Positions.ShouldBe([0.5f, 1f]);
    }

    [Fact]
    public void Interpolate_AddsStopsWhenTheTargetHasMore()
    {
        ColorGradient from = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient to = [new ColorGradientStop(SKColors.Black, 0), new ColorGradientStop(SKColors.White, 0.5f), new ColorGradientStop(SKColors.Black, 1)];

        ColorGradient start = from.Interpolate(to, 0f);
        ColorGradient end = from.Interpolate(to, 1f);

        // The new stop takes the position of the target's first stop and the color the gradient has there. That position
        // is taken by Red, so it moves up a little
        start.Colors.ShouldBe([SKColors.Red, SKColors.Red, SKColors.Blue]);
        start.Positions.ShouldBe([0f, 0.001f, 1f], 0.0001f);
        end.Colors.ShouldBe([SKColors.Black, SKColors.White, SKColors.Black]);
        end.Positions.ShouldBe([0f, 0.5f, 1f], 0.0001f);
    }

    [Fact]
    public void Interpolate_BlendsExtraStopsTowardsTheTargetsColorAtTheirPosition()
    {
        ColorGradient from = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Lime, 0.5f), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient to = [new ColorGradientStop(SKColors.Black, 0), new ColorGradientStop(SKColors.White, 1)];

        ColorGradient end = from.Interpolate(to, 1f);

        // The first two stops follow the target's stops, the third has no counterpart and takes the target's color at its
        // own position. Pinned as-is: the second stop goes to the target's last stop at 1, which is still taken by the
        // third stop, so it lands just below it
        end.Count.ShouldBe(3);
        end.Colors.ShouldBe([SKColors.Black, SKColors.White, SKColors.White]);
        end.Positions.ShouldBe([0f, 0.998f, 1f], 0.0001f);
    }

    #endregion

    #region Copies and equality

    [Fact]
    public void Copy_HasEqualButSeparateStops()
    {
        ColorGradient original = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        ColorGradient copy = new(original);

        copy.ShouldBe(original);
        copy.GetHashCode().ShouldBe(original.GetHashCode());
        copy[0].ShouldNotBeSameAs(original[0]);

        copy[0].Color = SKColors.Lime;
        original[0].Color.ShouldBe(SKColors.Red);
        copy.ShouldNotBe(original);
    }

    [Fact]
    public void Copy_ListensToItsOwnStops()
    {
        ColorGradient original = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        ColorGradient copy = new(original);
        ColorGradientStop copiedRed = copy[0], copiedBlue = copy[1];
        int originalChanges = 0, copyChanges = 0;
        original.StopChanged += (_, _) => originalChanges++;
        copy.StopChanged += (_, _) => copyChanges++;

        copiedRed.Position = 0.5f;
        copiedBlue.Position = 0.5f;

        copyChanges.ShouldBe(2);
        originalChanges.ShouldBe(0);
        copiedBlue.Position.ShouldBe(0.498f, 0.0001f, "the copied stops avoid each other's position");
    }

    [Fact]
    public void Copy_OfNothingIsEmpty()
    {
        new ColorGradient((ColorGradient?) null).ShouldBeEmpty();
    }

    [Fact]
    public void Copy_FromAListOfStops()
    {
        List<ColorGradientStop> stops = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        ColorGradient gradient = new(stops);

        gradient.Count.ShouldBe(2);
        gradient[0].ShouldNotBeSameAs(stops[0]);
        gradient[0].ShouldBe(stops[0]);

        int changes = 0;
        gradient.StopChanged += (_, _) => changes++;
        gradient[1].Color = SKColors.Lime;
        stops[1].Color.ShouldBe(SKColors.Blue);
        changes.ShouldBe(1);
    }

    [Fact]
    public void Copy_FromAListKeepsTheOrderOfTheList()
    {
        List<ColorGradientStop> stops = [new ColorGradientStop(SKColors.Blue, 1), new ColorGradientStop(SKColors.Red, 0)];

        ColorGradient gradient = new(stops);

        // Pinned as-is: the copy constructors don't sort, so stops out of order stay out of order until one of them changes,
        // and GetColor relies on stops being in order
        gradient.Colors.ShouldBe([SKColors.Blue, SKColors.Red]);
        gradient.GetColor(0.5f).ShouldBe(SKColors.Blue);
    }

    [Fact]
    public void Equals_ComparesStopsInOrder()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        gradient.Equals(new ColorGradient {new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)}).ShouldBeTrue();
        gradient.Equals(new ColorGradient {new ColorGradientStop(SKColors.Red, 0)}).ShouldBeFalse();
        gradient.Equals(new ColorGradient {new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 0.9f)}).ShouldBeFalse();
        gradient.Equals(new ColorGradient {new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Lime, 1)}).ShouldBeFalse();
        gradient.Equals(gradient).ShouldBeTrue();
        gradient.Equals(null).ShouldBeFalse();
        gradient.Equals(gradient.ToList()).ShouldBeFalse();
        new ColorGradient().Equals(new ColorGradient()).ShouldBeTrue();
        new ColorGradient().GetHashCode().ShouldBe(new ColorGradient().GetHashCode());
    }

    #endregion

    private static List<NotifyCollectionChangedEventArgs> RecordCollectionChanges(ColorGradient gradient)
    {
        List<NotifyCollectionChangedEventArgs> events = [];
        gradient.CollectionChanged += (_, e) => events.Add(e);
        return events;
    }
}
