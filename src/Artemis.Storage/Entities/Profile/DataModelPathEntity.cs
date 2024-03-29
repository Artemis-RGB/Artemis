﻿namespace Artemis.Storage.Entities.Profile;

public class DataModelPathEntity
{
    public string Path { get; set; } = string.Empty;
    public string? DataModelId { get; set; }
    public string? Type { get; set; }
}