using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    public class ProfileCategory : CorePropertyChanged, IStorageModel
    {
        private readonly List<Profile> _profiles = new();
        private string _name;
        private bool _isCollapsed;

        /// <summary>
        ///     Creates a new instance of the <see cref="ProfileCategory" /> class
        /// </summary>
        /// <param name="name">The name of the category</param>
        public ProfileCategory(string name)
        {
            _name = name;
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
        ///     Gets or sets a boolean indicating whether the category is collapsed or not
        ///     <para>Note: Has no implications other than inside the UI</para>
        /// </summary>
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => SetAndNotify(ref _isCollapsed, value);
        }

        /// <summary>
        ///     Gets a read only collection of the profiles inside this category
        /// </summary>
        public IReadOnlyCollection<Profile> Profiles => _profiles.AsReadOnly();

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}