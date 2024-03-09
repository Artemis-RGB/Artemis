﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents a category containing <see cref="ProfileConfigurations" />
/// </summary>
public class ProfileCategory : CorePropertyChanged, IStorageModel
{
    /// <summary>
    /// Represents an empty profile category.
    /// </summary>
    public static readonly ProfileCategory Empty = new("Empty", -1);
    
    private bool _isCollapsed;
    private bool _isSuspended;
    private string _name;
    private int _order;

    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileCategory" /> class
    /// </summary>
    /// <param name="name">The name of the category</param>
    /// <param name="order">The order of the category</param>
    internal ProfileCategory(string name, int order)
    {
        _name = name;
        _order = order;
        Entity = new ProfileCategoryEntity();
        ProfileConfigurations = new ReadOnlyCollection<ProfileConfiguration>([]);
        
        Save();
    }

    internal ProfileCategory(ProfileCategoryEntity entity)
    {
        _name = null!;
        Entity = entity;
        ProfileConfigurations = new ReadOnlyCollection<ProfileConfiguration>([]);

        Load();
    }

    /// <summary>
    ///     Gets or sets the name of the profile category
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetAndNotify(ref _name, value);
    }

    /// <summary>
    ///     The order in which this category appears in the update loop and sidebar
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetAndNotify(ref _order, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the category is collapsed or not
    ///     <para>Note: Has no implications other than inside the UI</para>
    /// </summary>
    public bool IsCollapsed
    {
        get => _isCollapsed;
        set => SetAndNotify(ref _isCollapsed, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this category is suspended, disabling all its profiles
    /// </summary>
    public bool IsSuspended
    {
        get => _isSuspended;
        set => SetAndNotify(ref _isSuspended, value);
    }

    /// <summary>
    ///     Gets a read only collection of the profiles inside this category
    /// </summary>
    public ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations { get; private set; }

    /// <summary>
    ///     Gets the unique ID of this category
    /// </summary>
    public Guid EntityId => Entity.Id;

    internal ProfileCategoryEntity Entity { get; }


    /// <summary>
    ///     Adds a profile configuration to this category
    /// </summary>
    public void AddProfileConfiguration(ProfileConfiguration configuration, int? targetIndex)
    {
        List<ProfileConfiguration> targetList = ProfileConfigurations.ToList();
        
        // TODO: Look into this, it doesn't seem to make sense
        // Removing the original will shift every item in the list forwards, keep that in mind with the target index
        if (configuration.Category == this && targetIndex != null && targetIndex.Value > targetList.IndexOf(configuration))
            targetIndex -= 1;

        configuration.Category.RemoveProfileConfiguration(configuration);
        
        if (targetIndex != null)
        {
            targetIndex = Math.Clamp(targetIndex.Value, 0, targetList.Count);
            targetList.Insert(targetIndex.Value, configuration);
        }
        else
        {
            targetList.Add(configuration);
        }

        configuration.Category = this;
        ProfileConfigurations = new ReadOnlyCollection<ProfileConfiguration>(targetList);

        for (int index = 0; index < ProfileConfigurations.Count; index++)
            ProfileConfigurations[index].Order = index;
        OnProfileConfigurationAdded(new ProfileConfigurationEventArgs(configuration));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[ProfileCategory] {Order} {nameof(Name)}: {Name}, {nameof(IsSuspended)}: {IsSuspended}";
    }

    /// <summary>
    ///     Occurs when a profile configuration is added to this <see cref="ProfileCategory" />
    /// </summary>
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileConfigurationAdded;

    /// <summary>
    ///     Occurs when a profile configuration is removed from this <see cref="ProfileCategory" />
    /// </summary>
    public event EventHandler<ProfileConfigurationEventArgs>? ProfileConfigurationRemoved;

    /// <summary>
    ///     Invokes the <see cref="ProfileConfigurationAdded" /> event
    /// </summary>
    protected virtual void OnProfileConfigurationAdded(ProfileConfigurationEventArgs e)
    {
        ProfileConfigurationAdded?.Invoke(this, e);
    }

    /// <summary>
    ///     Invokes the <see cref="ProfileConfigurationRemoved" /> event
    /// </summary>
    protected virtual void OnProfileConfigurationRemoved(ProfileConfigurationEventArgs e)
    {
        ProfileConfigurationRemoved?.Invoke(this, e);
    }

    internal void RemoveProfileConfiguration(ProfileConfiguration configuration)
    {
        ProfileConfigurations = new ReadOnlyCollection<ProfileConfiguration>(ProfileConfigurations.Where(pc => pc != configuration).ToList());
        for (int index = 0; index < ProfileConfigurations.Count; index++)
            ProfileConfigurations[index].Order = index;
        
        OnProfileConfigurationRemoved(new ProfileConfigurationEventArgs(configuration));
    }

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
        Name = Entity.Name;
        IsCollapsed = Entity.IsCollapsed;
        IsSuspended = Entity.IsSuspended;
        Order = Entity.Order;

        ProfileConfigurations = new ReadOnlyCollection<ProfileConfiguration>(Entity.ProfileConfigurations.Select(pc => new ProfileConfiguration(this, pc)).ToList());
    }

    /// <inheritdoc />
    public void Save()
    {
        Entity.Name = Name;
        Entity.IsCollapsed = IsCollapsed;
        Entity.IsSuspended = IsSuspended;
        Entity.Order = Order;
        
        Entity.ProfileConfigurations.Clear();
        foreach (ProfileConfiguration profileConfiguration in ProfileConfigurations)
            Entity.ProfileConfigurations.Add(profileConfiguration.Entity);
    }

    #endregion
}

/// <summary>
///     Represents a name of one of the default categories
/// </summary>
public enum DefaultCategoryName
{
    /// <summary>
    ///     The category used by profiles tied to games
    /// </summary>
    Games,

    /// <summary>
    ///     The category used by profiles tied to applications
    /// </summary>
    Applications,

    /// <summary>
    ///     The category used by general profiles
    /// </summary>
    General
}