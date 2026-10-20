using Artemis.Core.Modules;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.Modules;

/// <summary>
///     A data model with a property of every kind data model paths, bindings and conditions deal with.
///     Tests set the values directly.
/// </summary>
public class TestDataModel : DataModel
{
    public int Integer { get; set; }
    public float Float { get; set; }
    public double Double { get; set; }
    public bool Boolean { get; set; }
    public string? Text { get; set; }
    public SKColor Color { get; set; }
    public TestEnum Enum { get; set; }

    /// <summary>
    ///     A nested data model, paths into it look like <c>Child.Integer</c>
    /// </summary>
    public TestChildDataModel Child { get; } = new();

    /// <summary>
    ///     A nested plain object rather than a data model
    /// </summary>
    public TestItem Item { get; set; } = new();

    public List<int> Integers { get; set; } = [];
    public List<TestItem> Items { get; set; } = [];

    /// <summary>
    ///     An event without arguments
    /// </summary>
    public DataModelEvent Event { get; } = new();

    /// <summary>
    ///     An event with arguments, conditions can read them through paths like <c>EventWithArgs.Value</c>
    /// </summary>
    public DataModelEvent<TestEventArgs> EventWithArgs { get; } = new();

    [DataModelIgnore]
    public int Ignored { get; set; }
}

public class TestChildDataModel : DataModel
{
    public int Integer { get; set; }
    public string? Text { get; set; }
}

public class TestItem
{
    public int Integer { get; set; }
    public string? Text { get; set; }
}

public class TestEventArgs : DataModelEventArgs
{
    public int Value { get; set; }
}

public enum TestEnum
{
    First,
    Second,
    Third
}
