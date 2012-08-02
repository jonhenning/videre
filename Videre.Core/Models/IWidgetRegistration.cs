using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public interface IWidgetRegistration
    {
        int Register();
        int RegisterPortal(string portalId);
    }
}
