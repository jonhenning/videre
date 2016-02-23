using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Services;
using System.Web.Security;
using Videre.Core.Providers;
using CodeEndeavors.Extensions;
//using CodeEndeavors.Extensions;
//using Videre.Core.Extensions;
//using System.Diagnostics;

namespace Videre.Web.Controllers
{
    public class RouteController : Controller
    {

        public ActionResult Index(string name)
        {
            if (name == null)   //todo: hacky?
                name = "";

            if (Portal.CurrentPortal == null)
                return View("Installer", new Core.Models.View() { ClientId = "installer" });

            var template = Portal.GetPageTemplateFromUrl(name);

            foreach (var handler in RequestHandlers.OrderBy(h => h.Priority))
                handler.Execute(name, template);

            //moved to request handler
            //if (!template.IsAuthorized)
            //    FormsAuthentication.RedirectToLoginPage();  //todo: not authorized may mean we are already logged in.  Fix!

            this.ViewBag.Template = template;   //todo: do we want/need this?
            this.ViewBag.Title = Portal.CurrentPortal != null ? Portal.CurrentPortal.Title : "Videre";
            Portal.CurrentTemplate = template;
            Portal.CurrentUrl = name;
            this.ViewData.Model = template;


            return View();
        }

        List<IRequestHandler> _requestHandlers = null;
        private List<IRequestHandler> RequestHandlers
        {
            get
            {
                if (_requestHandlers == null)
                    _requestHandlers = ReflectionExtensions.GetAllInstances<IRequestHandler>();
                return _requestHandlers;
            }
        }

    }
}
