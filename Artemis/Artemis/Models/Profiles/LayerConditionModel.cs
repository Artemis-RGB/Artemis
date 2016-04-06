using System;
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
        public string Type { get; set; }

        public bool ConditionMet<T>(IGameDataModel subject)
        {
            if (string.IsNullOrEmpty(Field) || string.IsNullOrEmpty(Value) || string.IsNullOrEmpty(Type))
                return false;
            try
            {
                // Put the subject in a list, allowing Dynamic Linq to be used.
                var subjectList = new List<T> {(T) subject};
                var res = Type == "String"
                    ? subjectList.Where($"{Field}.ToLower() {Operator} @0", Value.ToLower()).Any()
                    : subjectList.Where($"{Field} {Operator} {Value}").Any();
                return res;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
    }
}