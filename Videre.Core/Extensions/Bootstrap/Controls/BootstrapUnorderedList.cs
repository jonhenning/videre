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

    public class BootstrapUnorderedListModel : BootstrapBaseInputControlModel
    {
        public BootstrapUnorderedListModel() : base()
        {
            items = new List<SelectListItem>();
        }

        public List<SelectListItem> items { get; set; }
    }

    public interface IBootstrapUnorderedList : IFluentBootstrapControlBase<IBootstrapUnorderedList, BootstrapUnorderedListModel>
    {
        IBootstrapUnorderedList Items(IEnumerable<SelectListItem> items);
    }

    public class BootstrapUnorderedList : BootstrapControlBase<IBootstrapUnorderedList, BootstrapUnorderedListModel>, IBootstrapUnorderedList
    {
        public BootstrapUnorderedList(HtmlHelper html, string id = null): base(html, id)
        {
        }

        public IBootstrapUnorderedList Items(IEnumerable<SelectListItem> items)
        {
            this._model.items.AddRange(items);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("ul");
            base.AddBaseMarkup(ctl);

            foreach (var o in this._model.items)
                ctl.InnerHtml += ToHtml(o);

            return ctl.ToString(TagRenderMode.Normal);
        }

        private string ToHtml(SelectListItem item)
        {
            var tag = new TagBuilder("li");
            tag.SetInnerText(item.Text);
            //tag.InnerHtml = HttpUtility.HtmlEncode(item.Text);

            if (item.Value != null)
                tag.Attributes["value"] = item.Value;
            if (item.Selected)
                tag.AddCssClass("active");
            return tag.ToString(TagRenderMode.Normal);
        }
    }

}
