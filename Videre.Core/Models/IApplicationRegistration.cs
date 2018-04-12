using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Videre.Core.Models
{
    public interface IApplicationRegistration
    {
        void Application_BeginRegister();

        void Application_BeginRegisterWidgets();

        void Application_PreRegisterWidgetsSave(int updates);

        void Application_EndRegisterWidgets(int updates);

        void Application_EndRegister();
    }
}
