using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;
using Videre.Core.Extensions;

namespace Videre.Core.WebReferenceBundler
{
    public class ASPNETWebReferenceBundler : Providers.IWebReferenceBundleProvider
    {
        public string BundleScripts(List<Models.BundleList> lists, bool enableOptimizations)
        {
            var sb = new StringBuilder();
            foreach (var list in lists) //lists is a grouping of lists that can be bundled
            {
                if (list.Bundle)
                {
                    var hash = String.Join("~", list.Items.Select(i => i.Src)).GetHashCode();
                    var src = "~/Scripts/_" + hash;
                    Bundle bundle = null;

                    if (enableOptimizations)
                        bundle = new ScriptBundle(src).Include(list.Items.Select(i => "~/" + i.Src).ToArray());
                    else
                        bundle = new Bundle(src).Include(list.Items.Select(i => "~/" + i.Src).ToArray());

                    bundle.Orderer = new PassthruBundleOrderer();
                    BundleTable.Bundles.Add(bundle);
                    BundleTable.EnableOptimizations = true; // enableOptimizations;
                    sb.AppendLine(System.Web.Optimization.Scripts.Render(src).ToHtmlString());
                }
                else
                {
                    foreach (var item in list.Items)
                        sb.AppendLine(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
                }
            }
            return sb.ToString();
        }
        public string BundleCss(List<Models.BundleList> lists, bool enableOptimizations)
        {
            var sb = new StringBuilder();
            foreach (var list in lists) //lists is a grouping of lists that can be bundled
            {
                if (list.Bundle)
                {
                    var hash = String.Join("~", list.Items.Select(i => i.Src)).GetHashCode();
                    var src = "~/Content/_" + hash;
                    var bundle = new StyleBundle(src).Include(list.Items.Select(i => "~/" + i.Src).ToArray());

                    bundle.Orderer = new PassthruBundleOrderer();
                    BundleTable.Bundles.Add(bundle);
                    BundleTable.EnableOptimizations = enableOptimizations;
                    sb.AppendLine(System.Web.Optimization.Styles.Render(src).ToHtmlString());
                }
                else
                {
                    foreach (var item in list.Items)
                        sb.AppendLine(string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", item.Src, HtmlExtensions.GetDataAttributeMarkup(item.DataAttributes)));
                }
            }
            return sb.ToString();
        }


        public string Name
        {
            get { return "ASP.NET Bundler"; }
        }

        public void Register()
        {
            
        }
    }
}
