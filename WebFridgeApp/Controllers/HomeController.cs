using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WebFridgeApp.Client;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http.Json;
using WebFridgeApp.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;

namespace WebFridgeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string apiFridge;

        public HomeController(IConfiguration configuration)
        {
            this.apiFridge = configuration["FridgeApi"];
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Fridge(Guid id)
        {
            using AuthClient client = new AuthClient(HttpContext);
            var result = await client.GetAsync($"{this.apiFridge}/Product/{id}");
            bool? isOk = await this.CheckToRefresh(result, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("Fridge", id);
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            IEnumerable<FridgeProduct> products = ParseJson<IEnumerable<FridgeProduct>>(result.Content.ReadAsStringAsync().Result);

            var nameResult = await client.GetAsync($"{this.apiFridge}/Fridge/Name/{id}");

            isOk = await this.CheckToRefresh(nameResult, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("Fridge", id);
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            ViewBag.Title = $"All Products in Fridge: \"{nameResult.Content.ReadAsStringAsync().Result}\".";
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;

            var emptyResult = await client.GetAsync($"{this.apiFridge}/Product/Empty/{id}");
            isOk = await this.CheckToRefresh(emptyResult, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("Fridge", id);
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }
            IEnumerable<Product> productsEmpty = ParseJson<IEnumerable<Product>>(emptyResult.Content.ReadAsStringAsync().Result);
            if (productsEmpty.Any())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"Add: {productsEmpty.ElementAt(0).Name}");
                for (int i = 1; i < productsEmpty.Count(); i++)
                {
                    sb.Append($", {productsEmpty.ElementAt(i).Name}");
                }
                ViewBag.EmptyProducts = sb.ToString();
            }
            ViewBag.FridgeId = id;

            return View(products);
        }

        public async Task<IActionResult> List()
        {
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            using AuthClient client = new AuthClient(HttpContext);
            var result = await client.GetAsync($"{this.apiFridge}/Fridge");

            bool? isOk = await this.CheckToRefresh(result, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("List");
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            return View(ParseJson<IEnumerable<Fridge>>(result.Content.ReadAsStringAsync().Result));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            using AuthClient client = new AuthClient(HttpContext);
            var result = await client.DeleteAsync($"{this.apiFridge}/Fridge/{id}");

            bool? isOk = await this.CheckToRefresh(result, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("Delete", new { id });
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult ProductForm(Guid id)
        {
            this.ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductForm(FridgeProduct prod)
        {
            prod.FridgeId = prod.Id;
            this.ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            if (ModelState.IsValid)
            {
                prod.Id = Guid.NewGuid();
                
                using AuthClient client = new AuthClient(HttpContext);

                var result = await client.PostAsJsonAsync($"{this.apiFridge}/Product", prod);

                bool? isOk = await this.CheckToRefresh(result, HttpContext);
                if (isOk.HasValue && isOk.Value)
                {
                    return RedirectToAction("ProductForm", new { prod });
                }
                else if (isOk.HasValue && !isOk.Value)
                {
                    return RedirectToAction("SignForm", "Login");
                }

                return RedirectToAction("Fridge", new { id = prod.FridgeId });
            }

            return View(prod);
        }

        [HttpGet]
        public IActionResult FridgeForm()
        {
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FridgeForm(Fridge fridge)
        {
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            if (ModelState.IsValid)
            {
                fridge.Id = Guid.NewGuid();
                using AuthClient client = new AuthClient(HttpContext);

                var result = await client.PostAsJsonAsync($"{this.apiFridge}/Fridge", fridge);

                bool? isOk = await this.CheckToRefresh(result, HttpContext);
                if (isOk.HasValue && isOk.Value)
                {
                    return RedirectToAction("FridgeForm", new { fridge });
                }
                else if (isOk.HasValue && !isOk.Value)
                {
                    return RedirectToAction("SignForm", "Login");
                }


                return RedirectToAction("Fridge", new { fridge.Id });
            }

            return View(fridge);
        }

        [HttpGet]
        public IActionResult UpdateFridgeForm(Guid id)
        {
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFridgeForm(Fridge fridge)
        {
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            if (ModelState.IsValid)
            {
                using AuthClient client = new AuthClient(HttpContext);

                var result = await client.PutAsJsonAsync($"{this.apiFridge}/Fridge", fridge);

                bool? isOk = await this.CheckToRefresh(result, HttpContext);
                if (isOk.HasValue && isOk.Value)
                {
                    return RedirectToAction("UpdateFridgeForm", new { fridge });
                }
                else if (isOk.HasValue && !isOk.Value)
                {
                    return RedirectToAction("SignForm", "Login");
                }


                return RedirectToAction("List");
            }

            return View(fridge);
        }

        [HttpGet]
        public IActionResult UpdateProductForm(Guid id, Guid fridge_id)
        {
            ViewBag.ProductId = id;
            ViewBag.FridgeId = fridge_id;
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductForm(FridgeProduct prod)
        {
            ViewBag.FridgeId = prod.FridgeId;
            ViewBag.WebApiName = this.apiFridge;
            ViewBag.Context = HttpContext;
            if (ModelState.IsValid)
            {
                using AuthClient client = new AuthClient(HttpContext);

                var result = await client.PutAsJsonAsync($"{this.apiFridge}/Product", prod);

                bool? isOk = await this.CheckToRefresh(result, HttpContext);
                if (isOk.HasValue && isOk.Value)
                {
                    return RedirectToAction("UpdateProductForm", new { prod });
                }
                else if (isOk.HasValue && !isOk.Value)
                {
                    return RedirectToAction("SignForm", "Login");
                }

                return RedirectToAction("Fridge", new { id = prod.FridgeId });
            }

            return View(prod);
        }

        public async Task<IActionResult> GetFridgeWithEmptyProducts()
        {
            using AuthClient client = new AuthClient(HttpContext);
            var result = await client.GetAsync($"{this.apiFridge}/Fridge");

            bool? isOk = await this.CheckToRefresh(result, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("GetFridgeWithEmptyProducts");
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            IEnumerable<Guid> fridgesId = ParseJson<IEnumerable<Fridge>>(result.Content.ReadAsStringAsync().Result).Select(i => i.Id);
            foreach (Guid id in fridgesId)
            {
                var emptyResult = await client.GetAsync($"{this.apiFridge}/Product/Empty/{id}");
                isOk = await this.CheckToRefresh(emptyResult, HttpContext);
                if (isOk.HasValue && isOk.Value)
                {
                    return RedirectToAction("GetFridgeWithEmptyProducts");
                }
                else if (isOk.HasValue && !isOk.Value)
                {
                    return RedirectToAction("SignForm", "Login");
                }
                IEnumerable<Product> productsEmpty = ParseJson<IEnumerable<Product>>(emptyResult.Content.ReadAsStringAsync().Result);
                if (productsEmpty.Any())
                {
                    return RedirectToAction("Fridge", new { id });
                }
            }

            return RedirectToAction("List");
        }

        public async Task<IActionResult> RemoveProduct(Guid id, Guid fridge_id)
        {
            using AuthClient client = new AuthClient(HttpContext);
            var result = await client.DeleteAsync($"{this.apiFridge}/Product/{id}");

            bool? isOk = await this.CheckToRefresh(result, HttpContext);
            if (isOk.HasValue && isOk.Value)
            {
                return RedirectToAction("RemoveProduct", new { id, fridge_id});
            }
            else if (isOk.HasValue && !isOk.Value)
            {
                return RedirectToAction("SignForm", "Login");
            }

            return RedirectToAction("Fridge", new { id = fridge_id });
        }

        public static T ParseJson<T>(string jsonValue)
        {
            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(jsonValue, option);
        }


        private async Task<bool?> CheckToRefresh(HttpResponseMessage message, HttpContext context)
        {
            if (!message.IsSuccessStatusCode)
            {
                if (message.ReasonPhrase == "Unauthorized")
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(context.Request.Cookies["refreshToken"]);
                    var tokenS = jsonToken as JwtSecurityToken;
                    Account account = new Account()
                    {
                        Id = new Guid(tokenS.Claims.First(a => a.Type == "Id").Value),
                        RefreshToken = context.Request.Cookies["refreshToken"],
                    };
                    using HttpClient client = new HttpClient();


                    var result = await client.PostAsJsonAsync($"{this.apiFridge}/Login/Refresh", account);

                    if (string.IsNullOrEmpty(result.Content.ReadAsStringAsync().Result))
                    {
                        return false;
                    }
                    else
                    {
                        HttpContext.Response.Cookies.Delete("token");
                        HttpContext.Response.Cookies.Append("token", result.Content.ReadAsStringAsync().Result);
                        return true;
                    }
                }
            }

            return null;
        }

    }
}
