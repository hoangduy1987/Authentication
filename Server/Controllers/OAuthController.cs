using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            var query = new QueryBuilder();

            query.Add("redirectUri", redirect_uri);
            query.Add("state", state);

            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string username,
            string redirectUri,
            string state
            )
        {
            var query = new QueryBuilder();
            query.Add("code", "asfafdfdfs");
            query.Add("state", state);

            return Redirect($"{redirectUri}{query.ToString()}");
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

            var serectBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(serectBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                    Constants.Issuer,
                    Constants.Audiance,
                    claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials
                );

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthTutorial"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var repsonseBytes = Encoding.UTF8.GetBytes(responseJson);

            await Response.Body.WriteAsync(repsonseBytes, 0 , repsonseBytes.Length);

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