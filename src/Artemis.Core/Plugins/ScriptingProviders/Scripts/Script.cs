using System;
using System.ComponentModel;

namespace Artemis.Core.ScriptingProviders;

/// <summary>
///     Represents a script processed by a <see cref="ScriptingProviders.ScriptingProvider" />.
/// </summary>
public abstract class Script : CorePropertyChanged, IDisposable
{
    private bool _disposed;
    private ScriptingProvider _scriptingProvider = null!;

    /// <summary>
    ///     The base constructor of any script
    /// </summary>
    /// <param name="configuration">The script configuration this script belongs to</param>
    protected Script(ScriptConfiguration configuration)
    {
        if (configuration.Script != null)
            throw new ArtemisCoreException("The provided script configuration already has an active script");

        ScriptConfiguration = configuration;
        ScriptConfiguration.PropertyChanged += ScriptConfigurationOnPropertyChanged;
    }

    /// <summary>
    ///     Gets the scripting provider this script belongs to
    /// </summary>
    public ScriptingProvider ScriptingProvider
    {
        get => _scriptingProvider;
        internal set => SetAndNotify(ref _scriptingProvider, value);
    }

    /// <summary>
    ///     Gets the script configuration this script belongs to
    /// </summary>
    public ScriptConfiguration ScriptConfiguration { get; }

    /// <summary>
    ///     Gets the script type of this script
    /// </summary>
    public abstract ScriptType ScriptType { get; }

    /// <summary>
    ///     Occurs when the contents of the script have changed
    /// </summary>
    public event EventHandler? ScriptContentChanged;

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

    /// <summary>
    ///     Invokes the <see cref="ScriptContentChanged" /> event
    /// </summary>
    protected virtual void OnScriptContentChanged()
    {
        ScriptContentChanged?.Invoke(this, EventArgs.Empty);
    }

    internal abstract void InternalCleanup();

    private void ScriptConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScriptConfiguration.ScriptContent))
            OnScriptContentChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ScriptConfiguration.PropertyChanged -= ScriptConfigurationOnPropertyChanged;
        ScriptConfiguration.Script = null;
        ScriptingProvider.InternalScripts.Remove(this);

        // Can't trust those pesky plugin devs!
        InternalCleanup();

        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
///     Represents a type of script
/// </summary>
public enum ScriptType
{
    /// <summary>
    ///     A global script that's always active
    /// </summary>
    Global,

    /// <summary>
    ///     A script tied to a <see cref="Profile" />
    /// </summary>
    Profile
}