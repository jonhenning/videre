using System;
using System.Linq;
using System.Collections.Generic;
//using System.Web.Script.Serialization;
//using Newtonsoft.Json;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;

namespace Videre.Core.Models
{
    public class User
    {
        public User()
        {
            //Roles = new List<string>();
            Attributes = new Dictionary<string, object>();
            //Claims = new List<UserClaim>();
        }

        private string _timeZone = null;

        public string Id { get; set; }
        public string PortalId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }
        public string TimeZone
        {
            get
            {
                //if (string.IsNullOrEmpty(_timeZone))
                //    return System.TimeZone.CurrentTimeZone.StandardName;

                return _timeZone;
            }
            set
            {
                _timeZone = value;
            }
        }

        public DateTime GetUserTime(DateTime date)
        {
            if (!string.IsNullOrEmpty(TimeZone))
            {
                var info = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                if (info != null)
                    return date.Add(info.BaseUtcOffset);
            }
            return date;
        }

        //[ScriptIgnore, JsonIgnore]
        [SerializeIgnore(new string[] { "db", "client" })]
        public string Password { get; set; }
        
        //[ScriptIgnore]
        [SerializeIgnore(new string[] { "client" })]
        public string PasswordHash { get; set; }

        [SerializeIgnore(new string[] { "client" })]
        public string PasswordSalt { get; set; }

        private List<string> _roles = new List<string>();
        [System.Obsolete("Use RoleIds")]
        [SerializeIgnore(new string[] { "client" })]
        public List<string> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }

        private List<string> _roleIds = new List<string>();
        public List<string> RoleIds
        {
            get
            {
                if (_roles != null && _roles.Count > 0)
                {
                    _roleIds.AddRange(_roles);
                    _roles.Clear();
                }
                return _roleIds;
            }
            set
            {
                _roleIds = value;
            }
        }

        [SerializeIgnore(new string[] { "db" })]
        public List<string> RoleNames   //todo: not sure I like this!
        {
            get
            {
                var roles = Services.Account.GetRoles(PortalId);
                return roles.Where(r => RoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
            }
        }
        public Dictionary<string, object> Attributes { get; set; }

        private List<Models.UserClaim> _claims = new List<Models.UserClaim>();
        [SerializeIgnore(new string[] { "client" })]    
        public List<Models.UserClaim> Claims
        {
            get
            {
                //if (_roles != null && _roles.Count > 0)    //conversion for backwards compat
                //{
                //    AssignRoleIds(_roles);
                //    _roles.Clear();
                //}
                return _claims;
            }
            set
            {
                _claims = value;
            }
        }

        public UserClaim GetClaim(string type, string issuer)
        {
            return Claims.Where(c => c.Type.Equals(type, System.StringComparison.InvariantCultureIgnoreCase) && c.Issuer.Equals(issuer, System.StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public T GetClaimValue<T>(string type, string issuer, T defaultValue)
        {
            var claim = GetClaim(type, issuer);
            if (claim != null)
                return claim.Value.ToType<T>();
            return defaultValue;
        }

        //private void AssignRoleIds(List<string> roles)
        //{
        //    _claims.RemoveAll(c => c.Type == "VidereRole" && c.Issuer == PortalId);
        //    _claims.AddRange(roles.Select(r => new Models.UserClaim() { Type = "VidereRole", Issuer = PortalId, Value = r }));
        //}

        [SerializeIgnore(new string[] { "db" })]
        public List<Models.SecureActivity> SecureActivities
        {
            get
            {
                return Services.Security.GetAuthorizedSecureActivities(Id);
            }
        }

        public bool IsActivityAuthorized(string area, string name)
        {
            return SecureActivities.Exists(a => a.Area.Equals(area, System.StringComparison.InvariantCultureIgnoreCase) && a.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        }

        [SerializeIgnore(new string[] { "db" })]
        public bool IsEmailVerified
        {
            get
            {
                return GetClaimValue<string>("Account Verified On", "Videre Account Verification", null) != null;
            }
        }



    }
}
