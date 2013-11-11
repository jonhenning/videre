using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Videre.Core.Extensions.Bootstrap.Controls;

namespace Videre.Core.Extensions.Bootstrap
{
    public partial class Bootstrap<TModel>
    {
        public HtmlHelper<TModel> Html;

        public Bootstrap(HtmlHelper<TModel> _html)
        {
            this.Html = _html;
        }

        public IBootstrapButton Button(string id = null)
        {
            return new BootstrapButton(Html, id);
        }

        public IBootstrapSpan Span(string id = null)
        {
            return new BootstrapSpan(Html, id);
        }

        public IBootstrapLabel Label(string text, string forId, string id)
        {
            return new BootstrapLabel(Html, text, forId, id);
        }

        public IBootstrapLabel Label(string id, string forId)
        {
            return new BootstrapLabel(Html, id, forId);
        }
        public IBootstrapTextBox TextBox(string id = null)
        {
            return new BootstrapTextBox(Html, id);
        }
        public IBootstrapEmail Email(string id = null)
        {
            return new BootstrapEmail(Html, id);
        }
        public IBootstrapPassword Password(string id = null)
        {
            return new BootstrapPassword(Html, id);
        }

        public IBootstrapDropDownList DropDownList(string id = null)
        {
            return new BootstrapDropDownList(Html, id);
        }

        public IBootstrapDropDownList RoleList(string id = null, List<string> selectedRoles = null)
        {
            selectedRoles = selectedRoles == null ? new List<string>() : selectedRoles;
            var items = Services.Account.GetRoles().Select(r => new SelectListItem() { Value = r.Id, Text = r.Name, Selected = selectedRoles.Contains(r.Id) }).ToList();
            return DropDownList(id).Options(items);
        }

        public IBootstrapDropDownList AuthorizedList(string id = null)
        {
            var clientControl = Html.ViewData.Model as Videre.Core.Models.IClientControl;
            if (clientControl != null)
            {
                return DropDownList(id).Options(new List<SelectListItem>()
                {
                    new SelectListItem {Text = clientControl.GetPortalText("None.Text", "None"), Value = ""},
                    new SelectListItem {Text = clientControl.GetPortalText("Authenticated.Text", "Authenticated"), Value = "true"},
                    new SelectListItem {Text = clientControl.GetPortalText("NotAuthenticated.Text", "Not Authenticated"), Value = "false"}
                });
            }
            else
                throw new Exception("No Client control found to optain text from");
        }


        public IBootstrapFormGroup FormGroup(IBootstrapLabel label, IBootstrapBaseControl control)
        {
            return new BootstrapFormGroup(Html, label, control);
        }
        public IBootstrapFormGroup FormGroup(IBootstrapLabel label, IBootstrapBaseControl control, BootstrapUnits.GridSize controlSize)
        {
            return new BootstrapFormGroup(Html, label, control, controlSize);
        }

        public IBootstrapFormGroup FormGroup(IBootstrapLabel label, List<IBootstrapBaseControl> controls)
        {
            return new BootstrapFormGroup(Html, label, controls);
        }

        public IBootstrapFormGroup FormGroup(IBootstrapLabel label, List<IBootstrapBaseControl> controls, BootstrapUnits.GridSize size)
        {
            return new BootstrapFormGroup(Html, label, controls, size);
        }

        //public BootstrapLabel LabelFor<TValue>(Expression<Func<TModel, TValue>> expression)
        //{
        //    return new BootstrapLabel(Html, ExpressionHelper.GetExpressionText(expression), ModelMetadata.FromLambdaExpression(expression, Html.ViewData));
        //}

    }
}
