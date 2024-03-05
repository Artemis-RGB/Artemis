using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile;

public class ProfileCategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsCollapsed { get; set; }
    public bool IsSuspended { get; set; }
    public int Order { get; set; }

    public List<ProfileContainerEntity> ProfileConfigurations { get; set; } = new();
}

public class ProfileContainerEntity
{
    public byte[] Icon { get; set; } = Array.Empty<byte>();
    public ProfileConfigurationEntity ProfileConfiguration { get; set; } = new();
    public ProfileEntity Profile { get; set; } = new();
}