using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Videre.Core.Models;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Carousel.Widgets
{
    public class Registration : IWidgetRegistration
    {
        public int Register()
        {
            //RouteTable.Routes.MapRoute(
            //    "Carousel_default",
            //    "carouselapi/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }
            //);

            // The Register method is called during application start
            // your widget manifest should be registered here.  
            var updates = CoreServices.Update.Register(new List<CoreModels.WidgetManifest>()
            {
                new CoreModels.WidgetManifest() { Path = "Carousel", Category = "General", Name = "Carousel", Title = "Carousel", EditorPath = "Widgets/Carousel/CarouselEditor", EditorType = "videre.widgets.editor.carousel", ContentProvider = "Videre.Carousel.Widgets.ContentProviders.CarouselContentProvider, Videre.Carousel.Widgets", AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        Name = "CarouselProvider",
                        Values = new List<string>() { "Pixedelic", "Boostrap" },
                        DefaultValue = "Pixedelic",
                        Required = true,
                        LabelKey = "CarouselProvider.Text",
                        LabelText = "Carousel Provider"
                    },
                    new AttributeDefinition()
                    {
                        Name = "CropImages",
                        Values = new List<string>() { "Yes", "No" },
                        DefaultValue = "Yes",
                        LabelKey = "CropImages.Text",
                        LabelText = "CropImages",
                        Dependencies = new List<AttributeDependency>() { new AttributeDependency() { DependencyName = "CarouselProvider", Values = new List<string>() { "Pixedelic" }}}
                    },
                    new AttributeDefinition()
                    {
                        Name = "AdvanceTime",
                        DefaultValue = "7000",
                        //DataType = "integer", //TODO:
                        LabelKey = "AdvanceTime.Text",
                        LabelText = "Advance Time (ms)",
                        Dependencies = new List<AttributeDependency>() { new AttributeDependency() { DependencyName = "CarouselProvider", Values = new List<string>() { "Pixedelic" }}}
                    }
                }
                }
            });

            // If you wish to secure an AJAX endpoint, you need to register an Area and a Name.
            // An area is just a namespace (not a MVC area).  The Name is something to group one or more roles under.
            // Typically the admin role will be granted the rights by default.
            // See the LocationController for a commented line of code on how to ensure the AJAX call has rights
            //updates += CoreServices.Update.Register(new List<CoreModels.SecureActivity>()
            //{
            //    new CoreModels.SecureActivity() { PortalId = CoreServices.Portal.CurrentPortalId, Area = "Videre.Carousel.Widgets", Name = "Administration", Roles = new List<string>() {CoreServices.Update.AdminRoleId} }
            //});

            return updates; //return number of updates we have (so we can call save if something changed)
        }

        public int RegisterPortal(string portalId)
        {
            var updates = 0;
            // add registration logic that is specific to a portal here
            return updates;
        }
    }
}
