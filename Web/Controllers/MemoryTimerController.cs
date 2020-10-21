using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Services;
using System.Web.Security;
using Videre.Core.ActionResults;
using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;
//using CodeEndeavors.Extensions;
//using Videre.Core.Extensions;
//using System.Diagnostics;

namespace Videre.Web.Controllers
{
    public class MemoryTimerController : Controller
    {
        public string Index()
        {
            return $"<pre>${MemoryTimer.StaticTimings.ToJson(true)}</pre>";
        }
    }
}
