using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebFridgeApp.Client
{
    public class AuthClient : HttpClient
    {
        public AuthClient(HttpContext context) : base()
        {
            this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.Request.Cookies["token"]);
        }
    }
}
