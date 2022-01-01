using System;
using Artemis.Core;
using Artemis.UI.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.Commands
{
    public class AddProfileElement : IProfileEditorCommand, IDisposable
    {
        private readonly int _index;
        private readonly RenderProfileElement _subject;
        private readonly ProfileElement _target;
        private bool _isAdded;

        public AddProfileElement(RenderProfileElement subject, ProfileElement target, int index)
        {
            _subject = subject;
            _target = target;
            _index = index;

            DisplayName = subject switch
            {
                Layer => "Add layer",
                Folder => "Add folder",
                _ => throw new ArgumentException("Type of subject is not supported")
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_isAdded)
                _subject.Dispose();
        }

        #region Implementation of IProfileEditorCommand

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public void Execute()
        {
            _isAdded = true;
            _target.AddChild(_subject, _index);
        }

        /// <inheritdoc />
        public void Undo()
        {
            _isAdded = false;
            _target.RemoveChild(_subject);
        }

        #endregion
    }
}