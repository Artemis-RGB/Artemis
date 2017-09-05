using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
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
        private Regex _rgx;

        public LayerConditionModel()
        {
            _interpreter = new Interpreter();
            _rgx = new Regex("\\((.*?)\\)");
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

                // If the path points to a collection, look inside this collection
                if (Field.Contains("("))
                {
                    // Find the collection in the field path
                    var collectionField = _rgx.Match(Field).Groups[1].Value;
                    var collectionInspect = (IEnumerable) GeneralHelpers.GetPropertyValue(subject, collectionField);
                    var operatorParts = Operator.Split('|');
                    _lastValue = collectionInspect;

                    if (operatorParts[0] == "any")
                    {
                        var anyMatch = false;
                        foreach (var collectionValue in collectionInspect)
                        {
                            var field = Field.Split(')').Last().Substring(1);
                            anyMatch = EvaluateOperator(collectionValue, field, operatorParts[1]);
                            if (anyMatch)
                                break;
                        }
                        return anyMatch;
                    }
                    if (operatorParts[0] == "all")
                    {
                        var allMatch = true;
                        foreach (var collectionValue in collectionInspect)
                        {
                            var field = Field.Split(')').Last().Substring(1);
                            allMatch = EvaluateOperator(collectionValue, field, operatorParts[1]);
                            if (!allMatch)
                                break;
                        }
                        return allMatch;
                    }
                    if (operatorParts[0] == "none")
                    {
                        var noneMatch = true;
                        foreach (var collectionValue in collectionInspect)
                        {
                            var field = Field.Split(')').Last().Substring(1);
                            noneMatch = !EvaluateOperator(collectionValue, field, operatorParts[1]);
                            if (!noneMatch)
                                break;
                        }
                        return noneMatch;
                    }
                }

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
                    returnValue = EvaluateOperator(subject, Field);

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
            var returnValue = _interpreter.Eval<bool>($"subject.{Field} {changeOperator} value", new Parameter("subject", subject.GetType(), subject), rightParam);

            // Set the last value to the new value
            _lastValue = inspect;
            // Return the evaluated result
            return returnValue;
        }

        private bool EvaluateOperator(object subject, string field, string operatorOverwrite = null)
        {
            // Since _lastValue won't be used, rely on Value to not be null
            if (string.IsNullOrEmpty(Value))
                return false;

            if (Type == "String")
            {
                var stringExpressionText = operatorOverwrite == null
                    ? $"subject.{field}.ToLower(){Operator}(value)"
                    : $"subject.{field}.ToLower(){operatorOverwrite}(value)";

                return _interpreter.Eval<bool>(stringExpressionText, new Parameter("subject", subject.GetType(), subject), new Parameter("value", Value.ToLower()));
            }

            Parameter rightParam = null;
            switch (Type)
            {
                case "Enum":
                    var enumType = GeneralHelpers.GetPropertyValue(subject, field).GetType();
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

            var expressionText = operatorOverwrite == null
                ? $"subject.{field} {Operator} value"
                : $"subject.{field} {operatorOverwrite} value";

            return _interpreter.Eval<bool>(expressionText, new Parameter("subject", subject.GetType(), subject), rightParam);
        }
    }
}
