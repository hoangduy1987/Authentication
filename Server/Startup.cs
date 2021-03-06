using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;

namespace Server
{
    public class Startup
    {
        // https://www.youtube.com/playlist?list=PLOeFnOV9YBa7dnrjpOG6lMpcyd7Wn7E8V
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("OAuth")
                    .AddJwtBearer("OAuth", config => 
                    {
                        var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
                        var key = new SymmetricSecurityKey(secretBytes);

                        // Set up for send token through url (require for send Authorization with Api)
                        config.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                if (context.Request.Query.ContainsKey("access_token"))
                                {
                                    context.Token = context.Request.Query["access_token"];
                                }

                                return Task.CompletedTask;
                            }
                        };

                        // Set up validation parameter
                        config.TokenValidationParameters = new TokenValidationParameters
                        {
                            ClockSkew = TimeSpan.Zero, // Make validate effect immediately
                            ValidIssuer = Constants.Issuer,
                            ValidAudience = Constants.Audience,
                            IssuerSigningKey = key
                        };
                    });

            // Add all authorize attr to all controllers
            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
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
