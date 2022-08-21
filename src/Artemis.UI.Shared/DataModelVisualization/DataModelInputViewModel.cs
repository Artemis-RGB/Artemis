using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Artemis.Core.Modules;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization;

/// <summary>
///     Represents a <see cref="DataModel" /> input view model
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public abstract class DataModelInputViewModel<T> : DataModelInputViewModel
{
    private bool _closed;
    [AllowNull] private T _inputValue;

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelInputViewModel{T}" /> class
    /// </summary>
    /// <param name="targetDescription">The description of the property this input VM is representing</param>
    /// <param name="initialValue">The initial value to set the input value to</param>
    protected DataModelInputViewModel(DataModelPropertyAttribute targetDescription, T initialValue)
    {
        TargetDescription = targetDescription;
        InputValue = initialValue;
    }

    /// <summary>
    ///     Gets or sets the value shown in the input
    /// </summary>
    [AllowNull]
    public T InputValue
    {
        get => _inputValue;
        set => this.RaiseAndSetIfChanged(ref _inputValue, value);
    }

    /// <summary>
    ///     Gets the description of the property this input VM is representing
    /// </summary>
    public DataModelPropertyAttribute TargetDescription { get; }

    internal override object InternalGuard { get; } = new();

    /// <inheritdoc />
    public sealed override void Submit()
    {
        if (_closed)
            return;
        _closed = true;

        OnSubmit();
        UpdateCallback(InputValue, true);
    }

    /// <inheritdoc />
    public sealed override void Cancel()
    {
        if (_closed)
            return;
        _closed = true;

        OnCancel();
        UpdateCallback(InputValue, false);
    }
}

/// <summary>
///     For internal use only, implement <see cref="DataModelInputViewModel{T}" /> instead.
/// </summary>
public abstract class DataModelInputViewModel : ReactiveObject
{
    /// <summary>
    ///     Prevents this type being implemented directly, implement <see cref="DataModelInputViewModel{T}" /> instead.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    internal abstract object InternalGuard { get; }

    internal Action<object?, bool> UpdateCallback { get; set; } = null!; // Set right after construction

    /// <summary>
    ///     Gets the types this input view model can support through type conversion. This list is defined when registering the
    ///     view model.
    /// </summary>
    internal IReadOnlyCollection<Type>? CompatibleConversionTypes { get; set; }

    /// <summary>
    ///     Submits the input value and removes this view model.
    ///     <para>This is called automatically when the user presses enter or clicks outside the view</para>
    /// </summary>
    public abstract void Submit();

    /// <summary>
    ///     Discards changes to the input value and removes this view model.
    ///     <para>This is called automatically when the user presses escape</para>
    /// </summary>
    public abstract void Cancel();

    /// <summary>
    ///     Called before the current value is submitted
    /// </summary>
    protected virtual void OnSubmit()
    {
    }

    /// <summary>
    ///     Called before the current value is discarded
    /// </summary>
    protected virtual void OnCancel()
    {
    }
}