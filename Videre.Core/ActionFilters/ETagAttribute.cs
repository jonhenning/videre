using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

//http://stackoverflow.com/questions/6642815/create-etag-filler-in-asp-net-mvc

public class ETagAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        filterContext.HttpContext.Response.Filter = new ETagFilter(filterContext.HttpContext.Response, filterContext.RequestContext.HttpContext.Request);
    }
}

public class ETagFilter : MemoryStream
{
    private HttpResponseBase _response = null;
    private HttpRequestBase _request;
    private Stream _filter = null;

    public ETagFilter(HttpResponseBase response, HttpRequestBase request)
    {
        _response = response;
        _request = request;
        _filter = response.Filter;
    }

    private string GetToken(Stream stream)
    {
        byte[] checksum = new byte[0];
        checksum = MD5.Create().ComputeHash(stream);
        return Convert.ToBase64String(checksum, 0, checksum.Length);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        byte[] data = new byte[count];
        Buffer.BlockCopy(buffer, offset, data, 0, count);
        var token = GetToken(new MemoryStream(data));

        string clientToken = _request.Headers["If-None-Match"];

        _response.Cache.SetLastModified(DateTime.UtcNow.AddDays(-30));  //allow chrome to cache this in memory cache...
        if (token != clientToken)
        {
            _response.AddHeader("ETag", token);
            _filter.Write(data, 0, count);
        }
        else
        {
            _response.SuppressContent = true;
            _response.StatusCode = 304;
            _response.StatusDescription = "Not Modified";
            _response.AddHeader("Content-Length", "0");
        }
    }
}