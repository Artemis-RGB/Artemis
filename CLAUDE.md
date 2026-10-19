# Artemis

Artemis is an RGB lighting app: C# on .NET 10, Avalonia for the UI, SkiaSharp for drawing and RGB.NET for talking to devices.
The solution is `src/Artemis.sln`. `Artemis.Core` holds profiles, layers, brushes, effects and the render pipeline, the
`Artemis.UI.*` projects are the app itself.

## Building and testing

```bash
dotnet build src/Artemis.sln
dotnet test --project src/Artemis.Core.Tests
```

The tests use xUnit v3 on Microsoft.Testing.Platform (switched on by `global.json` in the repo root), so use
`dotnet test --project ...` as shown above. The whole suite runs in a few seconds. To run part of it, pass a filter, for
example `dotnet test --project src/Artemis.Core.Tests --filter-class "*TimelineTests"`.

Pull requests to `master` run the tests on Windows and Linux (`.github/workflows/pr.yml`).

## Rules

- Changes to `Artemis.Core` come with tests in `Artemis.Core.Tests`: a test that fails without the change and passes
  with it for bug fixes, tests covering the new behaviour for features.
- `dotnet test --project src/Artemis.Core.Tests` must pass before you consider the work done.
- Tests run one at a time on purpose (see `TestAssembly.cs`), Artemis.Core keeps brush and effect registrations in
  static state that tests would otherwise share.

## The render harness

Most tests render real profiles through the same pipeline the app uses and check the colors that end up on each LED.

- `Infrastructure/Rendering/RenderHarness.cs` sets up fake devices from RGB.NET layout files, creates profiles, layers,
  folders and effects, and renders frames. No services or plugins are started and the harness controls time: a frame is
  only rendered when a test asks for one, after moving time forward by a given amount.
- `Infrastructure/Rendering/RenderTestBase.cs` is the base class for tests on a single ANSI keyboard. It gives you
  `Harness`, `Keyboard`, `Led(LedId)`, `Leds(...)`, `TopLetterRow`, and `Render(profile)` / `RenderTimeline(profile, ...)`
  which also take care of saving the render output described below. Tests that need other devices or render scales
  create their own `RenderHarness` (see `Rendering/SmallLedTests.cs`).
- The test brushes and effects under `Infrastructure/Plugins` are simple, predictable stand-ins for real plugins.
- Compare colors with `ShouldBeColor` (`Infrastructure/ColorAssertions.cs`), it allows a small difference per channel
  (2 by default) since drawing and sampling can be off by a little.

A typical test:

```csharp
public class LayerBrushTests : RenderTestBase
{
    [Fact]
    public void SolidBrush_DefaultsToItsDefaultValue()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }
}
```

### Render output

When a render test fails, what it rendered is written to
`src/Artemis.Core.Tests/render-output/<namespace>/<class>/<test>/`, for example
`render-output/Profiles/LayerBrushTests/SolidBrush_DefaultsToItsDefaultValue/`. Passing tests write nothing, unless you
set the environment variable `ARTEMIS_RENDER_OUTPUT=always`. The folder is ignored by git.

A single frame produces:

- `canvas.png`: the raw canvas the profile was drawn on
- `leds.png`: every LED drawn at its position, filled with the color it received
- `leds.json`: the color of every LED by LED ID, as hex (`#RRGGBB`, or `#AARRGGBB` when not fully opaque)

A timeline produces `canvas-strip.png`, `leds-strip.png` (all frames stacked top to bottom) and `timeline.json`.

When working out why a render test failed, read the JSON first: it holds the exact color each LED received, which is
more reliable than judging colors from the PNGs. Use the PNGs to get an idea of shapes and positions.

In CI the same files are attached to the test report, which is uploaded as the `test-results-<OS>` artifact of the run.

## Testing without rendering

Not everything needs the render harness. These pieces live under `src/Artemis.Core.Tests/Infrastructure`:

- `Plugins/TestPlugin.cs` hosts test plugin features (brushes, effects, modules, node providers) without plugin
  loading. `AddFeature` enables a feature the way Artemis does and disposing the plugin disables them again, which also
  removes their registrations from Core's static stores.
- `Plugins/Modules/TestModule.cs` is a module with a `TestDataModel`: numbers, text, a color, an enum, a nested data
  model, lists and data model events. Adding it to a `TestPlugin` registers the data model, so data model paths can find
  it both directly and when loaded from storage. See `DataModels/DataModelPathTests.cs`.
- `Plugins/Nodes/TestNodeProvider.cs` registers the test nodes (`TestBooleanNode`, `TestNumericNode`), so scripts using
  them can be saved and loaded. Create nodes with `AddNode<T>(script)` rather than `new`, otherwise they can't be loaded
  again. See `VisualScripting/NodeScriptTests.cs`.
- `Services/ServiceHarness.cs` registers the Artemis.Core services like the app does, backed by a new database in a
  temporary folder and logging to the test output. `Resolve<T>()` also works for protected services such as
  `ISettingsService`. Anything working on `Constants.DataFolder` or `Constants.PluginsFolder` directly (loading plugins,
  the web server) still points at your real Artemis folders, so don't call it. See `Services/ServiceHarnessTests.cs`.

Code coverage: `dotnet test --project src/Artemis.Core.Tests -- --coverage --coverage-output-format cobertura`.
