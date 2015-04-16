using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CoreServices = Videre.Core.Services;
using Videre.Core.Models;

namespace Videre.Core.UnitTests
{
    [TestClass]
    public class Authorization
    {
        [TestMethod]
        public void NotAuthenticatedUser()
        {
            CoreServices.IAuthorizationUser user = null;
            
            var noSecurityPage = new PageTemplate();
            var authenticatedPage = new PageTemplate() { Authenticated = true };
            var nonAuthenticatedPage = new PageTemplate() { Authenticated = false };
            var rolePage = new PageTemplate() { RoleIds = new List<string>() { "roleId1" } };
            var excludeRolePage = new PageTemplate() { ExcludeRoleIds = new List<string>() { "roleId1" } };
            var claimWidget = new Widget() { Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" } } };
            var complexWidget = new Widget() { Authenticated = true, RoleIds = new List<string>() { "roleId1" }, ExcludeRoleIds = new List<string>() { "roleId2" }, Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" } } };

            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, noSecurityPage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, authenticatedPage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, nonAuthenticatedPage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, rolePage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, excludeRolePage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, claimWidget));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, complexWidget));
        }

        [TestMethod]
        public void RoleUser()
        {
            CoreServices.IAuthorizationUser user = new AuthenticatedUser() { Id = "1", RoleIds = new List<string>() { "roleId1", "roleId2" } };

            var noSecurityPage = new PageTemplate();
            var authenticatedPage = new PageTemplate() { Authenticated = true };
            var nonAuthenticatedPage = new PageTemplate() { Authenticated = false };
            var rolePage = new PageTemplate() { RoleIds = new List<string>() { "roleId1" } };
            var excludeRolePage = new PageTemplate() { ExcludeRoleIds = new List<string>() { "roleId1" } };
            var claimWidget = new Widget() { Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" } } };
            var complexWidget = new Widget() { Authenticated = true, RoleIds = new List<string>() { "roleId1" }, ExcludeRoleIds = new List<string>() { "roleId2" }, Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" } } };

            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, noSecurityPage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, authenticatedPage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, nonAuthenticatedPage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, rolePage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, excludeRolePage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, claimWidget));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, complexWidget));   //exclude roleId2 trumps rest
        }

        [TestMethod]
        public void ClaimUser()
        {
            CoreServices.IAuthorizationUser user = new AuthenticatedUser() { Id = "1", Claims = new List<UserClaim>() { 
                new UserClaim() { Issuer = "A", Type = "B", Value = "C" }, 
                new UserClaim() { Issuer = "X", Type = "Y", Value = "12345" } } };

            var noSecurityPage = new PageTemplate();
            var authenticatedPage = new PageTemplate() { Authenticated = true };
            var nonAuthenticatedPage = new PageTemplate() { Authenticated = false };
            var rolePage = new PageTemplate() { RoleIds = new List<string>() { "roleId1" } };
            var excludeRolePage = new PageTemplate() { ExcludeRoleIds = new List<string>() { "roleId1" } };
            var claimWidget = new Widget() { Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" }, new UserClaim() { Issuer = "X", Type = "Y", Value = "C" } } };
            var wildcardClaimWidget = new Widget() { Claims = new List<UserClaim>() { new UserClaim() { Issuer = "AAA", Type = "BBB", Value = "CCC" }, new UserClaim() { Issuer = "X", Type = "Y", Value = "*" } } };
            var complexWidget = new Widget() { Authenticated = true, RoleIds = new List<string>() { "roleId1" }, ExcludeRoleIds = new List<string>() { "roleId2" }, Claims = new List<UserClaim>() { new UserClaim() { Issuer = "A", Type = "B", Value = "C" } } };

            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, noSecurityPage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, authenticatedPage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, nonAuthenticatedPage));
            Assert.IsFalse(CoreServices.Authorization.IsAuthorized(user, rolePage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, excludeRolePage));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, claimWidget));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, wildcardClaimWidget));
            Assert.IsTrue(CoreServices.Authorization.IsAuthorized(user, complexWidget));   
        }

    }
}
