using System.Collections.Generic;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public static class DynamicContentProviderFactory
    {
        private static readonly Dictionary<string, IDynamicContentProvider> _providers = new Dictionary<string, IDynamicContentProvider>();

        // todo: This needs to be done dynamically at runtime
        static DynamicContentProviderFactory()
        {
            RegisterProvider("FastTemplate", new FastTemplateDynamicContentProvider());
        }

        public static void RegisterProvider(string name, IDynamicContentProvider provider)
        {
            _providers[name] = provider;
        }

        public static IDynamicContentProvider GetProvider(string name)
        {
            IDynamicContentProvider provider;
            _providers.TryGetValue(name, out provider);
            return provider;
        }
    }
}
