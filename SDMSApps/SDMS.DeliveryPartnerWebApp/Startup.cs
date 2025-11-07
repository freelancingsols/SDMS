using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;

namespace SDMS.DeliveryPartnerWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp/dist";
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            var fileProvider = new CompositeFileProvider
                (
                    new List<IFileProvider>()
                    {
                        //new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp","dist")),
                        new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp","dist","browser")),
                        //new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp","dist","server")),
                        new PhysicalFileProvider(Path.Combine(env.ContentRootPath,"wwwroot"))
                    }
                );
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                FileProvider = fileProvider,
                DefaultFileNames = new List<string>() { "index.html" }
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider
                //,
                //RequestPath = ""
            });
            //app.UseStaticFiles();
            //if (!env.IsDevelopment())
            //{
            //    app.UseSpaStaticFiles();
            //}

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            //app.UseSpa(spa =>
            //{
            //    // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //    // see https://go.microsoft.com/fwlink/?linkid=864501

            //    spa.Options.SourcePath = "ClientApp";

            //    if (env.IsDevelopment())
            //    {
            //        spa.UseAngularCliServer(npmScript: "start");
            //        spa.UseAngularCliServer(npmScript: "serve:ssr");
            //    }
            //});
        }
    }
}
