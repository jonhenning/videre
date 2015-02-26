using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public interface IAuthenticationProvider
    {
        string Name { get; }
        string LoginButtonText { get; }
        //bool Enabled { get; }
        bool AllowAssociation { get; }
        bool AllowLogin { get; }
        bool AllowCreation { get; }
        bool AllowDuplicateAssociation { get; }
        void Register();
    }

    public interface IStandardAuthenticationProvider : IAuthenticationProvider
    {
        AuthenticationResult Login(string userName, string password);
    }

    public interface IAuthenticationPersistence
    {
        string Name { get; }
        AuthenticationResult SaveAuthentication(string userId, string userName, string password);
        Models.UserAuthentication GetUserAuthentication(string userId); //needed for export
        Models.UserAuthentication SaveAuthentication(Models.UserAuthentication auth, string userId); //needed for import
        bool DeleteAuthentication(string id, string userId);
        void InitializePersistence(string connection);
    }

    public interface IOAuthAuthenticationProvider : IAuthenticationProvider
    {
        AuthenticationResult VerifyAuthentication(string returnUrl);
        void RequestAuthentication(string provider, string returnUrl);
    }

}
