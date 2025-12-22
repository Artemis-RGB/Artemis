using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.DryIoc.Factories;

internal class PluginSettingsFactory : IPluginSettingsFactory
{
    private static readonly List<PluginSettings> PluginSettings = [];
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IPluginRepository _pluginRepository;

    public PluginSettingsFactory(IPluginRepository pluginRepository, IPluginManagementService pluginManagementService)
    {
        _pluginRepository = pluginRepository;
        _pluginManagementService = pluginManagementService;
    }

    public PluginSettings CreatePluginSettings(Type type)
    {
        Plugin? plugin = _pluginManagementService.GetPluginByAssembly(type.Assembly);

        if (plugin == null)
            throw new ArtemisCoreException("PluginSettings can only be injected with the PluginInfo parameter provided " +
                                           "or into a class defined in a plugin assembly");

        lock (PluginSettings)
        {
            PluginSettings? existingSettings = PluginSettings.FirstOrDefault(p => p.Plugin == plugin);
            if (existingSettings != null)
                return existingSettings;

            PluginSettings? settings = new(plugin, _pluginRepository);
            PluginSettings.Add(settings);
            return settings;
        }
    }
}

internal interface IPluginSettingsFactory
{
    PluginSettings CreatePluginSettings(Type type);
}