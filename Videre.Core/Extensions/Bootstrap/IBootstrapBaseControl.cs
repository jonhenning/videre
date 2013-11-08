using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IBootstrapBaseControl : IHtmlString, IFluentInterface
    {
        string Id { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        new string ToHtmlString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddHtmlAttributes(IDictionary<string, object> htmlAttributes);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddHtmlAttributes(object htmlAttributes);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddCss(string css);
    }
}
