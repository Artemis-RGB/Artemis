using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.PropertyInput;

/// <summary>
///     Represents a property input registration registered through
///     <see cref="IPropertyInputService.RegisterPropertyInput" />
/// </summary>
public class PropertyInputRegistration
{
    private readonly IPropertyInputService _propertyInputService;

    internal PropertyInputRegistration(IPropertyInputService propertyInputService, Plugin plugin, Type supportedType, Type viewModelType)
    {
        _propertyInputService = propertyInputService;
        Plugin = plugin;
        SupportedType = supportedType;
        ViewModelType = viewModelType;

        if (Plugin != Constants.CorePlugin)
            Plugin.Disabled += InstanceOnDisabled;
    }

    /// <summary>
    ///     Gets the plugin that registered the property input
    /// </summary>
    public Plugin Plugin { get; }

    /// <summary>
    ///     Gets the type supported by the property input
    /// </summary>
    public Type SupportedType { get; }

    /// <summary>
    ///     Gets the view model type of the property input
    /// </summary>
    public Type ViewModelType { get; }

    internal void Unsubscribe()
    {
        if (Plugin != Constants.CorePlugin)
            Plugin.Disabled -= InstanceOnDisabled;
    }

    private void InstanceOnDisabled(object? sender, EventArgs e)
    {
        // Profile editor service will call Unsubscribe
        _propertyInputService.RemovePropertyInput(this);
    }
}