using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents a path that points to a property in data model
/// </summary>
public class DataModelPath : IStorageModel, IDisposable, IPluginFeatureDependent
{
    private readonly LinkedList<DataModelPathSegment> _segments;
    private Expression<Func<object, object>>? _accessorLambda;
    private bool _disposed;

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelPath" /> class pointing directly to the target
    /// </summary>
    /// <param name="target">The target at which this path starts</param>
    public DataModelPath(DataModel target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Path = "";
        Entity = new DataModelPathEntity();

        _segments = [];

        Save();
        Initialize();
        SubscribeToDataModelStore();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelPath" /> class pointing to the provided path
    /// </summary>
    /// <param name="target">The target at which this path starts</param>
    /// <param name="path">A point-separated path</param>
    public DataModelPath(DataModel target, string path)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Entity = new DataModelPathEntity();

        _segments = [];

        Save();
        Initialize();
        SubscribeToDataModelStore();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelPath" /> class based on an existing path
    /// </summary>
    /// <param name="dataModelPath">The path to base the new instance on</param>
    public DataModelPath(DataModelPath dataModelPath)
    {
        if (dataModelPath == null)
            throw new ArgumentNullException(nameof(dataModelPath));

        Target = dataModelPath.Target;
        Path = dataModelPath.Path;
        Entity = new DataModelPathEntity();

        _segments = [];

        Save();
        Initialize();
        SubscribeToDataModelStore();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelPath" /> class based on a <see cref="DataModelPathEntity" />
    /// </summary>
    /// <param name="entity"></param>
    public DataModelPath(DataModelPathEntity entity)
    {
        Path = entity.Path;
        Entity = entity;

        _segments = [];

        Load();
        Initialize();
        SubscribeToDataModelStore();
    }

    /// <summary>
    ///     Gets the data model at which this path starts
    /// </summary>
    public DataModel? Target { get; private set; }

    /// <summary>
    ///     Gets the data model ID of the <see cref="Target" /> if it is a <see cref="DataModel" />
    /// </summary>
    public string? DataModelId => Target?.Module.Id;

    /// <summary>
    ///     Gets the point-separated path associated with this <see cref="DataModelPath" />
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    ///     Gets a boolean indicating whether all <see cref="Segments" /> are valid
    /// </summary>
    public bool IsValid => Segments.Any() && Segments.All(p => p.Type != DataModelPathSegmentType.Invalid);

    /// <summary>
    ///     Gets a read-only list of all segments of this path
    /// </summary>
    public IReadOnlyCollection<DataModelPathSegment> Segments => _segments.ToList().AsReadOnly();

    /// <summary>
    ///     Gets the entity used for persistent storage
    /// </summary>
    public DataModelPathEntity Entity { get; }

    internal Func<object, object>? Accessor { get; private set; }

    /// <summary>
    ///     Gets the current value of the path
    /// </summary>
    public object? GetValue()
    {
        if (_disposed)
            throw new ObjectDisposedException("DataModelPath");

        if (_accessorLambda == null || Target == null)
            return null;

        // If the accessor has not yet been compiled do it now that it's first required
        if (Accessor == null)
            Accessor = _accessorLambda.Compile();
        return Accessor(Target);
    }

    /// <summary>
    ///     Gets the property info of the property this path points to
    /// </summary>
    /// <returns>If static, the property info. If dynamic, <c>null</c></returns>
    public PropertyInfo? GetPropertyInfo()
    {
        if (_disposed)
            throw new ObjectDisposedException("DataModelPath");

        return Segments.LastOrDefault()?.GetPropertyInfo();
    }

    /// <summary>
    ///     Gets the type of the property this path points to
    /// </summary>
    /// <returns>If possible, the property type</returns>
    public Type? GetPropertyType()
    {
        if (_disposed)
            throw new ObjectDisposedException("DataModelPath");

        // Prefer the actual type from the segments
        Type? segmentType = Segments.LastOrDefault()?.GetPropertyType();
        if (segmentType != null)
            return segmentType;
        
        // Fall back to stored type
        if (!string.IsNullOrWhiteSpace(Entity.Type))
            return Type.GetType(Entity.Type);

        return null;
    }

    /// <summary>
    ///     Gets the property description of the property this path points to
    /// </summary>
    /// <returns>If found, the data model property description</returns>
    public DataModelPropertyAttribute? GetPropertyDescription()
    {
        if (_disposed)
            throw new ObjectDisposedException("DataModelPath");

        return Segments.LastOrDefault()?.GetPropertyDescription();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Path) ? "this" : Path;
    }

    /// <inheritdoc />
    public IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        if (Target == null)
            return [];
        return [Target.Module];
    }

    /// <summary>
    ///     Occurs whenever the path becomes invalid
    /// </summary>
    public event EventHandler? PathInvalidated;

    /// <summary>
    ///     Occurs whenever the path becomes valid
    /// </summary>
    public event EventHandler? PathValidated;

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposed = true;

            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;

            Invalidate();
        }
    }

    /// <summary>
    ///     Invokes the <see cref="PathInvalidated" /> event
    /// </summary>
    protected virtual void OnPathValidated()
    {
        PathValidated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invokes the <see cref="PathValidated" /> event
    /// </summary>
    protected virtual void OnPathInvalidated()
    {
        PathInvalidated?.Invoke(this, EventArgs.Empty);
    }

    internal void Invalidate()
    {
        Target?.RemoveDataModelPath(this);

        foreach (DataModelPathSegment dataModelPathSegment in _segments)
            dataModelPathSegment.Dispose();
        _segments.Clear();

        _accessorLambda = null;
        Accessor = null;

        OnPathInvalidated();
    }

    internal void Initialize()
    {
        if (Target == null)
            return;

        Target.AddDataModelPath(this);

        DataModelPathSegment startSegment = new(this, "target", "target");
        startSegment.Node = _segments.AddFirst(startSegment);

        // On an empty path don't bother processing segments
        if (!string.IsNullOrWhiteSpace(Path))
        {
            string[] segments = Path.Split(".");
            for (int index = 0; index < segments.Length; index++)
            {
                string identifier = segments[index];
                LinkedListNode<DataModelPathSegment> node = _segments.AddLast(
                    new DataModelPathSegment(this, identifier, string.Join('.', segments.Take(index + 1)))
                );
                node.Value.Node = node;
            }
        }

        ParameterExpression parameter = Expression.Parameter(typeof(object), "t");
        Expression? expression = Expression.Convert(parameter, Target.GetType());
        Expression? nullCondition = null;

        MethodInfo equals = typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public)!;
        foreach (DataModelPathSegment segment in _segments)
        {
            BinaryExpression notNull;
            try
            {
                notNull = Expression.NotEqual(expression, Expression.Default(expression.Type));
            }
            catch (InvalidOperationException)
            {
                notNull = Expression.NotEqual(
                    Expression.Call(
                        null,
                        equals,
                        Expression.Convert(expression, typeof(object)),
                        Expression.Convert(Expression.Default(expression.Type), typeof(object))),
                    Expression.Constant(true));
            }

            nullCondition = nullCondition != null ? Expression.AndAlso(nullCondition, notNull) : notNull;
            expression = segment.Initialize(parameter, expression, nullCondition);
            if (expression == null)
                return;
        }

        if (nullCondition == null)
            return;

        _accessorLambda = Expression.Lambda<Func<object, object>>(
            // Wrap with a null check
            Expression.Condition(
                nullCondition,
                Expression.Convert(expression, typeof(object)),
                Expression.Convert(Expression.Default(expression.Type), typeof(object))
            ),
            parameter
        );

        if (IsValid)
            OnPathValidated();
    }

    private void SubscribeToDataModelStore()
    {
        DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
        DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;
    }

    private void DataModelStoreOnDataModelAdded(object? sender, DataModelStoreEvent e)
    {
        if (e.Registration.DataModel.Module.Id != Entity.DataModelId)
            return;

        Invalidate();
        Target = e.Registration.DataModel;
        Initialize();
    }

    private void DataModelStoreOnDataModelRemoved(object? sender, DataModelStoreEvent e)
    {
        if (e.Registration.DataModel.Module.Id != Entity.DataModelId)
            return;

        Invalidate();
        Target = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #region Storage

    /// <inheritdoc />
    public void Load()
    {
        Path = Entity.Path;

        if (Target == null && Entity.DataModelId != null)
            Target = DataModelStore.Get(Entity.DataModelId)?.DataModel;
    }

    /// <inheritdoc />
    public void Save()
    {
        // Do not save an invalid state
        if (!IsValid)
            return;
        
        Entity.Path = Path;
        Entity.DataModelId = DataModelId;
        
        // Store the type name but only if available
        Type? pathType = Segments.LastOrDefault()?.GetPropertyType();
        if (pathType != null)
            Entity.Type = pathType.FullName;
    }

    #endregion
}