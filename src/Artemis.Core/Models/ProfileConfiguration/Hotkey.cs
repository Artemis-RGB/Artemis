using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents a key or combination of keys that changes the suspension status of a <see cref="ProfileConfiguration" />
/// </summary>
public class Hotkey : CorePropertyChanged, IStorageModel
{
    /// <summary>
    ///     Creates a new instance of <see cref="Hotkey" />
    /// </summary>
    public Hotkey()
    {
        Entity = new ProfileConfigurationHotkeyEntity();
    }

    /// <inheritdoc />
    public Hotkey(KeyboardKey? key, KeyboardModifierKey? modifiers)
    {
        Key = key;
        Modifiers = modifiers;
        Entity = new ProfileConfigurationHotkeyEntity();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Hotkey" /> based on the provided entity
    /// </summary>
    public Hotkey(ProfileConfigurationHotkeyEntity entity)
    {
        Entity = entity;
        Load();
    }

    /// <summary>
    ///     Gets or sets the <see cref="KeyboardKey" /> of the hotkey
    /// </summary>
    public KeyboardKey? Key { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="KeyboardModifierKey" />s of the hotkey
    /// </summary>
    public KeyboardModifierKey? Modifiers { get; set; }

    /// <summary>
    /// Gets the entity used to store this hotkey
    /// </summary>
    public ProfileConfigurationHotkeyEntity Entity { get; }

    /// <summary>
    ///     Determines whether the provided <see cref="ArtemisKeyboardKeyEventArgs" /> match the hotkey
    /// </summary>
    /// <returns><see langword="true" /> if the event args match the hotkey; otherwise <see langword="false" /></returns>
    public bool MatchesEventArgs(ArtemisKeyboardKeyEventArgs eventArgs)
    {
        return eventArgs.Key == Key && (eventArgs.Modifiers == Modifiers || (eventArgs.Modifiers == KeyboardModifierKey.None && Modifiers == null));
    }

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
        Key = (KeyboardKey?) Entity.Key;
        Modifiers = (KeyboardModifierKey?) Entity.Modifiers;
    }

    /// <inheritdoc />
    public void Save()
    {
        Entity.Key = (int?) Key;
        Entity.Modifiers = (int?) Modifiers;
    }

    #endregion
}