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
                result.AddError(ex);
            }
            return result;
        }

    }
}
