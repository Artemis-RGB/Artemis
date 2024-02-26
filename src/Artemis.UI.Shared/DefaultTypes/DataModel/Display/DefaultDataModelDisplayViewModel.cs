﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Artemis.Core;
using Artemis.UI.Shared.DataModelVisualization;

namespace Artemis.UI.Shared.DefaultTypes.DataModel.Display;

/// <summary>
///     Represents the default data model display view model that is used when no display viewmodel specific for the type
///     is registered
/// </summary>
internal class DefaultDataModelDisplayViewModel : DataModelDisplayViewModel<object>
{
    private readonly JsonSerializerOptions _serializerSettings;
    private string _display;

    public DefaultDataModelDisplayViewModel()
    {
        _serializerSettings = new JsonSerializerOptions() {ReferenceHandler = ReferenceHandler.IgnoreCycles};
        _display = "null";
    }

    public string Display
    {
        get => _display;
        set => RaiseAndSetIfChanged(ref _display, value);
    }

    protected override void OnDisplayValueUpdated()
    {
        if (DisplayValue is Enum enumDisplayValue)
            Display = EnumUtilities.HumanizeValue(enumDisplayValue);
        else if (DisplayValue is not string)
            Display = JsonSerializer.Serialize(DisplayValue, _serializerSettings);
        else
            Display = DisplayValue?.ToString() ?? "null";
    }
}