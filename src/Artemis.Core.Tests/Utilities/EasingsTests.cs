using System.Reflection;

namespace Artemis.Core.Tests.Utilities;

public class EasingsTests
{
    public static TheoryData<Easings.Functions> AllFunctions => new(Enum.GetValues<Easings.Functions>());

    /// <summary>
    ///     Each ease in function with its matching ease out function
    /// </summary>
    public static TheoryData<Easings.Functions, Easings.Functions> InAndOutPairs => new()
    {
        {Easings.Functions.QuadraticEaseIn, Easings.Functions.QuadraticEaseOut},
        {Easings.Functions.CubicEaseIn, Easings.Functions.CubicEaseOut},
        {Easings.Functions.QuarticEaseIn, Easings.Functions.QuarticEaseOut},
        {Easings.Functions.QuinticEaseIn, Easings.Functions.QuinticEaseOut},
        {Easings.Functions.SineEaseIn, Easings.Functions.SineEaseOut},
        {Easings.Functions.CircularEaseIn, Easings.Functions.CircularEaseOut},
        {Easings.Functions.ExponentialEaseIn, Easings.Functions.ExponentialEaseOut},
        {Easings.Functions.ElasticEaseIn, Easings.Functions.ElasticEaseOut},
        {Easings.Functions.BackEaseIn, Easings.Functions.BackEaseOut},
        {Easings.Functions.BounceEaseIn, Easings.Functions.BounceEaseOut}
    };

    [Theory]
    [MemberData(nameof(AllFunctions))]
    public void Interpolate_StartsAtZeroAndEndsAtOne(Easings.Functions function)
    {
        Easings.Interpolate(0, function).ShouldBe(0, 0.000001);
        Easings.Interpolate(1, function).ShouldBe(1, 0.000001);
    }

    [Theory]
    [MemberData(nameof(AllFunctions))]
    public void Interpolate_UsesTheFunctionOfTheSameName(Easings.Functions function)
    {
        MethodInfo method = typeof(Easings).GetMethod(function.ToString(), BindingFlags.Public | BindingFlags.Static)!;

        foreach (double p in new[] {0.1, 0.3, 0.5, 0.7, 0.9})
            Easings.Interpolate(p, function).ShouldBe((double) method.Invoke(null, [p])!, $"{function} at {p}");
    }

    [Theory]
    [MemberData(nameof(AllFunctions))]
    public void InOutFunctions_AreHalfwayHalfway(Easings.Functions function)
    {
        if (!function.ToString().EndsWith("InOut"))
            return;

        Easings.Interpolate(0.5, function).ShouldBe(0.5, 0.000001);
    }

    [Theory]
    [MemberData(nameof(InAndOutPairs))]
    public void EaseIn_IsEaseOutPlayedBackwards(Easings.Functions easeIn, Easings.Functions easeOut)
    {
        foreach (double p in new[] {0.1, 0.25, 0.5, 0.75, 0.9})
            Easings.Interpolate(p, easeIn).ShouldBe(1 - Easings.Interpolate(1 - p, easeOut), 0.000001, $"{easeIn} at {p}");
    }

    [Theory]
    [InlineData(Easings.Functions.Linear, 0.3, 0.3)]
    [InlineData(Easings.Functions.QuadraticEaseIn, 0.5, 0.25)]
    [InlineData(Easings.Functions.QuadraticEaseOut, 0.5, 0.75)]
    [InlineData(Easings.Functions.QuadraticEaseInOut, 0.25, 0.125)]
    [InlineData(Easings.Functions.CubicEaseIn, 0.5, 0.125)]
    [InlineData(Easings.Functions.CubicEaseInOut, 0.25, 0.0625)]
    [InlineData(Easings.Functions.QuarticEaseIn, 0.5, 0.0625)]
    [InlineData(Easings.Functions.QuarticEaseInOut, 0.25, 0.03125)]
    [InlineData(Easings.Functions.QuinticEaseIn, 0.5, 0.03125)]
    [InlineData(Easings.Functions.QuinticEaseInOut, 0.25, 0.015625)]
    [InlineData(Easings.Functions.SineEaseOut, 0.5, 0.70710678)]
    [InlineData(Easings.Functions.ExponentialEaseIn, 0.5, 0.03125)]
    [InlineData(Easings.Functions.BounceEaseOut, 0.5, 0.71875)]
    public void Functions_FollowTheirCurve(Easings.Functions function, double p, double expected)
    {
        Easings.Interpolate(p, function).ShouldBe(expected, 0.000001);
    }

    [Fact]
    public void BackAndElastic_OvershootTheRange()
    {
        Easings.BackEaseIn(0.3).ShouldBeLessThan(0);
        Easings.BackEaseOut(0.7).ShouldBeGreaterThan(1);
        Easings.ElasticEaseOut(0.1).ShouldBeGreaterThan(1);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 0)]
    [InlineData(0.999, 0)]
    [InlineData(1, 1)]
    public void Step_JumpsAtTheEnd(double p, double expected)
    {
        Easings.Step(p).ShouldBe(expected);
    }

    [Fact]
    public void Interpolate_WithAnUnknownFunctionIsLinear()
    {
        Easings.Interpolate(0.3, (Easings.Functions) 999).ShouldBe(0.3);
    }
}
