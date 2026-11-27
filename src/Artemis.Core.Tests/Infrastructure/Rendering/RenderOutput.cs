using System.Collections.Concurrent;
using System.Globalization;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     Writes render results to disk and attaches them to the test result so they can be inspected by humans and LLMs.
/// </summary>
internal static class RenderOutput
{
    private const float LedViewScale = 3;
    private const int StripHeaderHeight = 24;

    private static readonly ConcurrentDictionary<string, int> SaveCounts = [];

    /// <summary>
    ///     Gets the root folder all output is written to, <c>render-output</c> in the test project folder
    /// </summary>
    public static string Root { get; } = Path.Combine(FindProjectDirectory(), "render-output");

    /// <summary>
    ///     Gets a boolean indicating whether tests should write their render output even when they pass, enabled by setting
    ///     the <c>ARTEMIS_RENDER_OUTPUT</c> environment variable to <c>always</c>
    /// </summary>
    public static bool AlwaysWrite { get; } = string.Equals(Environment.GetEnvironmentVariable("ARTEMIS_RENDER_OUTPUT"), "always", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Starts a save under <c>render-output/&lt;test namespace&gt;/&lt;test class&gt;/&lt;name&gt;</c>, replacing previous
    ///     output of the same name.
    /// </summary>
    public static RenderOutputWriter Begin(string name)
    {
        // When a test saves under the same name more than once, number the later saves so they don't overwrite the first
        int count = SaveCounts.AddOrUpdate($"{TestContext.Current.Test?.UniqueID}/{name}", 1, (_, c) => c + 1);
        if (count > 1)
            name = $"{name}_{count}";

        string relativePath = Path.Combine(GetTestClassPath(), Sanitize(name));
        string directory = Path.Combine(Root, relativePath);
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);

        return new RenderOutputWriter(directory, relativePath.Replace(Path.DirectorySeparatorChar, '/'));
    }

    /// <summary>
    ///     Draws every LED at its position on the surface, filled with the color it received and labelled with its ID
    /// </summary>
    public static SKBitmap DrawLeds(RenderFrame frame)
    {
        SKRect surface = frame.LedColors.Keys.Select(l => l.AbsoluteRectangle).Aggregate(SKRect.Union);
        SKBitmap bitmap = new((int) Math.Ceiling(surface.Right * LedViewScale) + 1, (int) Math.Ceiling(surface.Bottom * LedViewScale) + 1);
        using SKCanvas canvas = new(bitmap);
        canvas.Clear(new SKColor(32, 32, 32));

        using SKPaint fill = new() {IsAntialias = true};
        using SKPaint border = new() {IsAntialias = true, Style = SKPaintStyle.Stroke, Color = new SKColor(80, 80, 80)};
        using SKPaint text = new() {IsAntialias = true};
        using SKFont font = new(SKTypeface.Default, 10);

        foreach ((ArtemisLed led, SKColor color) in frame.LedColors)
        {
            SKRect rect = new(
                led.AbsoluteRectangle.Left * LedViewScale + 1,
                led.AbsoluteRectangle.Top * LedViewScale + 1,
                led.AbsoluteRectangle.Right * LedViewScale - 1,
                led.AbsoluteRectangle.Bottom * LedViewScale - 1
            );
            fill.Color = color.WithAlpha(255);
            canvas.DrawRoundRect(rect, 3, 3, fill);
            canvas.DrawRoundRect(rect, 3, 3, border);

            // Pick a label color that stays readable on the LED color
            text.Color = color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114 > 128 ? SKColors.Black : SKColors.White;
            canvas.DrawText(GetLabel(led), rect.Left + 3, rect.Top + font.Size + 2, font, text);
        }

        return bitmap;
    }

    /// <summary>
    ///     Stacks a bitmap per frame top to bottom, each with a header showing the frame time
    /// </summary>
    public static SKBitmap DrawStrip(IReadOnlyList<RenderFrame> frames, Func<RenderFrame, SKBitmap> draw)
    {
        List<SKBitmap> bitmaps = frames.Select(draw).ToList();
        try
        {
            SKBitmap strip = new(bitmaps.Max(b => b.Width), bitmaps.Sum(b => b.Height + StripHeaderHeight));
            using SKCanvas canvas = new(strip);
            canvas.Clear(SKColors.Black);

            using SKPaint text = new() {IsAntialias = true, Color = SKColors.White};
            using SKFont font = new(SKTypeface.Default, 14);
            float y = 0;
            for (int i = 0; i < frames.Count; i++)
            {
                canvas.DrawText(string.Create(CultureInfo.InvariantCulture, $"t = {frames[i].Time.TotalSeconds:0.000}s"), 4, y + 17, font, text);
                canvas.DrawBitmap(bitmaps[i], 0, y + StripHeaderHeight);
                y += bitmaps[i].Height + StripHeaderHeight;
            }

            return strip;
        }
        finally
        {
            foreach (SKBitmap bitmap in bitmaps)
                bitmap.Dispose();
        }
    }

    public static object ToJson(RenderFrame frame)
    {
        return new
        {
            time = frame.Time.TotalSeconds,
            devices = frame.LedColors
                .GroupBy(l => l.Key.Device)
                .Select(d => new
                {
                    device = d.Key.RgbDevice.DeviceInfo.DeviceName,
                    leds = d.ToDictionary(l => l.Key.RgbLed.Id.ToString(), l => ToHex(l.Value))
                })
        };
    }

    private static string ToHex(SKColor color)
    {
        return color.Alpha == 255
            ? $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}"
            : $"#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    private static string GetLabel(ArtemisLed led)
    {
        string id = led.RgbLed.Id.ToString();
        int separator = id.IndexOf('_');
        return separator >= 0 ? id[(separator + 1)..] : id;
    }

    /// <summary>
    ///     Gets the path of the running test class relative to the root namespace, e.g. <c>Profiles/TimelineTests</c>
    /// </summary>
    private static string GetTestClassPath()
    {
        string? className = TestContext.Current.TestClass?.TestClassName;
        if (className == null)
            return string.Empty;

        string rootNamespace = typeof(RenderOutput).Assembly.GetName().Name + ".";
        if (className.StartsWith(rootNamespace))
            className = className[rootNamespace.Length..];

        return Path.Combine(className.Split('.').Select(Sanitize).ToArray());
    }

    private static string Sanitize(string name)
    {
        return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
    }

    private static string FindProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Artemis.Core.Tests.csproj")))
            directory = directory.Parent;
        return directory?.FullName ?? AppContext.BaseDirectory;
    }
}
