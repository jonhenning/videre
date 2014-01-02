using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Videre.Core.Models;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.OAuth
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            RouteTable.Routes.MapRoute(
                "OAuth_default",
                "OAuth/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                null,
                new string[] { "Videre.Core.OAuth.Controllers" }
            );

            var updates = CoreServices.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "OAuth", Name = "LogOn", Title = "OAuth Log On", Category = "OAuth" }, 
                new CoreModels.WidgetManifest() { Path = "OAuth", Name = "UserProfile", Title = "OAuth User Profile", Category = "OAuth" } 
            });

            CoreServices.Update.Register("OAuth", new CoreModels.AttributeDefinition() { Name = "Google", DefaultValue = "false", LabelKey = "Google.Text", LabelText = "Google", DataType = "boolean", InputType = "checkbox", ControlType = "checkbox"  });

            CoreServices.Update.Register("OAuth", new CoreModels.AttributeDefinition() { Name = "MicrosoftClientId", DefaultValue = "", LabelKey = "MicrosoftClientId.Text", LabelText = "Microsoft Client Id" });
            CoreServices.Update.Register("OAuth", new CoreModels.AttributeDefinition() { Name = "MicrosoftSecret", DefaultValue = "", LabelKey = "MicrosoftSecret.Text", LabelText = "Microsoft Secret"});

            Services.OAuth.RegisterOAuthClients();

            return updates;
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;
            return updates;        
        }
    }
}