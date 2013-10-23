using System;
using System.Collections.Generic;
using Videre.Core.Providers;

namespace Videre.Core.Services
{
    public static class DynamicContent
    {
        public static string ExpandTemplate(
            string providerName,
            string templateName,
            string templateText,
            IDictionary<string, object> tokens)
        {
            var provider = DynamicContentProviderFactory.GetProvider(providerName);
            if (provider == null)
                throw new Exception("Dynamic content provider does not exist: " + providerName);
            return provider.ExpandTemplate(templateName, templateText, tokens);
        }

        public static string ExpandTemplateFile(
            string providerName,
            string templateName,
            string templateFileName,
            IDictionary<string, object> tokens)
        {
            var provider = DynamicContentProviderFactory.GetProvider(providerName);
            if (provider == null)
                throw new Exception("Dynamic content provider does not exist: " + providerName);
            return provider.ExpandTemplateFile(templateName, templateFileName, tokens);            
        }
    }
}