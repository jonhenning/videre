using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using Videre.Core.Extensions;
using Videre.Core.Services;
using CoreModels = Videre.Core.Models;
using CoreServices = Videre.Core.Services;

namespace Videre.Carousel.Widgets.Services
{
    public class Carousel
    {
        public static Models.Carousel GetById(string id)
        {
            var res = CoreServices.Repository.GetResourceById<Models.Carousel>(id);
            if (res != null)
                return res.Data;
            return null;
        }
        
        public static List<Models.Carousel> Get(string portalId)
        {
            return CoreServices.Repository.GetResources<Models.Carousel>("Carousel", m => m.Data.PortalId == portalId, false).Select(f => f.Data).ToList();
        }

        public static Models.Carousel Get(string portalId, string name)
        {
            return CoreServices.Repository.GetResourceData<Models.Carousel>("Carousel", m => m.Data.PortalId == portalId && m.Data.Name == name, null);
        }

        public static string Import(string portalId, Models.Carousel carousel, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Get(portalId, carousel.Name);
            if (existing != null)
                carousel.Id = existing.Id;
            else
                carousel.Id = null;
            carousel.PortalId = portalId;
            return Save(carousel, userId);
        }

        public static string Save(Models.Carousel carousel, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Authentication.AuthenticatedUserId : userId;
            carousel.PortalId = string.IsNullOrEmpty(carousel.PortalId) ? CoreServices.Portal.CurrentPortalId : carousel.PortalId;
            Validate(carousel);
            var res = CoreServices.Repository.StoreResource("Carousel", null, carousel, userId);
            return res.Id;
        }

        public static void Validate(Models.Carousel carousel)
        {
            if (string.IsNullOrEmpty(carousel.Name))
                throw new Exception(CoreServices.Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Carousel"));
            if (Exists(carousel))
                throw new Exception(CoreServices.Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Carousel"));
        }

        public static bool Exists(Models.Carousel carousel)
        {
            var existingCarousel = Get(carousel.PortalId, carousel.Name); 
            return existingCarousel != null && existingCarousel.Name == carousel.Name && existingCarousel.Id != carousel.Id;
        }

        public static bool Delete(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? CoreServices.Authentication.AuthenticatedUserId : userId;
            var Carousel = CoreServices.Repository.GetResourceById<Models.Carousel>(id);
            if (Carousel != null)
                CoreServices.Repository.Delete(Carousel);
            return Carousel != null;
        }

    }
}
