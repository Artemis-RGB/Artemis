using System;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.Commands
{
    public class RemoveProfileElement : IProfileEditorCommand, IDisposable
    {
        private readonly int _index;
        private readonly RenderProfileElement _subject;
        private readonly ProfileElement _target;
        private bool _isRemoved;

        public RemoveProfileElement(RenderProfileElement subject)
        {
            if (subject.Parent == null)
                throw new ArtemisUIException("Can't remove a subject that has no parent");

            _subject = subject;
            _target = _subject.Parent;
            _index = _subject.Children.IndexOf(_subject);

            DisplayName = subject switch
            {
                Layer => "Remove layer",
                Folder => "Remove folder",
                _ => throw new ArgumentException("Type of subject is not supported")
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isRemoved)
                _subject.Dispose();
        }

        #region Implementation of IProfileEditorCommand

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public void Execute()
        {
            _isRemoved = true;
            _target.RemoveChild(_subject);
            _subject.Deactivate();
        }

        /// <inheritdoc />
        public void Undo()
        {
            _isRemoved = false;
            _subject.Activate();
            _target.AddChild(_subject, _index);

        }

        #endregion
    }
}