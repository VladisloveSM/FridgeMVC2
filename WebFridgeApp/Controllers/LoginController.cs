using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebFridgeApp.Models;

namespace WebFridgeApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly string apiFridge;

        public LoginController(IConfiguration configuration)
        {
            this.apiFridge = configuration["FridgeApi"];
        }

        [HttpGet]
        public IActionResult LoginForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginForm(InputLoginData data)
        {
            if (ModelState.IsValid)
            {
                Account account = new Account
                {
                    Id = Guid.NewGuid(),
                    Name = data.Name,
                    Hash = ComputeSha256Hash(data.Password),
                };

                using HttpClient client = new HttpClient();

                await client.PostAsJsonAsync($"{this.apiFridge}/Login", account);

                var result = await client.GetAsync($"{this.apiFridge}/Login?Name={account.Name}&hash={account.Hash}");

                Tokens tokens = HomeController.ParseJson<Tokens>(result.Content.ReadAsStringAsync().Result);

                HttpContext.Response.Cookies.Delete("token");
                HttpContext.Response.Cookies.Delete("refreshToken");
                HttpContext.Response.Cookies.Append("token", tokens.Token);
                HttpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken);

                return RedirectToAction("Index", "Home");
            }

            return View(data);
        }

        [HttpGet]
        public IActionResult SignForm()
        {
            return View();
        }

        public async Task<IActionResult> SignForm(LoginData data)
        {
            if (ModelState.IsValid)
            {
                using HttpClient client = new HttpClient();

                var hash = ComputeSha256Hash(data.Password);

                var result = await client.GetAsync($"{this.apiFridge}/Login?Name={data.Name}&hash={hash}");

                Tokens tokens = HomeController.ParseJson<Tokens>(result.Content.ReadAsStringAsync().Result);

                if (string.IsNullOrEmpty(tokens.Token) || string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    ViewBag.ErrorMessage = "You input wrong name or password";
                    return View();
                }

                HttpContext.Response.Cookies.Delete("token");
                HttpContext.Response.Cookies.Delete("refreshToken");
                HttpContext.Response.Cookies.Append("token", tokens.Token);
                HttpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken);

                return RedirectToAction("Index", "Home");
            }

            return View(data);
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
