using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using Ninject.Modules;
using NoStringEvaluating;
using NoStringEvaluating.Contract;
using NoStringEvaluating.Models.Values;
using NoStringEvaluating.Services.Cache;
using NoStringEvaluating.Services.Checking;
using NoStringEvaluating.Services.Parsing;
using NoStringEvaluating.Services.Parsing.NodeReaders;

namespace Artemis.VisualScripting.Ninject
{
    public class NoStringNinjectModule : NinjectModule
    {
        public override void Load()
        {
            // Pooling
            Bind<ObjectPool<Stack<InternalEvaluatorValue>>>()
                .ToConstant(ObjectPool.Create<Stack<InternalEvaluatorValue>>())
                .InSingletonScope();

            Bind<ObjectPool<List<InternalEvaluatorValue>>>()
                .ToConstant(ObjectPool.Create<List<InternalEvaluatorValue>>())
                .InSingletonScope();

            Bind<ObjectPool<ExtraTypeIdContainer>>()
                .ToConstant(ObjectPool.Create<ExtraTypeIdContainer>())
                .InSingletonScope();

            // Parser
            Bind<IFormulaCache>().To<FormulaCache>().InSingletonScope();
            Bind<IFunctionReader>().To<FunctionReader>().InSingletonScope();
            Bind<IFormulaParser>().To<FormulaParser>().InSingletonScope();

            // Checker
            Bind<IFormulaChecker>().To<FormulaChecker>().InSingletonScope();

            // Evaluator
            Bind<INoStringEvaluator>().To<NoStringEvaluator>().InSingletonScope();
        }
    }
}