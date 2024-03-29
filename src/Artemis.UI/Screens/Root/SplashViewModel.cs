﻿using System;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Root;

public partial class SplashViewModel : ViewModelBase
{
    [Notify] private string _status;

    public SplashViewModel(ICoreService coreService, IPluginManagementService pluginManagementService)
    {
        CoreService = coreService;
        _status = "Initializing Core";

        pluginManagementService.CopyingBuildInPlugins += OnPluginManagementServiceOnCopyingBuildInPluginsManagement;
        pluginManagementService.PluginLoading += OnPluginManagementServiceOnPluginManagementLoading;
        pluginManagementService.PluginLoaded += OnPluginManagementServiceOnPluginManagementLoaded;
        pluginManagementService.PluginEnabling += PluginManagementServiceOnPluginManagementEnabling;
        pluginManagementService.PluginEnabled += PluginManagementServiceOnPluginManagementEnabled;
        pluginManagementService.PluginFeatureEnabling += PluginManagementServiceOnPluginFeatureEnabling;
        pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureEnabled;
    }

    public ICoreService CoreService { get; }
    
    private void OnPluginManagementServiceOnPluginManagementLoaded(object? sender, PluginEventArgs args)
    {
        Status = "Initializing UI";
    }

    private void OnPluginManagementServiceOnPluginManagementLoading(object? sender, PluginEventArgs args)
    {
        Status = "Loading plugin: " + args.Plugin.Info.Name;
    }

    private void PluginManagementServiceOnPluginManagementEnabled(object? sender, PluginEventArgs args)
    {
        Status = "Initializing UI";
    }

    private void PluginManagementServiceOnPluginManagementEnabling(object? sender, PluginEventArgs args)
    {
        Status = "Enabling plugin: " + args.Plugin.Info.Name;
    }

    private void PluginManagementServiceOnPluginFeatureEnabling(object? sender, PluginFeatureEventArgs e)
    {
        Status = "Enabling: " + e.PluginFeature.Info.Name;
    }

    private void PluginManagementServiceOnPluginFeatureEnabled(object? sender, PluginFeatureEventArgs e)
    {
        Status = "Initializing UI";
    }

    private void OnPluginManagementServiceOnCopyingBuildInPluginsManagement(object? sender, EventArgs args)
    {
        Status = "Updating built-in plugins";
    }
}