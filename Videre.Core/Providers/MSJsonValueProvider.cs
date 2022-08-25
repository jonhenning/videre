using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Videre.Core.Providers
{
    public class MSJsonValueProvider : IJsonValueProviderFactory
    {
        public string Name { get { return "MSJsonValueProvider"; } }
        public void Swap()
        {
            var defaultJsonFactory = ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault();
            var index = ValueProviderFactories.Factories.IndexOf(defaultJsonFactory);
            ValueProviderFactories.Factories.Remove(defaultJsonFactory);
            ValueProviderFactories.Factories.Insert(index, new MSJsonValueProviderFactory());
        }
    }
}
