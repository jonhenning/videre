using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IFluentBootstrapControlBase<TControl, TModel> : IBootstrapBaseControl
    {
        TModel Model { get; }
        TControl HtmlAttributes(IDictionary<string, object> htmlAttributes);
        TControl HtmlAttributes(object htmlAttributes);
        TControl GridSize(BootstrapUnits.GridSize size);
        TControl Css(string css);
        TControl DataColumn(string name);
        TControl DataAttribute(string key, string value);
        TControl ToolTip(string token, string defaultText);

    }
}
