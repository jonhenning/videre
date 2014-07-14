using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeEndeavors.ResourceManager;
using System.Web;
using System.Configuration;
using CodeEndeavors.Extensions;
using System.Runtime.Remoting.Messaging;

namespace Videre.Core.Services
{
    public class Repository
    {

        public static T GetContextData<T>(string key) where T : class
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Items[key] as T;
            return CallContext.GetData(key) as T;
        }


        public static void SetContextData(string key, object data)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items[key] = data;
            else
                CallContext.SetData(key, data);
        }

        public static bool IsOpen
        {
            get { return GetContextData<ResourceRepository>("CurrentResourceRepository") != null; } 
        }

        public static ResourceRepository Current
        {
            get
            {
                ResourceRepository repo = null;
                if (!IsOpen)
                {
                    repo = new ResourceRepository(Portal.GetAppSetting("RepositoryConnection", ""));
                    SetContextData("CurrentResourceRepository", repo);
                    //if (HttpContext.Current != null)
                    //    HttpContext.Current.Items["CurrentResourceRepository"] = repo;
                    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.GetSetting("AzureBlobStorage", ""))) //todo: more elegant way of doing this!
                    //    HttpContext.Current.Items["CurrentResourceRepository"] = new ResourceRepository(ConfigurationManager.AppSettings.GetSetting("AzureBlobStorage", ""), ResourceRepository.RepositoryType.AzureBlob);
                    //else
                    //    HttpContext.Current.Items["CurrentResourceRepository"] = new ResourceRepository(ConfigurationManager.AppSettings.GetSetting("FileDb", @"~\FileDb"), ResourceRepository.RepositoryType.File);
                }
                else
                    repo = GetContextData<ResourceRepository>("CurrentResourceRepository");
                    //repo = HttpContext.Current.Items["CurrentResourceRepository"] as ResourceRepository;
                return repo;
            }
        }

        private static object _lock = new object(); //todo: probably move this into CodeEndeavors resourcemanager
        public static void Dispose()
        {
            if (IsOpen)
            {
                //Services.Logging.Logger.Debug("Disposing Repository...");
                if (Current.PendingUpdates > 0) //todo:  auto save here?
                {
                    lock (_lock)
                    {
                        Services.Logging.Logger.DebugFormat("Persisting {0} changes to repository", Current.PendingUpdates);
                        Current.SaveChanges();
                    }
                }
                Current.Dispose();
            }
        }

        //public static void ClearResources<T>(string Type)
        //{
        //    Current.ClearResources<T>(Type);
        //}

        //public static void DeleteAll<T>()
        //{
        //    Current.DeleteAll<T>();
        //}

        public static void SaveChanges()
        {
            Current.SaveChanges();
        }


    }
}
