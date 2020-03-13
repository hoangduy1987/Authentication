using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            // add token to header request
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _client.GetAsync($"{Constants.Audiance}/secret/index");

            var apiResponse = await _client.GetAsync("http://localhost:54857/secret/index");

            return View();
        }
    }
}