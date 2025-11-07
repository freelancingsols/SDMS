using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SDMS.ContentManagementApi.BL.Interface;
using SDMS.ContentManagementApi.Implementation;
using SDMS.ContentManagementApi.Interface;
using SDMS.DL.MySql.Implementation;
using SDMS.DL.MySql.Interface;
using SDMS.Models.ContentManagementModels;

namespace SDMS.ContentManagementApi
{
    public class Startup
    {
        private readonly Container container = new Container();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            //injection pending
            services.Add(new ServiceDescriptor(typeof(IBanner), typeof(Banners), ServiceLifetime.Scoped));
            services.Add(new ServiceDescriptor(typeof(IBanners), typeof(BL.Implementation.Banners), ServiceLifetime.Scoped));
            services.Add(new ServiceDescriptor(typeof(ISqlDBOperationsEntity<Banner, int>), typeof(SqlDBOperationsEntity<Banner, int>), ServiceLifetime.Scoped));
            //services.Add(new ServiceDescriptor(typeof(ISqlDBOperations<Banner, int>), typeof(SqlDBOperationsEntity<Banner, int>), ServiceLifetime.Scoped));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

           // IntializeContainer(app);

            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}/{action}");
                endpoints.MapDefaultControllerRoute();
            });
        }
       
    }
}
