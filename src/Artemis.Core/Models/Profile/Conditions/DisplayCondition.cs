using System;
using System.Linq.Expressions;
using Artemis.Core.Plugins.DataModelExpansions;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayCondition
    {
        public Expression<Func<DataModel, bool>> ExpressionTree { get; set; }
        public DisplayConditionGroup RootGroup { get; set; }
    }
}