using System;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile;

public class FolderEntity : RenderElementEntity
{
    public int Order { get; set; }
    public string? Name { get; set; }
    public bool IsExpanded { get; set; }
    public bool Suspended { get; set; }

    public Guid ProfileId { get; set; }
}