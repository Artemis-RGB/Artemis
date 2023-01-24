using Artemis.Core.DryIoc;
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

public class NoStringDryIocModule : IModule
{
    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        // Pooling
        builder.RegisterInstance(ObjectPool.Create<Stack<InternalEvaluatorValue>>());
        builder.RegisterInstance(ObjectPool.Create<List<InternalEvaluatorValue>>());
        builder.RegisterInstance(ObjectPool.Create<ExtraTypeIdContainer>());
        
        // Parser
        builder.Register<IFormulaCache, FormulaCache>(Reuse.Singleton);
        builder.Register<IFunctionReader, FunctionReader>(Reuse.Singleton);
        builder.Register<IFormulaParser, FormulaParser>(Reuse.Singleton);
        
        // Checker
        builder.Register<IFormulaChecker, FormulaChecker>(Reuse.Singleton);
        
        // Evaluator
        builder.Register<INoStringEvaluator, NoStringEvaluator>(Reuse.Singleton);
    }
}