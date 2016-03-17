using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Artemis.Models
{
    public class LayerConditionModel
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }

        public bool ConditionMet(object subject)
        {
            // Put the subject in a list, allowing Dynamic Linq to be used.
            var subjectList = new List<object> {subject};
            var res = subjectList.Where($"s => s.{Field} {Operator} {Value}").Any();
            return res;
        }
    }
}