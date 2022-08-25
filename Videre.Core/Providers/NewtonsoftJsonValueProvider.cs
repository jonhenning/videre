using System.Dynamic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Mvc;
using System;
using System.Linq;

namespace Videre.Core.Providers
{
    public class NewtonsoftJsonValueProvider : IJsonValueProviderFactory
    {
        public string Name { get { return "NewtonsoftJsonValueProvider"; } }
        public void Swap()
        {
            var defaultJsonFactory = ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault();
            var index = ValueProviderFactories.Factories.IndexOf(defaultJsonFactory);
            ValueProviderFactories.Factories.Remove(defaultJsonFactory);
            ValueProviderFactories.Factories.Insert(index, new NewtonsoftJsonValueProviderFactory());
        }
    }
}