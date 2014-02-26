using System.Collections.Generic;
using Videre.Core.Models;

//todo: change namespace to just Providers
namespace Videre.Core.ImportExportProviders
{
    public interface IImportExportProvider
    {
        string Name { get; }
        List<string> ProviderDependencies { get; }
        PortalExport Export(string id, PortalExport export = null, string portalId = null);
        void Import(PortalExport export, Dictionary<string, string> idMap, string portalId);
        List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null);
    }
}
