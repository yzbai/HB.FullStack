using System;
using System.Collections.Generic;

namespace HB.FullStack.Common.Models
{
    internal class ModelDefFactory : IModelDefFactory
    {
        private readonly IEnumerable<IModelDefProvider> _providers;
        private readonly Dictionary<ModelKind, List<IModelDefProvider>> _providerDict = new Dictionary<ModelKind, List<IModelDefProvider>>();

        public ModelDefFactory(IEnumerable<IModelDefProvider> providers)
        {
            _providers = providers;

            foreach (var provider in providers)
            {
                if (!_providerDict.TryGetValue(provider.ModelKind, out var providerList))
                {
                    providerList = new List<IModelDefProvider>();
                    _providerDict[provider.ModelKind] = providerList;
                }

                providerList.Add(provider);
            }
        }

        public ModelDef GetDef(Type type, ModelKind modelKind)
        {
            if (!typeof(IModel).IsAssignableFrom(type))
            {
                throw CommonExceptions.CannotGetModelDefForNonModel(type);
            }

            IEnumerable<IModelDefProvider> providerList = GetProviderList(type, modelKind);

            ModelDef? modelDef = null;

            foreach (var provider in providerList)
            {
                modelDef = provider.GetModelDef(type);

                if (modelDef != null)
                {
                    break;
                }
            }

            if (modelDef == null)
            {
                throw CommonExceptions.ModelDefProviderDidNotProvideModelDef(type, modelKind);
            }

            return modelDef;

            IEnumerable<IModelDefProvider> GetProviderList(Type type, ModelKind modelKind)
            {
                if (modelKind == ModelKind.UnKown)
                {
                    return _providers;
                }
                else
                {
                    if (!_providerDict.TryGetValue(modelKind, out var providerDictList))
                    {
                        throw CommonExceptions.LackModelDefProvider(type, modelKind);
                    }

                    return providerDictList;
                }
            }
        }
    }
}
