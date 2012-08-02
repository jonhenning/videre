using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Services;
using System.Web.Security;
//using CodeEndeavors.Extensions;
//using Videre.Core.Extensions;
//using System.Diagnostics;

namespace Videre.Web.Controllers
{
    public class RouteController : Controller
    {

        public ActionResult Index(string name)
        {
            if (Portal.CurrentPortal == null)
                return View("Installer", new Core.Models.View() { ClientId = "installer" });

            var template = Portal.GetPageTemplateFromUrl(name);
            if (!template.IsAuthorized)
                FormsAuthentication.RedirectToLoginPage();

            this.ViewBag.Template = template;   //todo: do we want/need this?
            this.ViewBag.Title = Portal.CurrentPortal != null ? Portal.CurrentPortal.Title : "Videre";
            Portal.CurrentTemplate = template;
            Portal.CurrentUrl = name;
            this.ViewData.Model = template;


            return View();
        }

    }
}
