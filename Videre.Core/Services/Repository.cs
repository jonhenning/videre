using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeEndeavors.ResourceManager;
using System.Web;
using System.Configuration;
using CodeEndeavors.Extensions;
using System.Runtime.Remoting.Messaging;
using CodeEndeavors.ResourceManager.DomainObjects;

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

        public static int PendingUpdates { 
            get 
            { 
                return current.PendingUpdates; 
            } 
            set
            {
                current.PendingUpdates = value;
            }
        }

        public static bool IsOpen
        {
            get { return GetContextData<ResourceRepository>("CurrentResourceRepository") != null; } 
        }

        //todo: add HttpRequest caching on these calls?

        public static List<Resource<T>> GetResources<T>()
        {
            return current.GetResources<T>();
        }

        public static List<Resource<T>> GetResources<T>(string type, string key = null)
        {
            return current.GetResources<T>(type, key);
        }

        public static List<Resource<T>> GetResources<T>(string type, Func<Resource<T>, dynamic> statement, bool bestMatch = false)
        {
            return current.GetResources<T>(type, statement, bestMatch);
        }

        public static List<Resource<T>> GetResources<T>(string type, string key, Func<Resource<T>, dynamic> statement, bool bestMatch = true)
        {
            return current.GetResources<T>(type, key, statement, bestMatch);
        }

        public static List<Resource<T>> GetResources<T>(List<Resource<T>> resources, List<Query<Resource<T>>> queries, bool bestMatch = true)
        {
            return current.GetResources<T>(resources, queries, bestMatch);
        }

        public static T GetResourceData<T>(string type, string key, List<Query<Resource<T>>> queries, T defaultValue)
        {
            return current.GetResourceData<T>(type, key, queries, defaultValue);
        }

        public static T GetResourceData<T>(string type, Func<Resource<T>, dynamic> statement, T defaultValue)
        {
            return current.GetResourceData<T>(type, statement, defaultValue);
        }

        public static Resource<T> GetResourceById<T>(string id)
        {
            return current.GetResourceById<T>(id);
        }

        public static Resource<T> StoreResource<T>(string type, string key, T data, string userId)
        {
            return current.StoreResource<T>(type, key, data, userId);
        }

        public static void Delete<T>(Resource<T> resource)
        {
            current.Delete<T>(resource);
        }

        public static void DeleteAll<T>(string type)
        {
            current.DeleteAll<T>(type);
        }

        private static ResourceRepository current
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

        [Obsolete("Do not use directly, methods are provided as statics in Services.Repository")]
        public static ResourceRepository Current
        {
            get
            {
                return current;
            }
        }

        private static object _lock = new object(); //todo: probably move this into CodeEndeavors resourcemanager
        public static void Dispose()
        {
            if (IsOpen)
            {
                //Services.Logging.Logger.Debug("Disposing Repository...");
                if (current.PendingUpdates > 0) //todo:  auto save here?
                {
                    lock (_lock)
                    {
                        Services.Logging.Logger.DebugFormat("Persisting {0} changes to repository", current.PendingUpdates);
                        current.SaveChanges();
                    }
                }
                current.Dispose();
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
            current.SaveChanges();
        }


    }
}
