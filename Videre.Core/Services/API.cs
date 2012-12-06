using System;
using Videre.Core.ActionResults;

namespace Videre.Core.Services
{
    public class API
    {
        public delegate void JsonResultHandler<T>(JsonResult<T> Result) where T : new();
        public static JsonResult<T> Execute<T>(JsonResultHandler<T> CodeFunc) where T : new()
        {
            var result = new JsonResult<T>();
            try
            {
                CodeFunc(result);
            }
            catch (Exception ex)
            {
                //todo: some flag needed to show friendly errors 
                result.AddError(ex);
                if (ex.InnerException != null)
                    result.AddError(ex.InnerException);
            }
            return result;
        }

    }
}
