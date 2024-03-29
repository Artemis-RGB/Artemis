﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Prerequisites;

public partial class PluginPrerequisiteViewModel : ActivatableViewModelBase
{
    private readonly ObservableAsPropertyHelper<int> _activeStepNumber;
    private readonly ObservableAsPropertyHelper<bool> _busy;
    private readonly bool _uninstall;
    [Notify] private PluginPrerequisiteActionViewModel? _activeAction;
    [Notify] private bool _installing;
    [Notify] private bool _isMet;
    [Notify] private bool _uninstalling;

    public PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall)
    {
        _uninstall = uninstall;

        PluginPrerequisite = pluginPrerequisite;
        Actions = new ObservableCollection<PluginPrerequisiteActionViewModel>(!_uninstall
            ? PluginPrerequisite.InstallActions.Select(a => new PluginPrerequisiteActionViewModel(a))
            : PluginPrerequisite.UninstallActions.Select(a => new PluginPrerequisiteActionViewModel(a)));

        _busy = this.WhenAnyValue(x => x.Installing, x => x.Uninstalling, (i, u) => i || u).ToProperty(this, x => x.Busy);
        _activeStepNumber = this.WhenAnyValue(x => x.ActiveAction, a => Actions.IndexOf(a!) + 1).ToProperty(this, x => x.ActiveStepNumber);

        this.WhenActivated(d =>
        {
            PluginPrerequisite.PropertyChanged += PluginPrerequisiteOnPropertyChanged;
            Disposable.Create(() => PluginPrerequisite.PropertyChanged -= PluginPrerequisiteOnPropertyChanged).DisposeWith(d);
        });

        // Could be slow so take it off of the UI thread
        Task.Run(() => IsMet = PluginPrerequisite.IsMet());
    }

    public ObservableCollection<PluginPrerequisiteActionViewModel> Actions { get; }
    public PluginPrerequisite PluginPrerequisite { get; }
    public bool Busy => _busy.Value;
    public int ActiveStepNumber => _activeStepNumber.Value;

    public async Task Install(CancellationToken cancellationToken)
    {
        if (Busy)
            return;

        Installing = true;
        try
        {
            await PluginPrerequisite.Install(cancellationToken);
        }
        finally
        {
            Installing = false;
            IsMet = PluginPrerequisite.IsMet();
        }
    }

    public async Task Uninstall(CancellationToken cancellationToken)
    {
        if (Busy)
            return;

        Uninstalling = true;
        try
        {
            await PluginPrerequisite.Uninstall(cancellationToken);
        }
        finally
        {
            Uninstalling = false;
            IsMet = PluginPrerequisite.IsMet();
        }
    }

    private void PluginPrerequisiteOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PluginPrerequisite.CurrentAction))
            ActivateCurrentAction();
    }

    private void ActivateCurrentAction()
    {
        PluginPrerequisiteActionViewModel? activeAction = Actions.FirstOrDefault(i => i.Action == PluginPrerequisite.CurrentAction);
        if (activeAction == null)
            return;

        ActiveAction = activeAction;
    }
}