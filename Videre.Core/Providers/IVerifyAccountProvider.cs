using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Services;

//todo: change namespace to just Providers
namespace Videre.Core.Providers
{
    public interface IVerifyAccountProvider
    {
        string Name { get; }

        void Initialize(string connection);

        bool IsAccountVerified(IAuthorizationUser user);

        bool VerifyAccount(Models.User user, string verificationCode, bool trust);
        bool RemoveAccountVerification(Models.User user);

        bool ForceVerifyAccount(Models.User user);

        bool SendVerification(Models.User user, string code);

        bool IssueAccountVerificationCode(Models.User user, string code);

        string GenerateVerificationCode();

    }
}
