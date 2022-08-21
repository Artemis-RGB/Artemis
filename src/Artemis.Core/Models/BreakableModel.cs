using System;
using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
///     Provides a default implementation for models that can have a broken state
/// </summary>
public abstract class BreakableModel : CorePropertyChanged, IBreakableModel
{
    private string? _brokenState;
    private Exception? _brokenStateException;

    /// <summary>
    ///     Invokes the <see cref="BrokenStateChanged" /> event
    /// </summary>
    protected virtual void OnBrokenStateChanged()
    {
        BrokenStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public abstract string BrokenDisplayName { get; }

    /// <summary>
    ///     Gets or sets the broken state of this breakable model, if <see langword="null" /> this model is not broken.
    ///     <para>Note: If setting this manually you are also responsible for invoking <see cref="BrokenStateChanged" /></para>
    /// </summary>
    public string? BrokenState
    {
        get => _brokenState;
        set => SetAndNotify(ref _brokenState, value);
    }

    /// <summary>
    ///     Gets or sets the exception that caused the broken state
    ///     <para>Note: If setting this manually you are also responsible for invoking <see cref="BrokenStateChanged" /></para>
    /// </summary>
    public Exception? BrokenStateException
    {
        get => _brokenStateException;
        set => SetAndNotify(ref _brokenStateException, value);
    }

    /// <inheritdoc />
    public bool TryOrBreak(Action action, string breakMessage)
    {
        try
        {
            action();
            ClearBrokenState(breakMessage);
            return true;
        }
        catch (Exception e)
        {
            SetBrokenState(breakMessage, e);
            return false;
        }
    }

    /// <inheritdoc />
    public void SetBrokenState(string state, Exception? exception)
    {
        BrokenState = state ?? throw new ArgumentNullException(nameof(state));
        BrokenStateException = exception;
        OnBrokenStateChanged();
    }

    /// <inheritdoc />
    public void ClearBrokenState(string state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (BrokenState == null)
            return;

        if (BrokenState != state) return;
        BrokenState = null;
        BrokenStateException = null;
        OnBrokenStateChanged();
    }

    /// <inheritdoc />
    public virtual IEnumerable<IBreakableModel> GetBrokenHierarchy()
    {
        if (BrokenState != null)
            yield return this;
    }

    /// <inheritdoc />
    public event EventHandler? BrokenStateChanged;
}