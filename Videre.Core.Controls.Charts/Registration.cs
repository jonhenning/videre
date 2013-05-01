using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Models;
using System.Web.Mvc;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;
using System.Web.Routing;

namespace Videre.Core.Controls.Charts
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            var updates = 0;
            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;
            return updates;        
        }
    }
}