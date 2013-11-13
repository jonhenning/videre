using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Extensions;

namespace Videre.Core.Extensions.Bootstrap.Controls
{

    public class BootstrapTabsModel : BootstrapBaseInputControlModel
    {
        public BootstrapTabsModel() : base()
        {
            tabs = new List<BootstrapTab>();
        }

        public List<BootstrapTab> tabs { get; set; }
    }

    public interface IBootstrapTabs : IFluentBootstrapInputControl<IBootstrapTabs, BootstrapTabsModel>
    {
        IBootstrapTabs Tabs(List<BootstrapTab> tabs);
    }

    public class BootstrapTab
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public bool Active { get; set; }
    }

    public class BootstrapTabs : BootstrapBaseInputControl<IBootstrapTabs, BootstrapTabsModel>, IBootstrapTabs
    {
        public BootstrapTabs(HtmlHelper html) : base(html) { }

        public IBootstrapTabs Tabs(List<BootstrapTab> tabs)
        {
            if (tabs != null)
                this._model.tabs.AddRange(tabs);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("ul");
            base.AddBaseMarkup(ctl);
            ctl.AddCssClass("nav nav-tabs videre-tabs");

            foreach (var o in this._model.tabs)
                ctl.InnerHtml += ToHtml(o);

            return ctl.ToString(TagRenderMode.Normal);
        }

        private string ToHtml(BootstrapTab tab)
        {
            var li = new TagBuilder("li");
            if (tab.Active)
                li.AddCssClass("active");
            
            var a = new TagBuilder("a");
            //a.SetInnerText(tab.Text);
            if (!string.IsNullOrEmpty(tab.Icon))
            {
                var icon = new TagBuilder("span");
                //icon.AddCssClass("glyphicon");  //todo:  do this automatically?
                icon.AddCssClass(tab.Icon);
                a.InnerHtml = icon.ToString(TagRenderMode.Normal) + " " + tab.Text;
            }
            else
                a.SetInnerText(tab.Text);


            a.Attributes.AddSafe("data-toggle", "tab");
            a.Attributes.AddSafe("href", "#" + tab.Id);

            li.InnerHtml = a.ToString(TagRenderMode.Normal);
            return li.ToString(TagRenderMode.Normal);
        }

    }

}
