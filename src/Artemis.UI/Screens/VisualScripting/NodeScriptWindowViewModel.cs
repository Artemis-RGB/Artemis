using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Threading;
using DynamicData;
using DynamicData.List;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptWindowViewModel : DialogViewModelBase<bool>
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly INodeService _nodeService;
    private readonly ISettingsService _settingsService;
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;

    public NodeScriptWindowViewModel(NodeScript nodeScript,
        INodeService nodeService,
        INodeEditorService nodeEditorService,
        INodeVmFactory vmFactory,
        ISettingsService settingsService,
        IProfileService profileService,
        IWindowService windowService)
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

        SourceList<NodeData> nodeSourceList = new();
        nodeSourceList.AddRange(nodeService.AvailableNodes);
        nodeSourceList.Connect()
            .GroupWithImmutableState(n => n.Category)
            .Bind(out ReadOnlyObservableCollection<IGrouping<NodeData, string>> categories)
            .Subscribe();
        Categories = categories;

        CreateNode = ReactiveCommand.Create<NodeData>(ExecuteCreateNode);
        Export = ReactiveCommand.CreateFromTask(ExecuteExport);
        Import = ReactiveCommand.CreateFromTask(ExecuteImport);

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

    public NodeScript NodeScript { get; }
    public NodeScriptViewModel NodeScriptViewModel { get; set; }

    public NodeEditorHistory History { get; }
    public ReactiveCommand<PluginSetting<bool>, Unit> ToggleBooleanSetting { get; set; }
    public ReactiveCommand<string, Unit> OpenUri { get; set; }
    public ReadOnlyObservableCollection<IGrouping<NodeData, string>> Categories { get; }
    public ReactiveCommand<NodeData, Unit> CreateNode { get; }
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

        string json = await File.ReadAllTextAsync(result[0]);
        _nodeService.ImportScript(json, NodeScript);
        History.Clear();

        await Task.Delay(200);
        NodeScriptViewModel.RequestAutoFit();
    }

    private void Update(object? sender, EventArgs e)
    {
        NodeScript.Run();
    }

    private void Save(object? sender, EventArgs e)
    {
        if (NodeScript.Context is Profile profile)
            _profileService.SaveProfile(profile, true);
    }
}