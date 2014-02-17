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
        bool Enabled { get; }
        void Register();
    }

    public interface IStandardAuthenticationProvider : IAuthenticationProvider
    {
        AuthenticationResult Login(string userName, string password);
    }

    public interface IAuthenticationPersistance
    {
        AuthenticationResult SaveAuthentication(string userId, string userName, string password);
        void InitializePersistance(string connection);
    }

    public interface IExternalAuthenticationProvider : IAuthenticationProvider
    {
        AuthenticationResult VerifyAuthentication(string returnUrl);
        void RequestAuthentication(string provider, string returnUrl);
    }

}
