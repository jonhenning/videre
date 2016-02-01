using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Videre.Core.Extensions;
using CodeEndeavors.Extensions;
using System.Configuration;
using Videre.Core.Models;
using Videre.Core.ActionResults;
using System.Web;

namespace Videre.Core.Services
{
    public class Package
    {
        public static string PackageDir
        {
            get
            {
                return Portal.ResolvePath(Portal.GetAppSetting("PackageDir", "~/_packages/"));
            }
        }

        public static string PublishDir
        {
            get
            {
                return Portal.ResolvePath(Portal.GetAppSetting("PublishDir", "~/_publish/"));
            }
        }

        public static string RemotePackageUrl
        {
            get 
            { 
                var url = Portal.CurrentPortal.GetAttribute("Core", "RemotePackageUrl", "");
                if (!url.EndsWith("core/package/", StringComparison.InvariantCultureIgnoreCase))
                    url = url.PathCombine("core/package/", "/");
                return url;
            }
        }

        public static List<Models.Package> GetNewestAvailablePackages()
        {
            var packages = GetAvailablePackages();
            return packages.GroupBy(p => p.Name, p => p).Select(p => p.OrderByDescending(p2 => p2.Version).First()).ToList();
        }

        public static List<Models.Package> GetAvailablePackages()
        {
            return GetPackagesFromDir(PackageDir);
        }

        public static Models.Package GetPublishedPackage(string name, string version)
        {
            return GetPublishedPackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        public static List<Models.Package> GetPublishedPackages()
        {
            return GetPackagesFromDir(PublishDir);
        }

        public static List<Models.Package> GetPackagesFromDir(string dir)
        {
            var packages = new List<Models.Package>();
            var packageDir = Portal.ResolvePath(dir);
            if (!Directory.Exists(packageDir))
                Directory.CreateDirectory(packageDir);

            foreach (var file in Directory.GetFiles(packageDir, "*.zip"))
            {
                var manifest = GetPackageManifest(file);
                if (manifest != null)
                {
                    manifest.FileName = file;
                    packages.Add(manifest);
                }
            }
            return packages;
        }

        public static string AddPackage(string fileName)
        {
            var manifest = GetPackageManifest(fileName);
            if (manifest == null)
                throw new Exception(Localization.GetLocalization(LocalizationType.Exception, "PackageRequiresManifest.Error", "Package requires a manifest.", "Core"));

            var destFileName = PackageDir.PathCombine(new FileInfo(fileName).Name, @"\");
            if (new FileInfo(fileName).FullName != new FileInfo(destFileName).FullName)
            {
                if (System.IO.File.Exists(destFileName))
                    System.IO.File.Delete(destFileName);
                System.IO.File.Move(fileName, destFileName);
            }
            return destFileName;
        }

        public static bool InstallAvailablePackage(string name, string version = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;

            var package = GetAvailablePackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(version) || p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase))).OrderByDescending(p => p.Version).FirstOrDefault();
            if (package != null)
            {
                InstallFile(Path.Combine(Portal.ResolvePath(PackageDir), package.FileName), portalId, false);
                //System.IO.File.Copy(Path.Combine(Portal.ResolvePath(PackageDir), package.FileName), Path.Combine(Portal.ResolvePath(UpdateDir), package.FileName), true);
                return true;
            }
            return false;
        }

        public static bool RemoveAvailablePackage(string name, string version)
        {
            var package = GetAvailablePackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(version) || p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase))).OrderByDescending(p => p.Version).FirstOrDefault();
            if (package != null)
            {
                System.IO.File.Delete(package.FileName);
                return true;
            }
            return false;
        }

        public static bool UninstallPackage(string name, string version)
        {
            var package = GetInstalledPackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(version) || p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase))).OrderByDescending(p => p.Version).FirstOrDefault();
            if (package != null)
            {
                //todo: get manifest/files from available package zip and do the right thing...
                DeletePackage(package.Id);
                RemoveAvailablePackage(name, version);
                return true;
            }
            return false;
        }

        public static bool TogglePublishPackage(string name, string version, bool publish)
        {
            if (publish)
            {
                var package = GetAvailablePackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(version) || p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase))).OrderByDescending(p => p.Version).FirstOrDefault();
                if (package != null)
                {
                    System.IO.File.Copy(package.FileName, PublishDir.PathCombine(new FileInfo(package.FileName).Name, @"\"), true);
                    return true;
                }
            }
            else
            {
                var package = GetPublishedPackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(version) || p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase))).OrderByDescending(p => p.Version).FirstOrDefault();
                if (package != null)
                {
                    System.IO.File.Delete(package.FileName);
                    return true;
                }
            }
            return false;
        }

        public static Models.Package GetPackageManifest(string zipFileName)
        {
            var entryName = zipFileName.GetZipFileList(e => e.EndsWith("package.manifest", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (!string.IsNullOrEmpty(entryName))
                return zipFileName.GetZipEntryContents(entryName).ToObject<Models.Package>();
            else
                return null;
        }

        public static List<Models.PortalExport> GetPackageContent(string zipFileName)
        {
            var ret = new List<Models.PortalExport>();
            var entries = zipFileName.GetZipFileList(e => e.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var entry in entries)
            {
                var export = zipFileName.GetZipEntryContents(entry).ToObject<Models.PortalExport>();
                if (isValidExport(export))
                    ret.Add(export);
            }
            return ret;
        }

        public static void ExtractNonContentJsonFiles(string zipFileName)
        {
            var rootDir = Portal.ResolvePath("~/");
            var entries = zipFileName.GetZipFileList(e => e.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var entry in entries)
            {
                var export = zipFileName.GetZipEntryContents(entry).ToObject<Models.PortalExport>();
                if (!isValidExport(export))
                {
                    zipFileName.ExtractEntry(entry, rootDir);
                }
            }
        }

        private static bool isValidExport(Models.PortalExport export)
        {
            return export != null && export.Portal != null && !string.IsNullOrEmpty(export.Portal.Id);
        }

        public static int ApplyPackageContent(string zipFileName, string portalId)
        {
            var count = 0;
            
            var content = GetPackageContent(zipFileName);
            foreach (var portalExport in content)
            {
                var hash = Package.GetJsonHash(portalExport.ToJson(ignoreType: "db"));
                if (!Package.ImportHashExists(new System.IO.FileInfo(zipFileName).Name, hash))
                {
                    Services.ImportExport.Import(portalExport, portalId);
                    Package.AddAppliedImportHash(new System.IO.FileInfo(zipFileName).Name, hash);
                    count++;
                }
            }

            return count;
        }

        public static bool InstallFile(string fileName, string portalId = null, bool removeFile = true)
        {
            var rootDir = Portal.ResolvePath("~/");
            var file = new FileInfo(fileName);
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;

            if (file != null)
            {
                switch (file.Extension.ToLower())
                {
                    case ".zip":
                        {
                            //todo: dependency checks
                            var packageManifest = GetPackageManifest(fileName);
                            
                            var packageFileName = packageManifest != null ? AddPackage(fileName) : fileName;   //if zip has manifest put in package folder and install from there

                            Logging.Logger.InfoFormat("Applying update for file: {0}", packageFileName);
                            packageFileName.ExtractZip(rootDir, @"-package\.manifest;-\.json");

                            //todo:  deferred package content apply or right now?  
                            //ideally manifest will have the registration assembly listed and we can register right away then instantly apply
                            ApplyPackageContent(packageFileName, portalId);

                            ExtractNonContentJsonFiles(packageFileName);    //we want to allow json files to be extracted, just ignore the ones that are package content

                            if (removeFile)
                                System.IO.File.Delete(file.FullName);

                            if (packageManifest != null)
                            {
                                var existingPackage = GetInstalledPackage(packageManifest.Name);
                                if (existingPackage != null)
                                    packageManifest.Id = existingPackage.Id; //todo: odd to do this..
                                packageManifest.FileName = packageFileName;
                                packageManifest.InstallDate = DateTime.UtcNow;
                                Save(packageManifest);
                            }

                            return true;
                        }
                    case ".json":
                        {
                            Logging.Logger.InfoFormat("Applying import for file: {0}", file.FullName);
                            var portalExport = file.FullName.GetFileJSONObject<Models.PortalExport>(false);
                            var hash = Package.GetJsonHash(portalExport.ToJson(ignoreType: "db"));
                            if (!Package.ImportHashExists(file.Name, hash))
                            {
                                Services.ImportExport.Import(portalExport, portalId);
                                Package.AddAppliedImportHash(file.Name, hash);
                            }
                            if (removeFile)
                                System.IO.File.Delete(file.FullName);
                            return true;
                        }
                    default:
                        {
                            Logging.Logger.Error("Unknown File Extension: " + file.Extension);
                            break;
                            //throw new Exception("Unknown File Extension: " + file.Extension);
                        }
                }
            }
            return false;
        }

        public static List<Models.Package> GetRemotePackages()
        {
            var packages = new List<Models.Package>();
            if (!string.IsNullOrEmpty(RemotePackageUrl))
            {
                var client = new System.Net.WebClient();
                var json = client.DownloadString(RemotePackageUrl.PathCombine("getpublishedpackages", "/"));
                packages = json.ToObject<List<Models.Package>>();
            }
            return packages;
        }

        public static Models.Package GetRemotePackage(string name, string version)
        {
            return GetRemotePackages().Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && p.Version.Equals(version, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public static bool DownloadRemotePackage(string name, string version)
        {
            if (!string.IsNullOrEmpty(RemotePackageUrl))
            {
                var remotePackage = GetRemotePackage(name, version); 
                if (remotePackage != null)
                {
                    var client = new System.Net.WebClient();
                    var fileName = remotePackage.FileName.Substring(remotePackage.FileName.LastIndexOf(@"\")+1);
                    var downloadFileName = PackageDir.PathCombine(fileName, @"\");
                    
                    client.DownloadFile(RemotePackageUrl.PathCombine("getpublishedpackage", "/") + "?name=" + HttpUtility.UrlEncode(name) + "&version=" + HttpUtility.UrlEncode(version), downloadFileName);   
                    
                    if (new FileInfo(downloadFileName).Length == 0) //if no data found... delete it... 
                    {
                        System.IO.File.Delete(downloadFileName);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public static List<Models.Package> GetInstalledPackages()
        {
            return Repository.Current.GetResources<Models.Package>("Package").Select(m => m.Data).OrderBy(i => i.Name).ToList();
        }
        public static Models.Package GetInstalledPackage(string name)
        {
            return GetInstalledPackages().Where(m => m.Name == name).FirstOrDefault();
        }
        public static Models.Package GetPackageById(string id)
        {
            return GetInstalledPackages().Where(m => m.Id == id).FirstOrDefault();
        }
        //public static string Import(Models.Package manifest, string userId = null)
        //{
        //    userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
        //    var existing = GetPackage(manifest.FullName);
        //    manifest.Id = existing != null ? existing.Id : null;
        //    return Save(manifest, userId);
        //}
        public static string Save(Models.Package package, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            if (!IsDuplicate(package))
            {
                var res = Repository.Current.StoreResource("Package", null, package, userId);
                return res.Id;
            }
            else
                throw new Exception(string.Format(Localization.GetLocalization(LocalizationType.Exception, "DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Core"), "Package"));
        }
        public static bool IsDuplicate(Models.Package package)
        {
            var m = GetInstalledPackage(package.Name);
            if (m != null)
                return m.Id != package.Id;
            return false;
        }
        public static bool Exists(Models.Package package)
        {
            var m = GetInstalledPackage(package.Name);
            return (m != null);
        }
        public static bool DeletePackage(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.Package>(id);
            if (res != null)
            {
                //throw new Exception("NOT IMPLEMENTED YET!");
                Repository.Current.Delete(res);
            }
            return res != null;
        }


        public static List<Models.ImportExportContentJob> GetExportJobs(string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Portal.CurrentPortalId : portalId;
            return Repository.Current.GetResources<Models.ImportExportContentJob>("ImportExportContentJob").Select(m => m.Data).Where(a =>
                (string.IsNullOrEmpty(portalId) || a.PortalId == portalId) 
                ).OrderBy(a => a.Name).ToList();
        }

        public static Models.ImportExportContentJob GetExportJob(string name, string portalId = null)
        {
            return GetExportJobs(portalId).Where(j => name.Equals(j.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public static string SaveExportJob(Models.ImportExportContentJob job, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            job.PortalId = string.IsNullOrEmpty(job.PortalId) ? Portal.CurrentPortalId : job.PortalId;
            //if (string.IsNullOrEmpty(job.Name))
                job.Name = job.Package.Name;
            Validate(job);
            var res = Repository.Current.StoreResource("ImportExportContentJob", null, job, userId);
            return res.Id;
        }

        public static bool DeleteExportJob(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.ImportExportContentJob>(id);
            if (res != null)
                Repository.Current.Delete(res);
            return res != null;
        }

        public static void Validate(Models.ImportExportContentJob job)
        {
            if (string.IsNullOrEmpty(job.PortalId) || string.IsNullOrEmpty(job.Name))
                throw new Exception(Localization.GetExceptionText("InvalidResource.Error", "{0} is invalid.", "Job"));
            if (IsDuplicate(job))
                throw new Exception(Localization.GetExceptionText("DuplicateResource.Error", "{0} already exists.   Duplicates Not Allowed.", "Job"));
        }
        public static bool IsDuplicate(Models.ImportExportContentJob job)
        {
            var e = GetExportJob(job.Name, job.PortalId);
            return (e != null && e.Id != job.Id);
        }

        public static bool Exists(Models.ImportExportContentJob job)
        {
            return GetExportJob(job.Name, job.PortalId) != null;
        }

        public static bool DeleteSecureActivity(string id, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var res = Repository.Current.GetResourceById<Models.ImportExportContentJob>(id);
            if (res != null)
                Repository.Current.Delete(res);
            return res != null;
        }

        public static string GetJsonHash(string json)
        {
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hash, 0, hash.Length);
        }

        public static List<Models.ImportHash> GetAppliedImportHashes(string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            return Repository.Current.GetResources<Models.ImportHash>().Select(h => h.Data).ToList();
        }

        public static bool AddAppliedImportHash(string name, string hash, string userId = null)
        {
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var importHash = new Models.ImportHash() { Name = name };
            var existing = GetAppliedImportHashes().Where(h => h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (existing != null)
                importHash = existing;
            if (importHash.Hash != hash)
            {
                importHash.Hash = hash;
                Repository.Current.StoreResource("ImportHash", null, importHash, userId);
                return true;
            }
            return false;
        }

        public static bool ImportHashExists(string name, string hash)
        {
            var existing = GetAppliedImportHashes().Where(h => h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return (existing != null && existing.Hash == hash);
        }

    }
}
