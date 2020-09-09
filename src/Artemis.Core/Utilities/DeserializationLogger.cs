using Newtonsoft.Json;
using Ninject;
using Serilog;

namespace Artemis.Core
{
    internal static class DeserializationLogger
    {
        private static ILogger _logger;

        public static void Initialize(IKernel kernel)
        {
            _logger = kernel.Get<ILogger>();
        }

        public static void LogPredicateDeserializationFailure(DisplayConditionPredicate displayConditionPredicate, JsonException exception)
        {
            _logger.Warning(
                exception,
                "Failed to deserialize display condition predicate {left} {operator} {right}",
                displayConditionPredicate.Entity.LeftPropertyPath,
                displayConditionPredicate.Entity.OperatorType,
                displayConditionPredicate.Entity.RightPropertyPath
            );
        }

        public static void LogListPredicateDeserializationFailure(DisplayConditionListPredicate displayConditionPredicate, JsonException exception)
        {
            _logger.Warning(
                exception,
                "Failed to deserialize display condition list predicate {list} => {left} {operator} {right}",
                displayConditionPredicate.Entity.ListPropertyPath,
                displayConditionPredicate.Entity.LeftPropertyPath,
                displayConditionPredicate.Entity.OperatorType,
                displayConditionPredicate.Entity.RightPropertyPath
            );
        }

        public static void LogModifierDeserializationFailure(string modifierName, JsonSerializationException exception)
        {
            _logger.Warning(exception, "Failed to deserialize static parameter for modifier {modifierName}", modifierName);
        }
    }
}
