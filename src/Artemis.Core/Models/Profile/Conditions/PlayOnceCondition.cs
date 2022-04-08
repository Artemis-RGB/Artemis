using System;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    public class PlayOnceCondition : ICondition
    {
        public PlayOnceCondition(RenderProfileElement profileElement)
        {
            ProfileElement = profileElement;
            Entity = new PlayOnceConditionEntity();
        }

        public PlayOnceCondition(PlayOnceConditionEntity entity, RenderProfileElement profileElement)
        {
            ProfileElement = profileElement;
            Entity = entity;
        }

        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
        }

        /// <inheritdoc />
        public void Save()
        {
        }

        #endregion

        #region Implementation of ICondition

        /// <inheritdoc />
        public IConditionEntity Entity { get; }

        /// <inheritdoc />
        public RenderProfileElement ProfileElement { get; }

        /// <inheritdoc />
        public bool IsMet { get; private set; }

        /// <inheritdoc />
        public void Update()
        {
            if (ProfileElement.Parent is RenderProfileElement parent)
                IsMet = parent.DisplayConditionMet;
            else
                IsMet = true;
        }

        /// <inheritdoc />
        public void UpdateTimeline(double deltaTime)
        {
            ProfileElement.Timeline.Update(TimeSpan.FromSeconds(deltaTime), false);
        }

        /// <inheritdoc />
        public void OverrideTimeline(TimeSpan position)
        {
            ProfileElement.Timeline.Override(position, false);
        }

        #endregion
    }
}