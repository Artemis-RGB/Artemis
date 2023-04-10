using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using Avalonia.Threading;
using DynamicData;
using DynamicData.List;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptWindowViewModel : NodeScriptWindowViewModelBase
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly INodeService _nodeService;
    private readonly IProfileService _profileService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private bool _pauseUpdate;

    public NodeScriptWindowViewModel(NodeScript nodeScript,
        INodeService nodeService,
        INodeEditorService nodeEditorService,
        INodeVmFactory vmFactory,
        ISettingsService settingsService,
        IProfileService profileService,
        IWindowService windowService) : base(nodeScript)
    {
        NodeScript = nodeScript;
        NodeScriptViewModel = vmFactory.NodeScriptViewModel(NodeScript, false);
        OpenUri = ReactiveCommand.Create<string>(s => Process.Start(new ProcessStartInfo(s) {UseShellExecute = true, Verb = "open"}));
        ToggleBooleanSetting = ReactiveCommand.Create<PluginSetting<bool>>(ExecuteToggleBooleanSetting);
        History = nodeEditorService.GetHistory(nodeScript);

        _nodeService = nodeService;
        _nodeEditorService = nodeEditorService;
        _settingsService = settingsService;
        _profileService = profileService;
        _windowService = windowService;

        CreateNode = ReactiveCommand.Create<NodeData>(ExecuteCreateNode);
        AutoArrange = ReactiveCommand.CreateFromTask(ExecuteAutoArrange);
        Export = ReactiveCommand.CreateFromTask(ExecuteExport);
        Import = ReactiveCommand.CreateFromTask(ExecuteImport);

        SourceList<NodeData> nodeSourceList = new();
        nodeSourceList.AddRange(nodeService.AvailableNodes);
        nodeSourceList.Connect()
            .GroupWithImmutableState(n => n.Category)
            .Transform(c => new NodeMenuItemViewModel(CreateNode, c))
            .Bind(out ReadOnlyObservableCollection<NodeMenuItemViewModel> categories)
            .Subscribe();
        Categories = categories;

        this.WhenActivated(d =>
        {
            DispatcherTimer updateTimer = new(TimeSpan.FromMilliseconds(25.0 / 1000), DispatcherPriority.Normal, Update);
            // TODO: Remove in favor of saving each time a node editor command is executed
            DispatcherTimer saveTimer = new(TimeSpan.FromMinutes(2), DispatcherPriority.Normal, Save);

            updateTimer.Start();
            saveTimer.Start();

            Disposable.Create(() =>
            {
                updateTimer.Stop();
                saveTimer.Stop();
            }).DisposeWith(d);
        });
    }

    public NodeScriptViewModel NodeScriptViewModel { get; set; }

    public NodeEditorHistory History { get; }
    public ReactiveCommand<PluginSetting<bool>, Unit> ToggleBooleanSetting { get; set; }
    public ReactiveCommand<string, Unit> OpenUri { get; set; }
    public ReadOnlyObservableCollection<NodeMenuItemViewModel> Categories { get; }
    public ReactiveCommand<NodeData, Unit> CreateNode { get; }
    public ReactiveCommand<Unit, Unit> AutoArrange { get; }
    public ReactiveCommand<Unit, Unit> Export { get; }
    public ReactiveCommand<Unit, Unit> Import { get; }

    public PluginSetting<bool> ShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", false);
    public PluginSetting<bool> AlwaysShowValues => _settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);

    private void ExecuteToggleBooleanSetting(PluginSetting<bool> setting)
    {
        setting.Value = !setting.Value;
        setting.Save();
    }

    private void ExecuteCreateNode(NodeData data)
    {
        // Place the node in the top-left of the canvas, keeping in mind the user may have panned
        INode node = data.CreateNode(NodeScript, null);
        Point point = new Point(0, 0).Transform(NodeScriptViewModel.PanMatrix.Invert());

        node.X = Math.Round(point.X / 10d, 0, MidpointRounding.AwayFromZero) * 10d + 20;
        node.Y = Math.Round(point.Y / 10d, 0, MidpointRounding.AwayFromZero) * 10d + 20;

        _nodeEditorService.ExecuteCommand(NodeScript, new AddNode(NodeScript, node));
    }

    private async Task ExecuteAutoArrange()
    {
        try
        {
            if (!NodeScript.ExitNodeConnected)
            {
                await _windowService.ShowConfirmContentDialog("Cannot auto-arrange", "The exit node must be connected in order to perform auto-arrange.", "Close", null);
                return;
            }

            _pauseUpdate = true;
            _nodeEditorService.ExecuteCommand(NodeScript, new OrganizeScript(NodeScript));
            await Task.Delay(200);
            NodeScriptViewModel.RequestAutoFit();
        }
        finally
        {
            _pauseUpdate = false;
        }
    }

    private async Task ExecuteExport()
    {
        // Might not cover everything but then the dialog will complain and that's good enough
        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("Artemis node script"))
            .ShowAsync();

        if (result == null)
            return;

        string json = _nodeService.ExportScript(NodeScript);
        await File.WriteAllTextAsync(result, json);
    }

    private async Task ExecuteImport()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("Artemis node script"))
            .ShowAsync();

        if (result == null)
            return;

        try
        {
            _pauseUpdate = true;
            string json = await File.ReadAllTextAsync(result[0]);
            _nodeService.ImportScript(json, NodeScript);
            History.Clear();

            await Task.Delay(200);
            NodeScriptViewModel.RequestAutoFit();
        }
        finally
        {
            _pauseUpdate = false;
        }
    }

    private void Update(object? sender, EventArgs e)
    {
        if (!_pauseUpdate)
            NodeScript.Run();
    }

    private void Save(object? sender, EventArgs e)
    {
        if (NodeScript.Context is Profile profile)
            _profileService.SaveProfile(profile, true);
    }
}