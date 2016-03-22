using System.Collections.Generic;
using System.Linq.Dynamic;
using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles
{
    public class LayerConditionModel
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }

        public bool ConditionMet<T>(IGameDataModel subject)
        {
            // Put the subject in a list, allowing Dynamic Linq to be used.
            var subjectList = new List<T> {(T) subject};
            var res = subjectList.Where($"{Field} {Operator} {Value}").Any();
            return res;
        }
    }
}