using System;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    public class StaticCondition : CorePropertyChanged, INodeScriptCondition
    {
        private readonly StaticConditionEntity _entity;

        public StaticCondition(ProfileElement profileElement)
        {
            _entity = new StaticConditionEntity();
            
            ProfileElement = profileElement;
            string typeDisplayName = profileElement.GetType().Name.ToLower();
            Script = new NodeScript<bool>($"Activate {typeDisplayName}", $"Whether or not this {typeDisplayName} should be active", profileElement.Profile);
        }

        internal StaticCondition(StaticConditionEntity entity, ProfileElement profileElement)
        {
            _entity = entity;

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
            if (!Script.HasNodes)
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

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Script.Dispose();
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            string typeDisplayName = ProfileElement.GetType().Name.ToLower();
            Script = new NodeScript<bool>($"Activate {typeDisplayName}", $"Whether or not this {typeDisplayName} should be active", _entity.Script,  ProfileElement.Profile);
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