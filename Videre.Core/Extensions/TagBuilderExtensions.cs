using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
namespace Videre.Core.Extensions
{
    public static class TagBuilderExtensions
    {
        public static TagBuilder AddCssStyle(this TagBuilder tb, string name, string value)
        {
            if (tb.Attributes.ContainsKey("style"))
                tb.Attributes["style"] += name + ":" + value + ";";
            else
                tb.Attributes.Add("style", name + ":" + value + ";");
            return tb;
        }

    }
}