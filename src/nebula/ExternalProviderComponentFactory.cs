using System;
using System.Collections.Generic;
using System.Reflection;
using ComposerCore;
using ComposerCore.Extensibility;

namespace Nebula
{
    public class ExternalProviderComponentFactory : IComponentFactory
    {
        private readonly Func<object> _instanceProvider;

        public ExternalProviderComponentFactory(Func<object> instanceProvider)
        {
            _instanceProvider = instanceProvider;
        }

        public object GetComponentInstance(ContractIdentity contract, IEnumerable<ICompositionListener> listenerChain)
        {
            return _instanceProvider.Invoke();
        }

        public IEnumerable<Type> GetContractTypes()
        {
            
            throw new NotImplementedException();
        }

        public void Initialize(IComposer composer)
        {
            
        }
    }
}