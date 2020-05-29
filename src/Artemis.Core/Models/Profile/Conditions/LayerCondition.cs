using System;
using System.Linq.Expressions;
using Artemis.Core.Plugins.Abstract.DataModels;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class LayerCondition
    {
        public Expression<Func<DataModel, bool>> ExpressionTree { get; set; }
    }
}