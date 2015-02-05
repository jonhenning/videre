using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Services;
using System.Web.Security;
using Videre.Core.ActionResults;
//using CodeEndeavors.Extensions;
//using Videre.Core.Extensions;
//using System.Diagnostics;

namespace Videre.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index(HandleErrorInfo exception)
        {
            return View("Error", exception);
        }

    }
}
