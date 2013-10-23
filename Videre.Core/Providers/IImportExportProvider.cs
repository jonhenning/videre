using System.Collections.Generic;
using Videre.Core.Models;

//todo: change namespace to just Providers
namespace Videre.Core.ImportExportProviders
{
    public interface IImportExportProvider
    {
        string Name { get; }
        PortalExport Export(string id, PortalExport export = null, string portalId = null);
        List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null);
    }
}
