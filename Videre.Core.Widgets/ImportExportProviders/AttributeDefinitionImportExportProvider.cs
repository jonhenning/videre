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
            foreach (var def in definitions)
                Services.Update.Register(def);
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
