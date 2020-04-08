using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Client
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        // https://tools.ietf.org/html/rfc6749#section-4.1
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(config => {
                //We check the cookie to confirm that we are authenticated
                config.DefaultAuthenticateScheme = "ClientCookie";
                // When we sign in we will deal out a cookie
                config.DefaultSignInScheme = "ClientCookie";
                // Use this to check if we are allowed to do something
                config.DefaultChallengeScheme = "OurServer";
            })
            .AddCookie("ClientCookie")
            .AddOAuth("OurServer", config => {
                config.ClientId = "client_id";
                config.ClientSecret = "client_secret";
                config.CallbackPath = "/oauth/callback";
                config.AuthorizationEndpoint = "http://localhost:62483/oauth/authorize";
                config.TokenEndpoint = "http://localhost:62483/oauth/token";

                config.SaveTokens = true;
                config.Events = new OAuthEvents()
                {
                    OnCreatingTicket = context =>
                    {
                        //var accessToken = context.AccessToken;
                        //var payload = accessToken.Split('.')[1];
                        //var bytes = Convert.FromBase64String(payload);
                        //var jsonPayload = Encoding.UTF8.GetString(bytes);
                        //var claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload);

                        //foreach (var claim in claims)
                        //{
                        //    context.Identity.AddClaim(new Claim(claim.Key, claim.Value));
                        //}

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpClient();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // who are you ?
            app.UseAuthentication();

            // Do you allow
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
