namespace Artemis.Storage.Legacy.Entities.Profile;

internal class KeyframeEntity
{
    public TimeSpan Position { get; set; }
    public int Timeline { get; set; }
    public string Value { get; set; } = string.Empty;
    public int EasingFunction { get; set; }
}