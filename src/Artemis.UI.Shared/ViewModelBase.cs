using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Artemis.UI.Shared.Events;
using FluentAvalonia.UI.Controls;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents the base class for Artemis view models
/// </summary>
public abstract class ContentDialogViewModelBase : ValidatableViewModelBase, IDisposable
{
    /// <summary>
    ///     Gets the content dialog that hosts the view model
    /// </summary>
    public ContentDialog? ContentDialog { get; internal set; }

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
///     Represents the base class for Artemis view models used to drive dialogs
/// </summary>
public abstract class DialogViewModelBase<TResult> : ValidatableViewModelBase
{
    /// <summary>
    ///     Closes the dialog with the given <paramref name="result" />
    /// </summary>
    /// <param name="result">The result of the dialog</param>
    public void Close(TResult result)
    {
        CloseRequested?.Invoke(this, new DialogClosedEventArgs<TResult>(result));
    }

    internal event EventHandler<DialogClosedEventArgs<TResult>>? CloseRequested;
}

/// <summary>
///     Represents the base class for Artemis view models that are interested in validation and the activated event
/// </summary>
public abstract class ValidatableViewModelBase : ReactiveValidationObject, IActivatableViewModel
{
    private string? _displayName;

    /// <summary>
    ///     Gets or sets the display name of the view model
    /// </summary>
    public string? DisplayName
    {
        get => _displayName;
        set => RaiseAndSetIfChanged(ref _displayName, value);
    }

    /// <summary>
    ///     RaiseAndSetIfChanged fully implements a Setter for a read-write property on a ReactiveObject, using
    ///     CallerMemberName to raise the notification and the ref to the backing field to set the property.
    /// </summary>
    /// <typeparam name="TRet">The type of the return value.</typeparam>
    /// <param name="backingField">A Reference to the backing field for this property.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="propertyName">
    ///     The name of the property, usually automatically provided through the CallerMemberName
    ///     attribute.
    /// </param>
    /// <returns>The newly set value, normally discarded.</returns>
    [NotifyPropertyChangedInvocator]
    public TRet RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null)
            throw new ArgumentNullException(nameof(propertyName));

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
            return newValue;

        this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        this.RaisePropertyChanged(propertyName);
        return newValue;
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();
    
    /// <summary>
    /// Raises the property changed event for the provided property.
    /// </summary>
    /// <param name="args">The event arguments containing the name of the property that changed.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    /// <summary>
    /// Raises the property changing event for the provided property.
    /// </summary>
    /// <param name="args">The event arguments containing the name of the property that is changing.</param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }
}

/// <summary>
///     Represents the base class for Artemis view models that are interested in the activated event
/// </summary>
public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
{
    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();
}

/// <summary>
///     Represents the base class for Artemis view models
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    private string? _displayName;

    /// <summary>
    ///     Gets or sets the display name of the view model
    /// </summary>
    public string? DisplayName
    {
        get => _displayName;
        set => RaiseAndSetIfChanged(ref _displayName, value);
    }

    /// <summary>
    ///     RaiseAndSetIfChanged fully implements a Setter for a read-write property on a ReactiveObject, using
    ///     CallerMemberName to raise the notification and the ref to the backing field to set the property.
    /// </summary>
    /// <typeparam name="TRet">The type of the return value.</typeparam>
    /// <param name="backingField">A Reference to the backing field for this property.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="propertyName">
    ///     The name of the property, usually automatically provided through the CallerMemberName
    ///     attribute.
    /// </param>
    /// <returns>The newly set value, normally discarded.</returns>
    [NotifyPropertyChangedInvocator]
    public TRet RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null)
            throw new ArgumentNullException(nameof(propertyName));

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
            return newValue;

        this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        this.RaisePropertyChanged(propertyName);
        return newValue;
    }

    /// <summary>
    /// Raises the property changed event for the provided property.
    /// </summary>
    /// <param name="args">The event arguments containing the name of the property that changed.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    /// <summary>
    /// Raises the property changing event for the provided property.
    /// </summary>
    /// <param name="args">The event arguments containing the name of the property that is changing.</param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }
}