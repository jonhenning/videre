using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public interface IWebReferenceBundleProvider
    {
        string Name { get; }
        void Register();
        string BundleScripts(List<Models.BundleList> lists, bool enableOptimizations);
        string BundleCss(List<Models.BundleList> lists, bool enableOptimizations);
    }
}
