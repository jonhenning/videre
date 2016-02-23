using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Models;

namespace Videre.Core.Services
{
    public static class Widget
    {
        public const string WidgetIdHeader = "X-Videre-WidgetId";

        public sealed class WidgetIdentity
        {
            public string Id { get; internal set; }

            public Models.Widget Instance { get; internal set; }
        }

        public static WidgetIdentity Current
        {
            get
            {
                var widgetId = HttpContext.Current.Request.Headers[WidgetIdHeader];
                return string.IsNullOrWhiteSpace(widgetId) ? null : new WidgetIdentity {Id = widgetId, Instance = GetWidgetById(widgetId)};
            }
        }

        public static List<Models.Widget> GetWidgetInstancesByName(string name, string portalId = null)
        {
            var widgets = new List<Models.Widget>();
            widgets.AddRange(Portal.GetPageTemplates(portalId).SelectMany(t => t.Widgets).Where(w => w.Manifest.Name == name).ToList());
            widgets.AddRange(Portal.GetLayoutTemplates(portalId).SelectMany(t => t.Widgets).Where(w => w.Manifest.Name == name).ToList());
            return widgets;
        }

        public static Models.Widget GetWidgetById(string id, string portalId = null)
        {
            var instance = Portal.GetPageTemplates(portalId).SelectMany(t => t.Widgets).FirstOrDefault(w => w.Id == id);
            return instance ?? Portal.GetLayoutTemplates(portalId).SelectMany(t => t.Widgets).FirstOrDefault(w => w.Id == id);
        }

        public static List<Models.WidgetManifest> GetWidgetManifests()
        {
            return Repository.GetResources<Models.WidgetManifest>("WidgetManifest")
                .Select(m => m.Data)
                .OrderBy(i => i.Name)
                .ToList();
        }

        public static WidgetManifest GetWidgetManifest(string fullName)
        {
            return GetWidgetManifests().FirstOrDefault(m => m.FullName == fullName);
        }

        public static WidgetManifest GetWidgetManifestById(string Id)
        {
            return GetWidgetManifests().FirstOrDefault(m => m.Id == Id);
        }

        public static string Save(WidgetManifest manifest, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(manifest))
            {
                var res = Repository.StoreResource("WidgetManifest", null, manifest, userId);
                return res.Id;
            }
            throw new Exception(string.Format(
                Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error",
                "{0} already exists.   Duplicates Not Allowed.", "Core"), "Widget Manifest"));
        }

        public static bool IsDuplicate(WidgetManifest manifest)
        {
            var m = GetWidgetManifest(manifest.FullName);
            if (m != null)
                return m.Id != manifest.Id;
            return false;
        }

        public static bool Exists(WidgetManifest manifest)
        {
            var m = GetWidgetManifest(manifest.FullName);
            return (m != null);
        }

        public static bool DeleteManifest(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.GetResourceById<WidgetManifest>(id);
            if (res != null)
            {
                // remove from all templates first!
                var pageTemplates = Portal.GetPageTemplates().Where(t => t.Widgets.Exists(w => w.ManifestId == id)).ToList();
                pageTemplates.ForEach(t =>
                {
                    t.Widgets.RemoveAll(w => w.ManifestId == id);
                    Portal.Save(t);
                });

                var layoutTemplates =
                    Portal.GetLayoutTemplates().Where(t => t.Widgets.Exists(w => w.ManifestId == id)).ToList();
                layoutTemplates.ForEach(t =>
                {
                    t.Widgets.RemoveAll(w => w.ManifestId == id);
                    Portal.Save(t);
                });

                Repository.Delete(res);
            }
            return res != null;
        }

    }
}