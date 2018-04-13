using CodeEndeavors.Extensions;
using System;
using System.Web;
using System.Web.Security;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.Providers
{
    public class ApplicationRegistrationHandler : IApplicationRegistration
    {
        string _lockSource = null;
        public void Application_BeginRegister()
        {
            //_lockSource = Services.Repository.ObtainLock(Environment.MachineName, CoreServices.Portal.CurrentPortalId);
        }

        public void Application_BeginRegisterWidgets()
        {
            
        }

        public void Application_EndRegister()
        {
            
        }
        public void Application_PreRegisterWidgetsSave(int updates)
        {
            //if (updates > 0 && _lockSource != Environment.MachineName)
            //    throw new Exception("Application Concurrency Error");
        }

        public void Application_EndRegisterWidgets(int updates)
        {
            //Services.Repository.RemoveLock(Environment.MachineName, CoreServices.Portal.CurrentPortalId);
        }

    }
}
