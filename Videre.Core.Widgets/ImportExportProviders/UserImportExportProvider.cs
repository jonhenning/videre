using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using Videre.Core.ImportExportProviders;
using Videre.Core.Services;

namespace Videre.Core.Widgets.ImportExportProviders
{
    public class UserImportExportProvider : IImportExportProvider
    {
        public string Name { get { return "User"; } }
        public List<string> ProviderDependencies
        {
            get { return new List<string>() {"Role"}; }
        }

        public List<ImportExportContent> GetExportContentItems(PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            return Services.Account.GetUsers(portalId).Select(u =>
                new ImportExportContent()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Type = Name,
                    Included = (export != null && export.Users != null ? export.Users.Exists(u2 => u2.Id == u.Id) : false)//,
                    //Preview = 
                }).ToList();
        }
        public PortalExport Export(string id, PortalExport export = null, string portalId = null)
        {
            portalId = string.IsNullOrEmpty(portalId) ? Services.Portal.CurrentPortalId : portalId;
            export = export ?? Services.ImportExport.GetPortalExport(portalId);
            export.Users = export.Users ?? new List<Models.User>();
            export.UserAuthentications = export.UserAuthentications ?? new List<Models.UserAuthentication>();

            var u = Services.Account.GetUserById(id);
            if (u != null)
            {
                export.Users.Add(u);
                export.Roles = export.Roles ?? new List<Models.Role>();
                export.Roles.AddRange(Services.Account.GetRoles(portalId).Where(r => u.RoleIds.Contains(r.Id)));
                var auth = Services.Authentication.GetUserAuthentication(u.Id);
                if (auth != null)
                    export.UserAuthentications.Add(auth);
            }

            return export;
        }

        public void Import(PortalExport export, Dictionary<string, string> idMap, string portalId)
        {
            if (export.Users != null)
            {
                Logging.Logger.DebugFormat("Importing {0} users...", export.Users.Count);
                foreach (var exportUser in export.Users)
                    ImportExport.SetIdMap<User>(exportUser.Id, Import(portalId, exportUser, idMap), idMap);

                foreach (var exportAuth in export.UserAuthentications)
                    ImportExport.SetIdMap<UserAuthentication>(exportAuth.Id, Import(portalId, exportAuth, idMap), idMap);

                foreach (var exportUser in export.Users)
                {
                    var newId = ImportExport.GetIdMap<User>(exportUser.Id, idMap);
                    var user = Account.GetUserById(newId);
                    if (user != null)
                    {
                        var claim = user.GetClaim("AuthenticationToken", "Videre");
                        if (claim != null)
                            claim.Value = ImportExport.GetIdMap<UserAuthentication>(claim.Value, idMap);
                        else
                        {
                            var userAuth = export.UserAuthentications.Where(a => a.UserId == exportUser.Id).FirstOrDefault();
                            if (userAuth != null)
                                user.Claims.Add(new UserClaim() { Type = "AuthenticationToken", Issuer = "Videre", Value = ImportExport.GetIdMap<UserAuthentication>(userAuth.Id, idMap) });  
                            else
                                throw new Exception("User import failed.  No UserAuthentication found");
                        }
                    }
                    Account.SaveUser(user);
                }

            }
        }

        private string Import(string portalId, Models.User user, Dictionary<string, string> idMap, string userId = null)
        {
            user = user.JsonClone();
            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existing = Account.GetUser(user.Name, portalId);
            user.PortalId = portalId;
            user.Id = existing != null ? existing.Id : null;
            user.RoleIds = Security.GetNewRoleIds(user.RoleIds, idMap);
            return Account.SaveUser(user, userId);
        }

        private string Import(string portalId, Models.UserAuthentication auth, Dictionary<string, string> idMap, string userId = null)
        {
            auth = auth.JsonClone();

            userId = string.IsNullOrEmpty(userId) ? Account.AuditId : userId;
            var existingUserId = ImportExport.GetIdMap<User>(auth.UserId, idMap);

            var existing = Authentication.GetUserAuthentication(existingUserId); //not needing portalid since we obtain userauthentication via userId which should be unique across portals
            //auth.PortalId = portalId;
            auth.Id = existing != null ? existing.Id : null;
            auth.UserId = existingUserId;

            var newAuth = Authentication.SaveUserAuthentication(auth, userId);
            if (newAuth != null)
                return newAuth.Id;
            return null;
        }

    }
}
