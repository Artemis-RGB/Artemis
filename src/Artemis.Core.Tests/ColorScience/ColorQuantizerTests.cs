using Artemis.Core.ColorScience;
using Artemis.Core.Tests.Infrastructure;
using SkiaSharp;

namespace Artemis.Core.Tests.ColorScience;

public class ColorQuantizerTests
{
    // One color for each type, right on the target lightness and well within the saturation limits of that type
    private static readonly SKColor Vibrant = SKColor.FromHsl(0, 80, 50);
    private static readonly SKColor LightVibrant = SKColor.FromHsl(120, 80, 74);
    private static readonly SKColor DarkVibrant = SKColor.FromHsl(240, 80, 26);
    private static readonly SKColor Muted = SKColor.FromHsl(60, 20, 50);
    private static readonly SKColor LightMuted = SKColor.FromHsl(180, 20, 74);
    private static readonly SKColor DarkMuted = SKColor.FromHsl(300, 20, 26);

    private static SKColor[] AllTypes => [Vibrant, LightVibrant, DarkVibrant, Muted, LightMuted, DarkMuted];

    #region Quantize

    [Fact]
    public void Quantize_FindsTheDistinctColors()
    {
        SKColor[] colors = [..Enumerable.Repeat(SKColors.Red, 8), ..Enumerable.Repeat(SKColors.Blue, 8)];

        SKColor[] result = ColorQuantizer.Quantize(colors, 2);

        result.ShouldBe([SKColors.Red, SKColors.Blue], ignoreOrder: true);
    }

    [Fact]
    public void Quantize_ToASingleColorAveragesTheColors()
    {
        SKColor[] colors = [..Enumerable.Repeat(SKColors.Red, 8), ..Enumerable.Repeat(SKColors.Blue, 8)];

        SKColor[] result = ColorQuantizer.Quantize(colors, 1);

        result.Length.ShouldBe(1);
        result[0].ShouldBeColor(new SKColor(127, 0, 127), 1);
    }

    [Fact]
    public void Quantize_GroupsSimilarColors()
    {
        SKColor[] colors =
        [
            new(250, 0, 0), new(255, 5, 0), new(245, 0, 5), new(255, 0, 0),
            new(0, 0, 250), new(5, 0, 255), new(0, 5, 245), new(0, 0, 255)
        ];

        SKColor[] result = ColorQuantizer.Quantize(colors, 2).OrderByDescending(c => c.Red).ToArray();

        result[0].ShouldBeColor(new SKColor(251, 1, 1), 2);
        result[1].ShouldBeColor(new SKColor(1, 1, 251), 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    public void Quantize_OnlyAcceptsPowersOfTwo(int amount)
    {
        SKColor[] colors = Enumerable.Repeat(SKColors.Red, 16).ToArray();

        Should.Throw<ArgumentException>(() => ColorQuantizer.Quantize(colors, amount)).ParamName.ShouldBe("amount");
    }

    [Fact]
    public void QuantizeSplit_DoublesTheColorsWithEverySplit()
    {
        SKColor[] colors = Enumerable.Range(0, 64).Select(i => new SKColor((byte) (i * 4), 0, 0)).ToArray();

        ColorQuantizer.QuantizeSplit(colors, 0).Length.ShouldBe(1);
        ColorQuantizer.QuantizeSplit(colors, 1).Length.ShouldBe(2);
        ColorQuantizer.QuantizeSplit(colors, 3).Length.ShouldBe(8);
    }

    [Fact]
    public void QuantizeSplit_NeedsAtLeastAsManyColorsAsItReturns()
    {
        SKColor[] colors = Enumerable.Repeat(SKColors.Red, 7).ToArray();

        Should.Throw<ArgumentException>(() => ColorQuantizer.QuantizeSplit(colors, 3));
    }

    #endregion

    #region Color variations

    [Fact]
    public void FindColorVariation_FindsEachTypeOfColor()
    {
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.Vibrant).ShouldBe(Vibrant);
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.LightVibrant).ShouldBe(LightVibrant);
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.DarkVibrant).ShouldBe(DarkVibrant);
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.Muted).ShouldBe(Muted);
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.LightMuted).ShouldBe(LightMuted);
        ColorQuantizer.FindColorVariation(AllTypes, ColorType.DarkMuted).ShouldBe(DarkMuted);
    }

    [Fact]
    public void FindColorVariation_PrefersTheColorClosestToTheTarget()
    {
        SKColor closer = SKColor.FromHsl(0, 90, 50);
        SKColor further = SKColor.FromHsl(0, 50, 60);

        ColorQuantizer.FindColorVariation([further, closer], ColorType.Vibrant).ShouldBe(closer);
    }

    [Fact]
    public void FindColorVariation_IsEmptyWhenNoColorFits()
    {
        ColorQuantizer.FindColorVariation([Muted, LightMuted, DarkMuted], ColorType.Vibrant).ShouldBe(SKColor.Empty);
        ColorQuantizer.FindColorVariation([], ColorType.Vibrant).ShouldBe(SKColor.Empty);
    }

    [Fact]
    public void FindColorVariation_IgnoringTheLimitsAlwaysFindsAColor()
    {
        ColorQuantizer.FindColorVariation([Muted, DarkMuted], ColorType.Vibrant, true).ShouldBe(Muted);
    }

    [Fact]
    public void FindColorVariation_FullySaturatedColorsAreVibrantAndGraysAreMuted()
    {
        ColorQuantizer.FindColorVariation([SKColors.Red], ColorType.Vibrant).ShouldBe(SKColors.Red);
        ColorQuantizer.FindColorVariation([SKColors.Gray], ColorType.Muted).ShouldBe(SKColors.Gray);
    }

    [Fact]
    public void FindColorVariation_PureBlackAndWhiteAreNeverMuted()
    {
        ColorQuantizer.FindColorVariation([SKColors.Black], ColorType.DarkMuted).ShouldBe(SKColor.Empty);
        ColorQuantizer.FindColorVariation([SKColors.White], ColorType.LightMuted).ShouldBe(SKColor.Empty);
    }

    [Fact]
    public void FindAllColorVariations_FillsEveryType()
    {
        ColorSwatch swatch = ColorQuantizer.FindAllColorVariations(AllTypes);

        swatch.ShouldBe(new ColorSwatch
        {
            Vibrant = Vibrant,
            LightVibrant = LightVibrant,
            DarkVibrant = DarkVibrant,
            Muted = Muted,
            LightMuted = LightMuted,
            DarkMuted = DarkMuted
        });
    }

    [Fact]
    public void FindAllColorVariations_LeavesTypesWithoutAFittingColorEmpty()
    {
        ColorSwatch swatch = ColorQuantizer.FindAllColorVariations([Vibrant, DarkMuted]);

        swatch.Vibrant.ShouldBe(Vibrant);
        swatch.DarkMuted.ShouldBe(DarkMuted);
        swatch.LightVibrant.ShouldBe(SKColor.Empty);
        swatch.DarkVibrant.ShouldBe(SKColor.Empty);
        swatch.Muted.ShouldBe(SKColor.Empty);
        swatch.LightMuted.ShouldBe(SKColor.Empty);
    }

    [Fact]
    public void FindAllColorVariations_IgnoringTheLimitsFillsEveryType()
    {
        ColorSwatch swatch = ColorQuantizer.FindAllColorVariations([Vibrant], true);

        swatch.ShouldBe(new ColorSwatch
        {
            Vibrant = Vibrant,
            LightVibrant = Vibrant,
            DarkVibrant = Vibrant,
            Muted = Vibrant,
            LightMuted = Vibrant,
            DarkMuted = Vibrant
        });
    }

    [Fact]
    public void ColorSwatch_ToStringIsTheTypeName()
    {
        // The data model viewer shows this instead of every color
        new ColorSwatch {Vibrant = Vibrant}.ToString().ShouldBe(typeof(ColorSwatch).FullName);
    }

    #endregion

    #region Gradients from images

    [Fact]
    public void GetGradientFromImage_BuildsAnEvenlySpacedGradientOfTheImagesColorTypes()
    {
        using SKBitmap bitmap = CreateImage(AllTypes);

        ColorGradient gradient = ColorQuantizer.GetGradientFromImage(bitmap);

        gradient.Count.ShouldBe(6);
        gradient.Select(s => s.Position).ShouldBe([0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f], 0.0001f);
        gradient.Select(s => s.Color).ShouldBe(AllTypes, ignoreOrder: true);
    }

    [Fact]
    public void GetGradientFromImage_SortsTheColorsStartingFromBlack()
    {
        using SKBitmap bitmap = CreateImage(AllTypes);

        ColorGradient gradient = ColorQuantizer.GetGradientFromImage(bitmap);

        SKColor[] sorted = AllTypes;
        ColorSorter.Sort(sorted, SKColors.Black);
        gradient.Colors.ShouldBe(sorted);
    }

    [Fact]
    public void GetGradientFromImage_UsesEmptyColorsForMissingTypes()
    {
        using SKBitmap bitmap = CreateImage([Vibrant, DarkVibrant]);

        ColorGradient gradient = ColorQuantizer.GetGradientFromImage(bitmap);

        gradient.Count.ShouldBe(6);
        gradient.Count(s => s.Color == SKColor.Empty).ShouldBe(4);
        gradient.Colors.ShouldContain(Vibrant);
        gradient.Colors.ShouldContain(DarkVibrant);
    }

    /// <summary>
    ///     Creates a 16x16 image, the smallest image the gradient can be taken from, filled with bands of the given colors
    /// </summary>
    private static SKBitmap CreateImage(SKColor[] colors)
    {
        SKBitmap bitmap = new(16, 16);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            bitmap.SetPixel(x, y, colors[(y * 16 + x) * colors.Length / 256]);
        return bitmap;
    }

    #endregion
}
