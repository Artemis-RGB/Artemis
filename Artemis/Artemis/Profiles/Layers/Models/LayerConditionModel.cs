using System;
using System.Globalization;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
using DynamicExpresso;
using MahApps.Metro.Controls;

namespace Artemis.Profiles.Layers.Models
{
    public class LayerConditionModel
    {
        private readonly Interpreter _interpreter;
        private object _lastValue;

        public LayerConditionModel()
        {
            _interpreter = new Interpreter();
        }

        public string Field { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
        public string Type { get; set; }
        public HotKey HotKey { get; set; }

        public bool ConditionMet(ModuleDataModel subject)
        {
            lock (subject)
            {
                if (string.IsNullOrEmpty(Field) || string.IsNullOrEmpty(Type))
                    return false;

                var inspect = GeneralHelpers.GetPropertyValue(subject, Field);
                if (inspect == null)
                {
                    _lastValue = null;
                    return false;
                }

                bool returnValue;
                if (Operator == "changed" || Operator == "decreased" || Operator == "increased")
                    returnValue = EvaluateEventOperator(subject, inspect);
                else
                    returnValue = EvaluateOperator(subject);

                _lastValue = inspect;
                return returnValue;
            }
        }

        private bool EvaluateEventOperator(ModuleDataModel subject, object inspect)
        {
            // DynamicExpresso doesn't want a null so when it was previously null (should only happen first iteration)
            // return false since that would be the only possible outcome
            if (_lastValue == null)
                return false;

            // Assign the right parameter
            var rightParam = new Parameter("value", _lastValue);

            // Come up with the proper operator
            var changeOperator = "";
            if (Operator == "changed")
                changeOperator = "!=";
            else if (Operator == "decreased")
                changeOperator = "<";
            else if (Operator == "increased")
                changeOperator = ">";

            // Evaluate the result and store it
            var returnValue = _interpreter.Eval<bool>($"subject.{Field} {changeOperator} value",
                new Parameter("subject", subject.GetType(), subject), rightParam);

            // Set the last value to the new value
            _lastValue = inspect;
            // Return the evaluated result
            return returnValue;
        }

        private bool EvaluateOperator(ModuleDataModel subject)
        {
            // Since _lastValue won't be used, rely on Value to not be null
            if (string.IsNullOrEmpty(Value))
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
                case "Single":
                    // Parse commas as decimals
                    rightParam = new Parameter("value", float.Parse(Value.Replace(",", "."),
                        CultureInfo.InvariantCulture));
                    break;
            }

            return _interpreter.Eval<bool>($"subject.{Field} {Operator} value",
                new Parameter("subject", subject.GetType(), subject), rightParam);
        }
    }
}