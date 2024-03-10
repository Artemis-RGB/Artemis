using System;

namespace Artemis.Storage.Entities;

internal class RawProfileContainer
{
    public Guid Id { get; set; }
    public string ProfileConfiguration { get; set; }
    public string Profile { get; set; }
}