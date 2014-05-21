using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Helpers;
using Videre.Core.ActionResults;
using System.Web;

namespace Videre.Core.Services
{
    public class API
    {
        public delegate void JsonResultHandler<T>(JsonResult<T> result) where T : new();
        public static JsonResult<T> Execute<T>(JsonResultHandler<T> codeFunc) where T : new()
        {
            return Execute<T>(codeFunc, true);
        }

        public static JsonResult<T> Execute<T>(JsonResultHandler<T> codeFunc, bool verifyAntiForgeryToken) where T : new()
        {
            var result = new JsonResult<T>();
            try
            {
                if (verifyAntiForgeryToken && AntiForgeryTokenVerification)
                    VerifyAntiForgeryToken();
                codeFunc(result);
            }
            catch (Exception ex)
            {
                //todo: some flag needed to show friendly errors 
                result.AddError(ex);
                Logging.Logger.Error("API Error", ex);
                if (ex.InnerException != null)
                    result.AddError(ex.InnerException);
            }
            return result;
        }

        public static bool AntiForgeryTokenVerification
        {
            get
            {
                return Portal.GetAppSetting("AntiForgeryTokenVerification", true);
            }
        }

        public static string GetAntiForgeryToken()
        {
            string cookieToken;
            string formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            return cookieToken + ":" + formToken;
        }

        public static void VerifyAntiForgeryToken()
        {
            string cookieToken = "";
            string formToken = "";
            var request = HttpContext.Current.Request;

            if (request.Headers["RequestVerificationToken"] != null)
            {
                string[] tokens = request.Headers["RequestVerificationToken"].ToString().Split(':');
                if (tokens.Length == 2)
                {
                    cookieToken = tokens[0].Trim();
                    formToken = tokens[1].Trim();
                }
            }
            AntiForgery.Validate(cookieToken, formToken);
        }

    }
}
