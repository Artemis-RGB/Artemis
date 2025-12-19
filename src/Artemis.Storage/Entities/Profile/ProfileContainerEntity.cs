using System;

namespace Artemis.Storage.Entities.Profile;

public class ProfileContainerEntity
{
    public Guid Id { get; set; }
    public byte[] Icon { get; set; } = [];
    
    public ProfileCategoryEntity ProfileCategory { get; set; } = null!;
    
    public ProfileConfigurationEntity ProfileConfiguration { get; set; } = new();
    public ProfileEntity Profile { get; set; } = new();
}