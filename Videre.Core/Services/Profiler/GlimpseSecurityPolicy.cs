using Glimpse.AspNet.Extensions;
using Glimpse.Core.Extensibility;

namespace Videre.Core.Services.Profiler
{
    public class GlimpseSecurityPolicy:IRuntimePolicy
    {
        public RuntimePolicy Execute(IRuntimePolicyContext policyContext)
        {
            if (Videre.Core.Services.Authentication.IsAuthenticated &&  Videre.Core.Services.Account.CurrentUser.IsActivityAuthorized("Profiler", "Glimpse"))
                return RuntimePolicy.On;

            return RuntimePolicy.Off;
        }

        public RuntimeEvent ExecuteOn
        {
			// The RuntimeEvent.ExecuteResource is only needed in case you create a security policy
			// Have a look at http://blog.getglimpse.com/2013/12/09/protect-glimpse-axd-with-your-custom-runtime-policy/ for more details
            get { return RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource; }
        }
    }
}
