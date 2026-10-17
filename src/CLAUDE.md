# Artemis

Artemis is a desktop app that adds highly configurable RGB lighting support for games/peripherals (RGB.NET-based). Cross-platform via Avalonia, with OS-specific host projects for Windows/Linux/macOS.

## Solution layout

- `Artemis.Core` — domain/business logic: profiles, plugins, devices, services (DI-registered), color science, layouts. No UI dependency.
- `Artemis.Storage` — EF Core + SQLite persistence (`ArtemisDbContext`), entities and repositories. `Artemis.Storage.Legacy` handles migration from the old storage format.
- `Artemis.UI.Shared` — reusable MVVM building blocks shared by the main UI: routing framework (`Routing/`), `ActivatableViewModelBase`, shared controls/behaviors/converters/styles.
- `Artemis.UI` — the actual application screens (`Screens/...`), organized by feature area (ProfileEditor, Workshop, SurfaceEditor, Settings, etc.). This is where almost all feature work happens.
- `Artemis.UI.Windows` / `Artemis.UI.Linux` / `Artemis.UI.MacOS` — thin OS-specific host/entry-point projects (WinExe etc.) that reference `Artemis.UI`. On this machine, `Artemis.UI.Windows` is the one that actually runs.
- `Artemis.WebClient.Workshop` / `Artemis.WebClient.Updating` — GraphQL/HTTP clients for the workshop (plugin/profile marketplace) and update checks.
- Plugins themselves live in a separate sibling repo, `Artemis.Plugins` — not needed for typical work in this repo and not expected to be cloned alongside it.

Target framework: `net10.0` (Windows host uses `net10.0-windows10.0.17763.0`). Nullable reference types enabled everywhere.

## Build & verify

- Build the whole solution to sanity-check changes: `dotnet build Artemis.sln` (or `Artemis.UI.csproj` while iterating, but always do a full solution build before considering a task done).
- There is no automated test suite in this solution (no `*.Tests` projects) — verification is: it builds, plus running the app manually for anything UI-visible (see `run` skill / launch `Artemis.UI.Windows`).
- Treat compiler warnings you introduce as worth fixing, but the codebase already has a long tail of pre-existing warnings (obsolete API usage, missing XML docs, etc.) — don't try to clean those up incidentally.

## MVVM conventions (ReactiveUI + DynamicData + Avalonia)

- View models use `PropertyChanged.SourceGenerator`: private backing fields annotated `[Notify]` (e.g. `[Notify] private ProfileConfiguration? _profileConfiguration;`) generate the public property — don't hand-write `INotifyPropertyChanged` boilerplate.
- Routable screens derive from `RoutableScreen` / `RoutableScreen<TParam>` (in `Artemis.UI.Shared.Routing`), which itself derives from `ActivatableViewModelBase`. Override `OnNavigating`/`OnClosing` for route lifecycle, not the constructor, for anything that depends on navigation parameters.
- The router (`Artemis.UI.Shared/Routing/Router/Router.cs`) recycles screen instances by default (`RecycleScreens = true`): navigating between two routes that resolve to the same view model type reuses the existing instance rather than constructing a new one. Don't assume a fresh instance per navigation — state must be reset/updated in `OnNavigating`, not just the constructor.
- Any `Transform`/`Bind`/subscription wiring on observable collections (DynamicData) or services goes inside `this.WhenActivated(d => { ... }).DisposeWith(d)`, even when the source is just one of the view model's own properties. This is the established convention throughout the codebase (see `ConfigurationSectionDesignViewModel`, `StatusBarViewModel`, `PropertiesViewModel`) — follow it for consistency and so activation/deactivation cleanup is automatic if the subscription later grows an external dependency.
- To react to a *changing* collection-valued property (e.g. rebinding a child list when the parent object changes), use `this.WhenAnyValue(vm => vm.Parent).Select(p => p?.Children.ToObservableChangeSet() ?? Observable.Empty<IChangeSet<T>>()).Switch()`. DynamicData's `Switch()` on an `IObservable<IObservable<IChangeSet<T>>>` clears the previously-bound results when the source switches — don't manually clear a `SourceList` in navigation code as a substitute for this.
- Prefer `SourceList<T>`/`SourceCache<T>` + `Connect()`/`ToObservableChangeSet()` + `.Transform(...).Bind(out ReadOnlyObservableCollection<TViewModel> x)` over manually maintaining `ObservableCollection<TViewModel>` in view models that wrap a model-level collection.
- DI is DryIoc (`Splat.DryIoc`); constructor injection throughout, view models are resolved via the container/`Func<TModel, TViewModel>` factories for per-item view model creation in `Transform`.

## Git

- Current default branch: `master`. Commit message style seen in history is short and imperative, often prefixed with the feature area, e.g. `Workshop - Fix misalignment in EntryTypes causing plugins to not load`, `Surface editor - Show model instead of device name`. Follow that `Area - Change` style when the change is scoped to one area; a plain imperative summary is fine otherwise.
