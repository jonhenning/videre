using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public interface IAuthenticationResetProvider
    {
        string Name { get; }
        void Register();
        void InitializePersistence(string connection);
        Services.AuthenticationResetResult IssueResetTicket(string userId, string url);
        Services.AuthenticationResetResult Authenticate(string userName, string password);
    }
}
