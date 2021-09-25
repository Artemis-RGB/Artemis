using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a condition that is based on a data model value
    /// </summary>
    public class StaticCondition : CorePropertyChanged, INodeScriptCondition
    {
        private readonly string _displayName;
        private readonly StaticConditionEntity _entity;

        /// <summary>
        ///     Creates a new instance of the <see cref="StaticCondition" /> class
        /// </summary>
        public StaticCondition(ProfileElement profileElement)
        {
            _entity = new StaticConditionEntity();
            _displayName = profileElement.GetType().Name;

            ProfileElement = profileElement;
            Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not this {_displayName} should be active", profileElement.Profile);
        }

        internal StaticCondition(StaticConditionEntity entity, ProfileElement profileElement)
        {
            _entity = entity;
            _displayName = profileElement.GetType().Name;

            ProfileElement = profileElement;
            Script = null!;

            Load();
        }

        /// <summary>
        ///     Gets the script that drives the static condition
        /// </summary>
        public NodeScript<bool> Script { get; private set; }

        /// <inheritdoc />
        public IConditionEntity Entity => _entity;

        /// <inheritdoc />
        public ProfileElement ProfileElement { get; }

        /// <inheritdoc />
        public bool IsMet { get; private set; }

        /// <inheritdoc />
        public void Update()
        {
            if (!Script.ExitNodeConnected)
            {
                IsMet = true;
                return;
            }

            Script.Run();
            IsMet = Script.Result;
        }

        /// <inheritdoc />
        public void ApplyToTimeline(bool isMet, bool wasMet, Timeline timeline)
        {
            if (isMet && !wasMet && timeline.IsFinished)
                timeline.JumpToStart();
            else if (!isMet && wasMet && timeline.StopMode == TimelineStopMode.SkipToEnd)
                timeline.JumpToEndSegment();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Script.Dispose();
        }

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not this {_displayName} should be active", _entity.Script, ProfileElement.Profile);
        }

        /// <inheritdoc />
        public void Save()
        {
            Script.Save();
            _entity.Script = Script.Entity;
        }

        /// <inheritdoc />
        public void LoadNodeScript()
        {
            Script.Load();
        }

        #endregion
    }
}