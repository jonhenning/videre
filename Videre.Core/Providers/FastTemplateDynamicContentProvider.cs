using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeEndeavors.Extensions;
using PaniciSoftware.FastTemplate.Common;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public class FastTemplateDynamicContentProvider : IDynamicContentProvider
    {
        private static readonly TemplateCache _templateCache = new TemplateCache();

        // todo: this code will not support #parse or #include as-is, need to discuss approach with jon
        public string ExpandTemplate(
            string templateName, 
            string templateText, 
            IDictionary<string, object> tokens)
        {
            var cacheResult = _templateCache.GetTemplate(
                templateName,
                s => new ResourceResolverResult
                {
                    Stream = new MemoryStream(Encoding.UTF8.GetBytes(templateText ?? string.Empty))
                });
            if (!cacheResult.Success)
                throw new Exception("Dynamic content error: provider = FastTemplate, errors: = " + cacheResult.Errors.ToJson());

            var executionResult = cacheResult.Template.Execute(ToTemplateDictionary(tokens));
            if (executionResult.Errors.Count > 0)
                throw new Exception("Dynamic content error: provider = FastTemplate, errors: = " + executionResult.Errors.ToJson());

            return executionResult.Output;
        }
       
        public string ExpandTemplateFile(
            string templateName, 
            string templateFileName, 
            IDictionary<string, object> tokens)
        {
            var cacheResult = _templateCache.GetTemplate(
                templateName,
                s =>
                {
                    var result = new ResourceResolverResult();
                    var fileName = Portal.ResolvePath(templateFileName);
                    if (!System.IO.File.Exists(fileName))
                    {
                        result.Errors.Add(Error.NewError(
                            "V001", 
                            "Template file does not exist: " + fileName,
                            "Ensure the file exists in the proper location on the web server."));
                    }
                    result.SourceUrl = fileName;
                    result.Stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);                    
                    return result;
                });            
            if (!cacheResult.Success)
                throw new Exception("Dynamic content error: provider = FastTemplate, errors: = " + cacheResult.Errors.ToJson());

            var executionResult = cacheResult.Template.Execute(ToTemplateDictionary(tokens));
            if (executionResult.Errors.Count > 0)
                throw new Exception("Dynamic content error: provider = FastTemplate, errors: = " + executionResult.Errors.ToJson());

            return executionResult.Output;
        }

        private static TemplateDictionary ToTemplateDictionary(IDictionary<string, object> dict)
        {
            var templateDict = new TemplateDictionary();
            if (dict == null)
                return templateDict;
            foreach (var key in dict.Keys)
                templateDict[key] = dict[key];
            return templateDict;
        }
    }
}