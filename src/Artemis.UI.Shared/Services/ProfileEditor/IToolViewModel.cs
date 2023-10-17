using System;
using Artemis.Core;
using Avalonia.Input;
using Material.Icons;

namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Represents a profile editor tool.
/// </summary>
public interface IToolViewModel : IDisposable
{
    /// <summary>
    ///     Gets or sets a boolean indicating whether the tool is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Gets a boolean indicating whether the tool is enabled.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    ///     Gets a boolean indicating whether or not this tool is exclusive.
    ///     Exclusive tools deactivate any other exclusive tools when activated.
    /// </summary>
    public bool IsExclusive { get; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this tool should be shown in the toolbar.
    /// </summary>
    public bool ShowInToolbar { get; }

    /// <summary>
    ///     Gets the order in which this tool should appear in the toolbar.
    /// </summary>
    public int Order { get; }

    /// <summary>
    ///     Gets the icon which this tool should show in the toolbar.
    /// </summary>
    public MaterialIconKind Icon { get; }

    /// <summary>
    ///     Gets the tooltip which this tool should show in the toolbar.
    /// </summary>
    public string ToolTip { get; }
    
    /// <summary>
    /// Gets the keyboard hotkey that activates the tool.
    /// </summary>
    Hotkey? Hotkey { get; }
}

/// <inheritdoc cref="IToolViewModel" />
public abstract class ToolViewModel : ActivatableViewModelBase, IToolViewModel
{
    private bool _isSelected;

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
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #region Implementation of IToolViewModel

    /// <inheritdoc />
    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <inheritdoc />
    public abstract bool IsEnabled { get; }

    /// <inheritdoc />
    public abstract bool IsExclusive { get; }

    /// <inheritdoc />
    public abstract bool ShowInToolbar { get; }

    /// <inheritdoc />
    public abstract int Order { get; }

    /// <inheritdoc />
    public abstract MaterialIconKind Icon { get; }

    /// <inheritdoc />
    public abstract string ToolTip { get; }

    /// <inheritdoc />
    public abstract Hotkey? Hotkey { get; }

    #endregion
}