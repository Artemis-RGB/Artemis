using Artemis.Core.Internal;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     Base class for visual scripting tests. Registers the test nodes of both <see cref="TestNodeProvider" /> and
///     <see cref="ScriptTestNodeProvider" /> and disposes the scripts a test creates, since scripts listen to the static
///     node type store until they are disposed.
/// </summary>
public abstract class ScriptTestBase : IDisposable
{
    private readonly List<IDisposable> _tracked = [];

    protected ScriptTestBase()
    {
        Plugin = new TestPlugin();
        Nodes = Plugin.AddFeature(new TestNodeProvider());
        ScriptNodes = Plugin.AddFeature(new ScriptTestNodeProvider());
    }

    protected TestPlugin Plugin { get; }
    protected TestNodeProvider Nodes { get; }
    protected ScriptTestNodeProvider ScriptNodes { get; }

    /// <summary>
    ///     Creates a new script that is disposed at the end of the test
    /// </summary>
    protected NodeScript<T> CreateScript<T>(string name = "Test", List<DefaultNode>? defaultNodes = null)
    {
        return Track(new NodeScript<T>(name, "A test script", null, defaultNodes));
    }

    /// <summary>
    ///     Saves the script and loads a new script from a copy of what was saved, the way a profile is stored and loaded
    ///     again. The new script is disposed at the end of the test.
    /// </summary>
    protected NodeScript<T> SaveAndLoad<T>(NodeScript<T> script, List<DefaultNode>? defaultNodes = null)
    {
        script.Save();
        return Load<T>(script.Entity, script.Name, defaultNodes);
    }

    /// <summary>
    ///     Loads a new script from a copy of the provided entity. The new script is disposed at the end of the test.
    /// </summary>
    protected NodeScript<T> Load<T>(NodeScriptEntity entity, string name = "Test", List<DefaultNode>? defaultNodes = null)
    {
        return Track(new NodeScript<T>(name, "A test script", Copy(entity), null, defaultNodes));
    }

    /// <summary>
    ///     Copies the entity through JSON, so a loaded script never shares the entity of the script that saved it
    /// </summary>
    protected static NodeScriptEntity Copy(NodeScriptEntity entity)
    {
        return CoreJson.Deserialize<NodeScriptEntity>(CoreJson.Serialize(entity))!;
    }

    protected T Track<T>(T disposable) where T : IDisposable
    {
        _tracked.Add(disposable);
        return disposable;
    }

    public virtual void Dispose()
    {
        foreach (IDisposable disposable in _tracked)
            disposable.Dispose();
        Plugin.Dispose();
    }
}

internal static class ScriptTestExtensions
{
    /// <summary>
    ///     Gets the exit node of the script, the node whose input becomes the result of the script
    /// </summary>
    public static ExitNode<T> GetExitNode<T>(this NodeScript<T> script)
    {
        return script.Nodes.OfType<ExitNode<T>>().Single();
    }
}
