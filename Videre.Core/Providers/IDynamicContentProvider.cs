using System.Collections.Generic;

namespace Videre.Core.Providers
{
    public interface IDynamicContentProvider
    {
        string ExpandTemplate(
            string templateName, 
            string templateText, 
            IDictionary<string, object> tokens);

        string ExpandTemplateFile(
            string templateName,
            string templateFileName,
            IDictionary<string, object> tokens);
    }
}