using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.UI.Shared.Tests.Infrastructure;

/// <summary>
///     An always-on condition that remembers whether it was disposed
/// </summary>
public class TestCondition(RenderProfileElement profileElement) : ICondition
{
    public bool Disposed { get; private set; }

    public IConditionEntity Entity { get; } = new AlwaysOnConditionEntity();
    public RenderProfileElement ProfileElement { get; } = profileElement;
    public bool IsMet => true;

    public void Update()
    {
    }

    public void UpdateTimeline(double deltaTime)
    {
    }

    public void OverrideTimeline(TimeSpan position)
    {
    }

    public void Load()
    {
    }

    public void Save()
    {
    }

    public IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        return [];
    }

    public void Dispose()
    {
        Disposed = true;
    }
}
