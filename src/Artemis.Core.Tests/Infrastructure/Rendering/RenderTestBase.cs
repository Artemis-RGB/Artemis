using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using RGB.NET.Core;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     Base class for tests that render onto a single ANSI keyboard
/// </summary>
public abstract class RenderTestBase : IDisposable
{
    private readonly List<IDisposable> _tracked = [];
    private readonly List<(IDisposable Copy, Action Save)> _pendingOutput = [];

    protected RenderTestBase()
    {
        Harness = new RenderHarness();
        Keyboard = Harness.AddDevice(TestLayouts.KeyboardAnsi);
    }

    protected RenderHarness Harness { get; }
    protected ArtemisDevice Keyboard { get; }

    /// <summary>
    ///     Gets the name of the running test, useful as the name of saved render output
    /// </summary>
    protected static string TestName => TestContext.Current.TestCase?.TestMethodName ?? "Unknown";

    /// <summary>
    ///     Gets the keyboard LED with the given ID
    /// </summary>
    protected ArtemisLed Led(LedId ledId)
    {
        return Keyboard.GetLed(ledId, false) ?? throw new ArgumentException($"The keyboard has no LED {ledId}", nameof(ledId));
    }

    /// <summary>
    ///     Gets the keyboard LEDs with the given IDs
    /// </summary>
    protected ArtemisLed[] Leds(params LedId[] ledIds)
    {
        return ledIds.Select(Led).ToArray();
    }

    /// <summary>
    ///     Gets the top letter row, Q through P, ordered left to right
    /// </summary>
    protected ArtemisLed[] TopLetterRow => Leds(
        LedId.Keyboard_Q, LedId.Keyboard_W, LedId.Keyboard_E, LedId.Keyboard_R, LedId.Keyboard_T,
        LedId.Keyboard_Y, LedId.Keyboard_U, LedId.Keyboard_I, LedId.Keyboard_O, LedId.Keyboard_P
    );

    /// <summary>
    ///     Advances the profile by the given amount of seconds and renders a frame, see <see cref="SaveOutput(RenderFrame, string?)" />.
    /// </summary>
    protected RenderFrame Render(Profile profile, double seconds = 0.1)
    {
        RenderFrame frame = Harness.Render(profile, TimeSpan.FromSeconds(seconds));
        SaveOutput(frame);
        return frame;
    }

    /// <summary>
    ///     Renders the profile at each of the given timestamps (in seconds), see <see cref="SaveOutput(RenderTimeline, string?)" />.
    /// </summary>
    protected RenderTimeline RenderTimeline(Profile profile, params double[] seconds)
    {
        RenderTimeline timeline = Harness.RenderTimeline(profile, seconds.Select(TimeSpan.FromSeconds).ToArray());
        SaveOutput(timeline);
        return timeline;
    }

    /// <summary>
    ///     Saves the frame as render output once the test finished, but only if the test failed or
    ///     <see cref="RenderOutput.AlwaysWrite" /> is enabled. Writing the output takes far longer than rendering.
    /// </summary>
    /// <param name="frame">The frame to save, may be disposed by the test</param>
    /// <param name="name">The name to save the frame under, defaults to the name of the test</param>
    protected void SaveOutput(RenderFrame frame, string? name = null)
    {
        string outputName = name ?? TestName;
        RenderFrame copy = frame.Copy();
        _pendingOutput.Add((copy, () => copy.Save(outputName)));
    }

    /// <inheritdoc cref="SaveOutput(RenderFrame, string?)" />
    protected void SaveOutput(RenderTimeline timeline, string? name = null)
    {
        string outputName = name ?? TestName;
        RenderTimeline copy = timeline.Copy();
        _pendingOutput.Add((copy, () => copy.Save(outputName)));
    }

    /// <summary>
    ///     Disposes the provided object at the end of the test
    /// </summary>
    protected T Track<T>(T disposable) where T : IDisposable
    {
        _tracked.Add(disposable);
        return disposable;
    }

    public void Dispose()
    {
        // The result of the test is known by the time the test class is disposed
        bool writeOutput = RenderOutput.AlwaysWrite || TestContext.Current.TestState?.Result == TestResult.Failed;
        foreach ((IDisposable copy, Action save) in _pendingOutput)
        {
            if (writeOutput)
                save();
            copy.Dispose();
        }

        foreach (IDisposable disposable in _tracked)
            disposable.Dispose();
        Harness.Dispose();
        GC.SuppressFinalize(this);
    }
}
