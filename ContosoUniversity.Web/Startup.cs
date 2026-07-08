using AutoMapper;
using ContosoUniversity.Common;
using ContosoUniversity.Common.Data;
using ContosoUniversity.Common.Interfaces;
using ContosoUniversity.Data;
using ContosoUniversity.Web;
using ContosoUniversity.Web.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            CurrentEnvironment = env;
            Configuration = config;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomizedContext(Configuration, CurrentEnvironment);
            services.AddCustomizedIdentity(Configuration, CurrentEnvironment);
            services.AddCustomizedAuthentication(Configuration);
            services.AddCustomizedMessage(Configuration);
            services.AddCustomizedMvc(CurrentEnvironment);
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<WebProfile>();
            });

            services.AddScoped<IDbInitializer, WebInitializer>();
            services.AddScoped<IModelBindingHelperAdaptor, DefaultModelBindingHelaperAdaptor>();
            services.AddScoped<IUrlHelperAdaptor, UrlHelperAdaptor>();
            services.AddSingleton<IConfiguration>(Configuration);

            // Call to change httpsport or redirect status code.
            // services.AddHttpsRedirection(options =>
            // {
            //     options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //     options.HttpsPort = 20650;
            // });
        }

        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IDbInitializer dbInitializer)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var webContext = scope.ServiceProvider.GetRequiredService<WebContext>();
                webContext.Database.Migrate();
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                dbInitializer.Initialize();
            }
            else if (env.IsProduction())
            {
                // app.UseExceptionHandler("/Home/Error");
            }

            // aspnetcore 2.1 Require HTTPS
            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-3.1&tabs=visual-studio
            // enable via config file
            var enableHttps = Configuration["EnableHttps"];
            if (!string.IsNullOrWhiteSpace(enableHttps) && enableHttps.ToLower() == "true")
            {
                // enable https redirection middleware
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            // Endpoint routing (required for ASP.NET Core 3.0+ / .NET Core 3+)
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // MVC controllers
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Razor Pages
                endpoints.MapRazorPages();
            });
        }
    }
}
