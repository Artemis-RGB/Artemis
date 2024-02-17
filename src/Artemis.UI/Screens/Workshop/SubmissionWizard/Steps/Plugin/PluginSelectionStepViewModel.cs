using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Plugin;

public partial class PluginSelectionStepViewModel : SubmissionViewModel
{
    private readonly IWindowService _windowService;
    [Notify] private PluginInfo? _selectedPlugin;
    [Notify] private string? _path;

    /// <inheritdoc />
    public PluginSelectionStepViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<EntryTypeStepViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.SelectedPlugin).Select(p => p != null));
        Browse = ReactiveCommand.CreateFromTask(ExecuteBrowse);

        this.WhenActivated((CompositeDisposable _) =>
        {
            ShowGoBack = State.EntryId == null;
            if (State.EntrySource is PluginEntrySource pluginEntrySource)
            {
                SelectedPlugin = pluginEntrySource.PluginInfo;
                Path = pluginEntrySource.Path;
            }
        });
    }

    public ReactiveCommand<Unit, Unit> Browse { get; }

    private async Task ExecuteBrowse()
    {
        string[]? files = await _windowService.CreateOpenFileDialog().HavingFilter(f => f.WithExtension("zip").WithName("ZIP files")).ShowAsync();
        if (files == null)
            return;

        // Find the metadata file in the zip
        using ZipArchive archive = ZipFile.OpenRead(files[0]);
        ZipArchiveEntry? metaDataFileEntry = archive.Entries.FirstOrDefault(e => e.Name == "plugin.json");
        if (metaDataFileEntry == null)
            throw new ArtemisPluginException("Couldn't find a plugin.json in " + files[0]);

        using StreamReader reader = new(metaDataFileEntry.Open());
        PluginInfo pluginInfo = CoreJson.DeserializeObject<PluginInfo>(reader.ReadToEnd())!;
        if (!pluginInfo.Main.EndsWith(".dll"))
            throw new ArtemisPluginException("Main entry in plugin.json must point to a .dll file");

        SelectedPlugin = pluginInfo;
        Path = files[0];
    }

    private void ExecuteContinue()
    {
        if (SelectedPlugin == null || Path == null)
            return;

        State.EntrySource = new PluginEntrySource(SelectedPlugin, Path);
        
        if (string.IsNullOrWhiteSpace(State.Name))
            State.Name = SelectedPlugin.Name;
        if (string.IsNullOrWhiteSpace(State.Summary))
            State.Summary = SelectedPlugin.Description ?? "";
        
        if (State.EntryId == null)
            State.ChangeScreen<SpecificationsStepViewModel>();
        else
            State.ChangeScreen<UploadStepViewModel>();
    }
}