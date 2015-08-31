using StructureMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Videre.Core.Extensions;
using Videre.Core.Providers;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Services
{
    public class WebReferenceBundler
    {
        private static List<IWebReferenceBundleProvider> _webReferenceBundleProviders = new List<IWebReferenceBundleProvider>();

        public static void RegisterWebReferenceBundlers()
        {
            ObjectFactory.Configure(x =>
                x.Scan(scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<IWebReferenceBundleProvider>();
                }));
            _webReferenceBundleProviders = ObjectFactory.GetAllInstances<IWebReferenceBundleProvider>().ToList();
            foreach (var provider in _webReferenceBundleProviders)
                provider.Register();

            var bundlers = new List<string>() { "" };
            bundlers.AddRange(_webReferenceBundleProviders.Select(b => b.Name));

            var updates = CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Web References",
                Name = "WebReferenceBundleProvider",
                Values = bundlers,
                DefaultValue = "",
                Required = false,
                LabelKey = "WebReferenceBundleProvider.Text",
                LabelText = "Web Reference Bundle Provider"
            });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Web References",
                Name = "EnableBundleOptimizations",
                DefaultValue = false,
                Required = false,
                LabelKey = "EnableBundleOptimizations.Text",
                LabelText = "Enable Bundle Optimizations",
                DataType = "boolean",
                InputType = "checkbox",
                ControlType = "checkbox"
            });

            updates += CoreServices.Update.Register(new CoreModels.AttributeDefinition()
            {
                GroupName = "Web References",
                Name = "VersionScripts",
                DefaultValue = false,
                Required = false,
                LabelKey = "VersionScripts.Text",
                LabelText = "Version Scripts",
                DataType = "boolean",
                InputType = "checkbox",
                ControlType = "checkbox"
            });

            if (updates > 0)
                CoreServices.Repository.SaveChanges();
        }

        public static List<IWebReferenceBundleProvider> GetWebReferenceBundleProviders()
        {
            return _webReferenceBundleProviders;
        }

        public static IWebReferenceBundleProvider WebReferenceBundleProvider
        {
            get
            {
                return GetWebReferenceBundleProviders().Where(p => p.Name == Services.Portal.GetPortalSetting("Web References", "WebReferenceBundleProvider", "")).FirstOrDefault();
            }
        }

        public static bool EnableBundleOptimizations
        {
            get
            {
                return Services.Portal.GetPortalSetting("Web References", "EnableBundleOptimizations", false);
            }
        }

        public static bool VersionScripts
        {
            get
            {
                return Services.Portal.GetPortalSetting("Web References", "VersionScripts", false);
            }
        }

        public static string GenerateUnrenderedScriptMarkup(HtmlHelper helper)
        {
            var sb = new System.Text.StringBuilder();

            var listItems = GetUnrenderedMarkupList(helper, "js");

            if (WebReferenceBundleProvider != null)
                sb.AppendLine(WebReferenceBundleProvider.BundleScripts(GetBundlingLists(listItems), EnableBundleOptimizations));
            else
            {
                foreach (var item in listItems)
                    sb.AppendLine(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", getVersionedReference(item.Src), HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
            }
            SetMarkupListRendered(helper, "js");

            listItems = GetUnrenderedMarkupList(helper, "inlinejs");
            if (listItems.Count > 0)
            {
                sb.AppendLine(string.Format("<script type=\"text/javascript\">{0}</script>", String.Join("\r\n", listItems.Select(l => l.Text))));
            }
            SetMarkupListRendered(helper, "inlinejs");

            listItems = GetUnrenderedMarkupList(helper, "documentreadyjs");
            if (listItems.Count > 0)
            {
                sb.AppendLine(string.Format("<script type=\"text/javascript\">$(document).ready(function() {{{0}}});</script>", String.Join("\r\n", listItems.Select(l => l.Text))));
            }
            SetMarkupListRendered(helper, "documentreadyjs");

            listItems = GetUnrenderedMarkupList(helper, "documentreadyendjs");
            if (listItems.Count > 0)
            {
                sb.AppendLine(string.Format("<script type=\"text/javascript\">$(document).ready(function() {{{0}}});</script>", String.Join("\r\n", listItems.Select(l => l.Text))));
            }
            SetMarkupListRendered(helper, "documentreadyendjs");

            return sb.ToString();
        }

        //todo: use cache or just static var...
        private static ConcurrentDictionary<string, string> _referenceVersion = new ConcurrentDictionary<string, string>();
        private static string getVersionedReference(string src)
        {
            if (!VersionScripts)
                return src;

            if (!_referenceVersion.ContainsKey(src))
            {
                var path = System.Web.Hosting.HostingEnvironment.MapPath(src);
                var version = "0";
                if (System.IO.File.Exists(path))
                    version = new FileInfo(System.Web.Hosting.HostingEnvironment.MapPath(src)).LastWriteTime.ToString("yyyyMMddhhmmss");
                _referenceVersion[src] = version;
            }
            return src + "?v=" + _referenceVersion[src];

        }

        public static string GenerateUnrenderedStylesheetMarkup(HtmlHelper helper)
        {
            var sb = new System.Text.StringBuilder();

            var listItems = GetUnrenderedMarkupList(helper, "css");
            if (WebReferenceBundleProvider != null)
                sb.AppendLine(WebReferenceBundleProvider.BundleCss(GetBundlingLists(listItems), EnableBundleOptimizations));
            else
            {
                foreach (var item in listItems)
                    sb.AppendLine(string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", getVersionedReference(item.Src), HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
            }

            SetMarkupListRendered(helper, "css");
            return sb.ToString();
        }

        public static List<Models.ReferenceListItem> GetReferenceList(HtmlHelper helper, string type)
        {
            var dict = helper.GetContextItem<ConcurrentDictionary<string, List<Models.ReferenceListItem>>>("ReferenceList");
            if (!dict.ContainsKey(type))
                dict[type] = new List<Models.ReferenceListItem>();
            return dict[type];
        }

        private static List<Models.ReferenceListItem> GetUnrenderedMarkupList(HtmlHelper helper, string type)
        {
            var list = GetReferenceList(helper, type);
            var alreadyRendered = GetRenderedMarkupList(helper, type);
            return list.Where(l => alreadyRendered.ContainsKey(l.Src) == false).ToList();
        }

        private static void SetMarkupListRendered(HtmlHelper helper, string type)
        {
            var list = GetReferenceList(helper, type);
            var alreadyRendered = GetRenderedMarkupList(helper, type);
            foreach (var script in list)
                alreadyRendered[script.Src] = true;
        }

        private static ConcurrentDictionary<string, bool> GetRenderedMarkupList(HtmlHelper helper, string type)
        {
            return helper.GetContextItem<ConcurrentDictionary<string, bool>>("MarkupListRendered_" + type);
        }

        //todo:  better way to do this?
        //determine the breaks in the bundles so we can maintain our ordering while allowing for bundled and unbundled references to co-exist
        private static List<Models.BundleList> GetBundlingLists(List<Models.ReferenceListItem> items)
        {
            var ret = new List<Models.BundleList>();
            if (items.Count > 0)
            {
                var list = new Models.BundleList() { Bundle = items[0].CanBundle };
                foreach (var item in items)
                {
                    if (item.CanBundle != list.Bundle)
                    {
                        ret.Add(list);
                        list = new Models.BundleList() { Bundle = item.CanBundle };
                    }
                    list.Items.Add(item);
                }
                if (list.Items.Count > 0)
                    ret.Add(list);
            }
            return ret;
        }

    }
}
