using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using CodeEndeavors.Extensions;
using CodeEndeavors.ResourceManager.Extensions;
using Videre.Core.Models;
using DomainObjects = CodeEndeavors.ResourceManager.DomainObjects;

namespace Videre.Core.Services
{
    public class UI
    {
        public static string ThemePath
        {
            get { return "~/Content/Themes/"; }
        }

        public static string LayoutPath
        {
            get { return "~/Views/Layouts/"; }
        }

        public static string LayoutContentPath
        {
            get { return "~/Content/Layouts/"; }
        }

        public static Models.Theme CurrentTheme
        {
            get
            {
                return Portal.CurrentPortal != null ? GetTheme(Portal.CurrentPortal.ThemeName) : null;
            }
        }

        public static string GetDefaultLayoutPath()
        {
            return GetLayoutPath("General");
        }

        public static string GetLayoutPath(string name)
        {
            return UI.LayoutPath.PathCombine(name).PathCombine("Layout.cshtml");
        }

        public static Models.Layout GetLayout(string name)
        {
            return GetLayouts().Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
        }

        public static List<Models.Layout> GetLayouts()
        {
            var layouts = new List<Models.Layout>();
            foreach (var file in Directory.GetFiles(Portal.ResolvePath(LayoutPath), "*.manifest"))
                layouts.Add(file.GetFileJSONObject<Models.Layout>(true));
            return layouts;
        }

        public static Models.Theme GetTheme(string name)
        {
            return GetThemes().Where(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
        }

        public static List<Models.Theme> GetThemes()
        {
            var themes = new List<Models.Theme>();
            var themeDir = Portal.ResolvePath(ThemePath);
            if (!Directory.Exists(themeDir))
                Directory.CreateDirectory(themeDir);
            foreach (var file in Directory.GetFiles(themeDir, "*.manifest"))
                themes.Add(file.GetFileJSONObject<Models.Theme>(true));
            return themes;
        }

        public static void InstallTheme(Models.Theme theme)
        {
            var filePath = Portal.ResolvePath(ThemePath);
            var fileName = filePath + theme.Name + ".manifest";
            if (!System.IO.File.Exists(filePath + theme.Name + ".manifest"))
            {
                theme.Thumbnail = DownloadResource(theme.Thumbnail, ThemePath.PathCombine(theme.Name));
                foreach (var file in theme.Files)
                    file.Path = DownloadResource(file.Path, ThemePath.PathCombine(theme.Name));
                theme.ToJson().WriteText(fileName);
            }
            else 
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Theme"));
        }

        public static void UninstallTheme(string name)
        {
            var theme = GetTheme(name);
            if (theme != null)
            {
                var themeDir = Portal.ResolvePath(ThemePath.PathCombine(theme.Name));
                if (Directory.Exists(themeDir))
                    Directory.Delete(themeDir, true);
                DeleteResource(ThemePath.PathCombine(theme.Name + ".manifest"));
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "NotFound.Error", "{0} not found.", "Core"), name));
        }

        private static bool DeleteResource(string path) //todo: need this???
        {
            var fileName = Portal.ResolvePath(path);
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
                return true;
            }
            return false;
        }

        private static string DownloadResource(string url, string relativePath)
        {
            if (url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                var fileName = url.Split('/').Last();
                relativePath = relativePath.PathCombine(fileName);
                url.DownloadFile(Portal.ResolvePath(relativePath));
                return relativePath;
            }

            return url;
        }

    }
}