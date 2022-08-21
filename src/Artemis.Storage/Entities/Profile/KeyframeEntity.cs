using System;

namespace Artemis.Storage.Entities.Profile;

public class KeyframeEntity
{
    public TimeSpan Position { get; set; }
    public int Timeline { get; set; }
    public string Value { get; set; }
    public int EasingFunction { get; set; }
}