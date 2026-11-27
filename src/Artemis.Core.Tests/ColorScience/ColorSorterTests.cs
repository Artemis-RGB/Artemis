using Artemis.Core.ColorScience;
using SkiaSharp;

namespace Artemis.Core.Tests.ColorScience;

public class ColorSorterTests
{
    [Fact]
    public void Sort_StartsClosestToTheStartColorAndWalksToTheNearestColor()
    {
        SKColor[] colors = [SKColors.White, new(128, 128, 128), SKColors.Black, new(64, 64, 64)];

        ColorSorter.Sort(colors, SKColors.Black);

        colors.ShouldBe([SKColors.Black, new SKColor(64, 64, 64), new SKColor(128, 128, 128), SKColors.White]);
    }

    [Fact]
    public void Sort_FromWhiteStartsAtTheLightEnd()
    {
        SKColor[] colors = [SKColors.Black, new(128, 128, 128), SKColors.White, new(64, 64, 64)];

        ColorSorter.Sort(colors, SKColors.White);

        colors.ShouldBe([SKColors.White, new SKColor(128, 128, 128), new SKColor(64, 64, 64), SKColors.Black]);
    }

    [Fact]
    public void Sort_StartsFromBlackByDefault()
    {
        SKColor[] colors = [SKColors.White, SKColors.Black];

        ColorSorter.Sort(colors);

        colors.ShouldBe([SKColors.Black, SKColors.White]);
    }

    [Fact]
    public void Sort_FollowsTheNearestColorRatherThanTheStartColor()
    {
        // Grays with a lightness of about 60, 35 and 75, sorted from a gray of about 50. The darker gray is closer to the
        // start, but after the 60 gray the 75 gray is closer, so the sorter picks that one first
        SKColor gray60 = new(145, 145, 145);
        SKColor gray35 = new(82, 82, 82);
        SKColor gray75 = new(184, 184, 184);
        SKColor[] colors = [gray35, gray75, gray60];

        ColorSorter.Sort(colors, new SKColor(119, 119, 119));

        colors.ShouldBe([gray60, gray75, gray35]);
    }

    [Fact]
    public void Sort_KeepsDuplicates()
    {
        SKColor[] colors = [SKColors.White, SKColors.Black, SKColors.White, SKColors.Black];

        ColorSorter.Sort(colors, SKColors.Black);

        colors.ShouldBe([SKColors.Black, SKColors.Black, SKColors.White, SKColors.White]);
    }

    [Fact]
    public void Sort_HandlesManyColors()
    {
        // Over 1024 colors the sorter rents a buffer instead of using the stack
        Random random = new(42);
        SKColor[] colors = Enumerable.Range(0, 2000).Select(_ => (byte) random.Next(256)).Select(v => new SKColor(v, v, v)).ToArray();

        ColorSorter.Sort(colors, SKColors.Black);

        colors.Select(c => c.Red).ShouldBeInOrder(SortDirection.Ascending);
        colors.Length.ShouldBe(2000);
    }

    [Fact]
    public void Sort_OfNothingDoesNothing()
    {
        SKColor[] colors = [];

        ColorSorter.Sort(colors, SKColors.Black);

        colors.ShouldBeEmpty();
    }

    [Fact]
    public void GetColorDifference_IsZeroForTheSameColor()
    {
        ColorSorter.GetColorDifference(SKColors.Orange, SKColors.Orange).ShouldBe(0f);
    }

    [Fact]
    public void GetColorDifference_BetweenBlackAndWhiteIsTheFullLightnessRange()
    {
        ColorSorter.GetColorDifference(SKColors.Black, SKColors.White).ShouldBe(100f, 0.1f);
    }

    [Fact]
    public void GetColorDifference_IsSmallerForSimilarColors()
    {
        float similar = ColorSorter.GetColorDifference(SKColors.Red, new SKColor(230, 20, 20));
        float different = ColorSorter.GetColorDifference(SKColors.Red, SKColors.Blue);

        similar.ShouldBeLessThan(different);
        similar.ShouldBeGreaterThan(0f);
    }

    [Fact]
    public void GetColorDifference_DependsOnWhichColorComesFirst()
    {
        // The formula Artemis uses (CIE94) weighs the difference by how colorful the first color is, so swapping the colors
        // gives a different answer
        float redToGray = ColorSorter.GetColorDifference(SKColors.Red, SKColors.Gray);
        float grayToRed = ColorSorter.GetColorDifference(SKColors.Gray, SKColors.Red);

        redToGray.ShouldBeLessThan(grayToRed);
    }

    [Fact]
    public void LabColor_HasTheExpectedLightness()
    {
        LabColor white = new(SKColors.White);
        LabColor black = new(SKColors.Black);
        LabColor gray = new(new SKColor(119, 119, 119));

        white.L.ShouldBe(100f, 0.1f);
        white.A.ShouldBe(0f, 0.1f);
        white.B.ShouldBe(0f, 0.1f);
        black.L.ShouldBe(0f, 0.1f);
        gray.L.ShouldBe(50f, 0.5f);
    }

    [Fact]
    public void LabColor_PlacesColorsOnTheirSide()
    {
        // In Lab, A runs from green (negative) to red (positive) and B from blue (negative) to yellow (positive)
        new LabColor(SKColors.Red).A.ShouldBeGreaterThan(50f);
        new LabColor(SKColors.Lime).A.ShouldBeLessThan(-50f);
        new LabColor(SKColors.Blue).B.ShouldBeLessThan(-50f);
        new LabColor(SKColors.Yellow).B.ShouldBeGreaterThan(50f);
    }
}
