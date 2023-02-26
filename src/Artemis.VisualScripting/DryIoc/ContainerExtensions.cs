using DryIoc;
using Microsoft.Extensions.ObjectPool;
using NoStringEvaluating;
using NoStringEvaluating.Contract;
using NoStringEvaluating.Models.Values;
using NoStringEvaluating.Services.Cache;
using NoStringEvaluating.Services.Checking;
using NoStringEvaluating.Services.Parsing;
using NoStringEvaluating.Services.Parsing.NodeReaders;

namespace Artemis.VisualScripting.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers NoStringEvaluating services into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterNoStringEvaluating(this IContainer container)
    {
        // Pooling
        container.RegisterInstance(ObjectPool.Create<Stack<InternalEvaluatorValue>>());
        container.RegisterInstance(ObjectPool.Create<List<InternalEvaluatorValue>>());
        container.RegisterInstance(ObjectPool.Create<ExtraTypeIdContainer>());
        
        // Parser
        container.Register<IFormulaCache, FormulaCache>(Reuse.Singleton);
        container.Register<IFunctionReader, FunctionReader>(Reuse.Singleton);
        container.Register<IFormulaParser, FormulaParser>(Reuse.Singleton);
        
        // Checker
        container.Register<IFormulaChecker, FormulaChecker>(Reuse.Singleton);
        
        // Evaluator
        container.Register<INoStringEvaluator, NoStringEvaluator>(Reuse.Singleton);
    }
}