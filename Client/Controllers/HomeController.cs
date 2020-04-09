﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            // get token from Http context
            var token = await HttpContext.GetTokenAsync("access_token");

            var serverClient = _httpClientFactory.CreateClient();
            // add token to header request
            serverClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Check that token still can authorize
            var response = await serverClient.GetAsync("http://localhost:62483/secret/index");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // refresh token here
                await RefreshAccessToken();
            }

            // call Api with token received
            var apiClient = _httpClientFactory.CreateClient();
            // Get new refresh token
            token = await HttpContext.GetTokenAsync("access_token");
            apiClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var apiResponse = await apiClient.GetAsync("http://localhost:54857/secret/index");

            return View();
        }

        public async Task RefreshAccessToken()
        {
            // Call to server side to get new token
            var refreshTokenClient = _httpClientFactory.CreateClient();

            var requestData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:62483/oauth/token")
            {
                Content = new FormUrlEncodedContent(requestData)
            };

            var basicCredentials = "username:password";
            var encodedCredentials = Encoding.UTF8.GetBytes(basicCredentials);
            var base64Credentials = Convert.ToBase64String(encodedCredentials);

            request.Headers.Add("Authorization", $"Basic {base64Credentials}");

            var response = await refreshTokenClient.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            var newAccessToken = responseData.GetValueOrDefault("access_token");
            var newRefreshToken = responseData.GetValueOrDefault("refresh_token");

            //re-write authentication
            var authInfo = await HttpContext.AuthenticateAsync("ClientCookie");

            authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

            //store again authenticate info
            await HttpContext.SignInAsync("ClientCookie", authInfo.Principal, authInfo.Properties);
        } 
    }
}