using Newtonsoft.Json;
using Ninject;
using Serilog;

namespace Artemis.Core
{
    internal static class DeserializationLogger
    {
        private static ILogger? _logger;

        public static void Initialize(IKernel kernel)
        {
            _logger = kernel.Get<ILogger>();
        }

        public static void LogPredicateDeserializationFailure(DataModelConditionPredicate dataModelConditionPredicate, JsonException exception)
        {
            _logger?.Warning(
                exception,
                "Failed to deserialize display condition predicate {left} {operator} {right}",
                dataModelConditionPredicate.Entity.LeftPath?.Path,
                dataModelConditionPredicate.Entity.OperatorType,
                dataModelConditionPredicate.Entity.RightPath?.Path
            );
        }

        public static void LogModifierDeserializationFailure(string modifierName, JsonSerializationException exception)
        {
            _logger?.Warning(exception, "Failed to deserialize static parameter for modifier {modifierName}", modifierName);
        }
    }
}
