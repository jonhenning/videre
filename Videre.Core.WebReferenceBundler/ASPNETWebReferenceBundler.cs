using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
                    if (BundleTable.Bundles.GetBundleFor(bundle.Path) == null)
                        BundleTable.Bundles.Add(bundle);
                    BundleTable.EnableOptimizations = enableOptimizations;
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
                    var bundle = new StyleBundle(src);
                    bundle.Transforms.Add(new CssRewriteUrlTransform2());
                    bundle.Include(list.Items.Select(i => "~/" + i.Src).ToArray());
                    //list.Items.Select(i => "~/" + i.Src).ToList().ForEach(i =>
                    //{
                    //    bundle.Include(i, new CssRewriteUrlTransform2());
                    //});

                    bundle.Orderer = new PassthruBundleOrderer();
                    if (BundleTable.Bundles.GetBundleFor(bundle.Path) == null)
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

    //https://gunnarpeipman.com/aspnet-mvc-bundling/
    public class CssRewriteUrlTransform2 : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            Regex pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);

            response.Content = string.Empty;

            // open each of the files
            foreach (BundleFile bfile in response.Files)
            {
                var file = bfile.VirtualFile;
                using (var reader = new StreamReader(file.Open()))
                {

                    var contents = reader.ReadToEnd();

                    // apply the RegEx to the file (to change relative paths)
                    var matches = pattern.Matches(contents);

                    if (matches.Count > 0)
                    {
                        var directoryPath = VirtualPathUtility.GetDirectory(file.VirtualPath);

                        foreach (Match match in matches)
                        {
                            if (match.Value.IndexOf("url(//") == -1)    //if external url don't replace
                            {
                                // this is a path that is relative to the CSS file
                                var imageRelativePath = match.Groups[2].Value;

                                // get the image virtual path
                                var imageVirtualPath = VirtualPathUtility.Combine(directoryPath, imageRelativePath);

                                // convert the image virtual path to absolute
                                var quote = match.Groups[1].Value;
                                var replace = String.Format("url({0}{1}{0})", quote, VirtualPathUtility.ToAbsolute(imageVirtualPath));
                                contents = contents.Replace(match.Groups[0].Value, replace);
                            }
                        }

                    }
                    // copy the result into the response.
                    response.Content = String.Format("{0}\r\n{1}", response.Content, contents);
                }
            }
        }
    }

}
