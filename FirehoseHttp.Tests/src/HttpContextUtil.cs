using System.Text;
using Microsoft.AspNetCore.Http;

namespace WitchPixels.FirehoseHttp.Tests;

public class HttpContextUtil
{
    public static HttpContext CreateWithBody(string body)
    {
        var httpContext = new DefaultHttpContext();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        httpContext.Request.Body = memoryStream;
        return httpContext;
    }
}