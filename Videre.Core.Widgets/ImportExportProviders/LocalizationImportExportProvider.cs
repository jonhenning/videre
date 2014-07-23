using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;
using Videre.Core.Services;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class LocalizationImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "Localization"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>(); }
        }
        
        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Localization.Get(portalId).Select(l =>
                new ImportExportContent()
                {
                    Id = l.Id,
                    Name = l.Namespace + "/" + l.Key,
                    Type = Name,
                    Included = (export != null && export.Localizations != null ? export.Localizations.Exists(l2 => l2.Id == l.Id) : false),
                    Preview = l.Text
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Localizations = export.Localizations ?? new List<Models.Localization>();
            var l = Services.Localization.GetById(id);
            if (l != null)
                export.Localizations.Add(l);
            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Localizations != null)
            {
                Logging.Logger.DebugFormat("Importing {0} localizations...", export.Localizations.Count);
                foreach (var exportLocalization in export.Localizations)
                    ImportExport.SetIdMap<Models.Localization>(exportLocalization.Id, Services.Localization.Import(portalId, exportLocalization), idMap);
            }
        }
        //since currently part of content provider, place in service 
        //private string Import(string portalId, Models.Localization localization, string userId = null)
        //{
        //    userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;

        //    if (localization.Type == LocalizationType.Portal)
        //        localization.Namespace = portalId;   //type portal uses portalid as namespace

        //    var existing = Services.Localization.Get(portalId, localization.Type, localization.Namespace, localization.Key, localization.Locale);

        //    localization.PortalId = portalId;
        //    localization.Id = existing != null ? existing.Id : null;
        //    return Services.Localization.Save(localization, userId);
        //}

    }
}
