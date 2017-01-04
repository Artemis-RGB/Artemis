using System;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
using DynamicExpresso;

namespace Artemis.Profiles.Layers.Models
{
    public class LayerConditionModel
    {
        private readonly Interpreter _interpreter;

        public LayerConditionModel()
        {
            _interpreter = new Interpreter();
        }

        public string Field { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
        public string Type { get; set; }

        public bool ConditionMet(ModuleDataModel subject)
        {
            lock (subject)
            {
                if (string.IsNullOrEmpty(Field) || string.IsNullOrEmpty(Value) || string.IsNullOrEmpty(Type))
                    return false;

                var inspect = GeneralHelpers.GetPropertyValue(subject, Field);
                if (inspect == null)
                    return false;

                // Put the subject in a list, allowing Dynamic Linq to be used.
                if (Type == "String")
                {
                    return _interpreter.Eval<bool>($"subject.{Field}.ToLower(){Operator}(value)",
                        new Parameter("subject", subject.GetType(), subject),
                        new Parameter("value", Value.ToLower()));
                }

                Parameter rightParam = null;
                switch (Type)
                {
                    case "Enum":
                        var enumType = GeneralHelpers.GetPropertyValue(subject, Field).GetType();
                        rightParam = new Parameter("value", Enum.Parse(enumType, Value));
                        break;
                    case "Boolean":
                        rightParam = new Parameter("value", bool.Parse(Value));
                        break;
                    case "Int32":
                        rightParam = new Parameter("value", int.Parse(Value));
                        break;
                }

                return _interpreter.Eval<bool>($"subject.{Field} {Operator} value",
                    new Parameter("subject", subject.GetType(), subject), rightParam);
            }
        }
    }
}