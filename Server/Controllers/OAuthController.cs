using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Server.Controllers
{
    public class OAuthController : Controller
    {
        [HttpGet]
        public IActionResult Authorize(
            string response_type, // authorization flow type
            string client_id,  // client id
            string redirect_uri, // 
            string scope, // what info I want = email, grandma, tel
            string state // random string generated to confirm that we are going to back to the same client
            )
        {
            var query = new QueryBuilder
            {
                { "redirect_uri", redirect_uri },
                { "state", state }
            };

            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string username,
            string redirect_uri,
            string state
            )
        {
            var query = new QueryBuilder
            {
                { "code", "thisismycode" },
                { "state", state }
            };

            return Redirect($"{redirect_uri}{query}");
        }

        public async Task<IActionResult> Token(
            string grant_type, // flow of access_token request
            string code, // confirmation of authentication process
            string redirect_uri,
            string client_id
            )
        {
            // some mechanism for validating the code
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")
            };

            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            const string algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                    Constants.Issuer,
                    Constants.Audience,
                    claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddMilliseconds(1),
                    signingCredentials
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token = accessToken,
                token_type = "Bearer",
                raw_claim = "oauthTutorial"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            await Response.Body.WriteAsync(responseBytes, 0 , responseBytes.Length);

            return Redirect(redirect_uri);
        }

        /// <summary>
        /// Validate the token
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}