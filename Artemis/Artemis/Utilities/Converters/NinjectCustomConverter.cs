using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Ninject;

namespace Artemis.Utilities.Converters
{
    public class NinjectContractResolver : DefaultContractResolver

    {
        private readonly IKernel _kernel;

        public NinjectContractResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)

        {
            var contract = base.CreateObjectContract(objectType);
            if ((bool) _kernel.CanResolve(objectType))
                contract.DefaultCreator = () => _kernel.Get(objectType);
            return contract;
        }
    }
}