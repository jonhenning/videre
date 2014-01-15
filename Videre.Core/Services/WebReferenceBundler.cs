using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Optimization;
using Videre.Core.Extensions;

namespace Videre.Core.Services
{
    public class WebReferenceBundler
    {
        public static bool UseBundles
        {
            get
            {
                return Services.Portal.GetPortalSetting("Core", "UseResourceBundles", false);
            }
        }

        public static bool EnableBundleOptimizations
        {
            get
            {
                return Services.Portal.GetAppSetting("EnableBundleOptimizations", true);
            }
        }

        public static string GenerateUnrenderedScriptMarkup(HtmlHelper helper)
        {
            var sb = new System.Text.StringBuilder();

            var listItems = GetUnrenderedMarkupList(helper, "js");

            if (UseBundles)
            {
                var lists = GetBundlingLists(listItems);
                foreach (var list in lists) //lists is a grouping of lists that can be bundled
                {
                    if (list.Bundle)
                    {
                        var hash = String.Join("~", list.Items.Select(i => i.Src)).GetHashCode();
                        var src = "~/Scripts/_" + hash;
                        var bundle = new ScriptBundle(src).Include(list.Items.Select(i => "~/" + i.Src).ToArray());

                        bundle.Orderer = new PassthruBundleOrderer();
                        BundleTable.Bundles.Add(bundle);
                        BundleTable.EnableOptimizations = EnableBundleOptimizations;
                        sb.AppendLine(System.Web.Optimization.Scripts.Render(src).ToHtmlString());
                    }
                    else
                    {
                        foreach (var item in list.Items)
                            sb.AppendLine(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
                    }
                }
            }
            else
            {
                foreach (var item in listItems)
                    sb.AppendLine(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
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

        public static string GenerateUnrenderedStylesheetMarkup(HtmlHelper helper)
        {
            var sb = new System.Text.StringBuilder();

            var listItems = GetUnrenderedMarkupList(helper, "css");

            if (UseBundles)
            {
                var lists = GetBundlingLists(listItems);
                foreach (var list in lists) //lists is a grouping of lists that can be bundled
                {
                    if (list.Bundle)
                    {
                        var hash = String.Join("~", list.Items.Select(i => i.Src)).GetHashCode();
                        var src = "~/Content/_" + hash;
                        var bundle = new StyleBundle(src).Include(list.Items.Select(i => "~/" + i.Src).ToArray());

                        bundle.Orderer = new PassthruBundleOrderer();
                        BundleTable.Bundles.Add(bundle);
                        BundleTable.EnableOptimizations = EnableBundleOptimizations;
                        sb.AppendLine(System.Web.Optimization.Styles.Render(src).ToHtmlString());
                    }
                    else
                    {
                        foreach (var item in list.Items)
                            sb.AppendLine(string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
                    }
                }
            }
            else
            {
                foreach (var item in listItems)
                    sb.AppendLine(string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
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

        private class PassthruBundleOrderer : IBundleOrderer
        {
            public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
            {
                return files;
            }
        }

        private class BundleList
        {
            public BundleList()
            {
                Items = new List<Models.ReferenceListItem>();
            }
            public List<Models.ReferenceListItem> Items { get; set; }
            public bool Bundle { get; set; }
        }
        //todo:  better way to do this?
        //determine the breaks in the bundles so we can maintain our ordering while allowing for bundled and unbundled references to co-exist
        private static List<BundleList> GetBundlingLists(List<Models.ReferenceListItem> items)
        {
            var ret = new List<BundleList>();
            if (items.Count > 0)
            {
                var list = new BundleList() { Bundle = items[0].CanBundle };
                foreach (var item in items)
                {
                    if (item.CanBundle != list.Bundle)
                    {
                        ret.Add(list);
                        list = new BundleList() { Bundle = item.CanBundle };
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
