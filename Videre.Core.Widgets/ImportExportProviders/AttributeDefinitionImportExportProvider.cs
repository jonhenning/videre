using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class AttributeDefinitionImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Portal Attribute Definition"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            var exportDefinitions = GetDefinitionsFromCustom(export);

            return AllPortalAttributeDefinitions().Select(r =>
                new ImportExportContent()
                {
                    Id = r.GroupName + "." + r.Name,
                    Name = r.GroupName + "." + r.Name,
                    Included = exportDefinitions != null && exportDefinitions.Exists(d => d.GroupName == r.GroupName && d.Name == r.Name),
                    Preview = r.DefaultValue != null ? r.DefaultValue.ToString() : ""
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);

            //var exportDefinitions = GetDefinitionsFromCustom(export);
            
            var defs = GetDefinitionsFromCustom(export);
            var def = AllPortalAttributeDefinitions().Where(d => d.GroupName + "." + d.Name == id).FirstOrDefault();
            if (def != null)
                defs.Add(def);

            export.Custom = export.Custom != null ? export.Custom : new Dictionary<string, object>();
            if (!export.Custom.ContainsKey(Name))
                export.Custom[Name] = defs;

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            var definitions = GetDefinitionsFromCustom(export);
            var existingDefs = AllPortalAttributeDefinitions();
            foreach (var def in definitions)
            {
                //being selective about how we update this.
                var existingDef = existingDefs.Where(d => d.GroupName == def.GroupName && d.Name == def.Name).FirstOrDefault();
                if (existingDef != null)
                {
                    //todo: this logic could use some improvement... brute forcing it for lack of time..
                    def.Id = existingDef.Id;
                    def.LabelKey = existingDef.LabelKey ?? def.LabelKey;
                    def.LabelText = existingDef.LabelText ?? def.LabelText;
                    def.DataType = existingDef.DataType ?? def.DataType;
                    def.ControlType = existingDef.ControlType ?? def.ControlType;
                    def.InputType = existingDef.InputType ?? def.InputType;
                    def.InputType = existingDef.InputType ?? def.InputType;
                    def.Required = existingDef.Required;
                    def.DefaultValue = existingDef.DefaultValue ?? def.DefaultValue;

                    if (def.Values != null && existingDef.Values != null)
                        def.Values = def.Values.Union(existingDef.Values).ToList(); //merge lists
                    else if (def.Values == null)
                        def.Values = existingDef.Values;
                    if (def.Dependencies != null && existingDef.Dependencies != null)
                        def.Dependencies = def.Dependencies.Union(existingDef.Dependencies).ToList(); //merge lists
                    else if (def.Dependencies == null)
                        def.Dependencies = existingDef.Dependencies;
                }
                Services.Update.Register(def);
            }
        }

        private List<AttributeDefinition> AllPortalAttributeDefinitions()
        {
            return Services.Portal.AttributeDefinitions.Values.SelectMany(a => a).ToList();
        }

        private List<AttributeDefinition> GetDefinitionsFromCustom(PortalExport export)
        {
            var exportDefinitions = new List<AttributeDefinition>();
            if (export != null && export.Custom != null && export.Custom.ContainsKey(Name))
                exportDefinitions = export.Custom[Name].ToJson().ToObject<List<AttributeDefinition>>();
            return exportDefinitions;
        }


    }
}
