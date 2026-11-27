using Artemis.Core.Internal;

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

public static class TestConditions
{
    /// <summary>
    ///     Gives the element a static display condition whose script is driven by the returned node.
    /// </summary>
    public static TestBooleanNode UseStaticCondition(RenderProfileElement element, bool isMet, StaticPlayMode playMode = StaticPlayMode.Repeat, StaticStopMode stopMode = StaticStopMode.Finish)
    {
        StaticCondition condition = new(element) {PlayMode = playMode, StopMode = stopMode};
        TestBooleanNode node = new() {Value = isMet};
        condition.Script.AddNode(node);
        condition.Script.Nodes.OfType<ExitNode<bool>>().Single().Input.ConnectTo(node.Output);

        element.DisplayCondition = condition;
        return node;
    }

    /// <summary>
    ///     Gives the element a play once display condition.
    /// </summary>
    public static void UsePlayOnceCondition(RenderProfileElement element)
    {
        element.DisplayCondition = new PlayOnceCondition(element);
    }
}
