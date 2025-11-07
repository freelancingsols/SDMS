using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SDMS.AuthenticationApi.Helper;
using SDMS.AuthenticationApi.Models;
using SDMS.AuthenticationApi.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace SDMS.AuthenticationApi
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
            //services.AddControllers(); ;
            services.AddControllersWithViews();

            // cookie policy to deal with temporary browser incompatibilities
            //services.AddSameSiteCookiePolicy();
            //Server=localhost;Database=mydb;Uid=sa;Pwd=1234;
            services.AddDbContext<MyDbContext>(
            options =>
            {
                options.UseMySQL("Server=localhost;Database=db_1;Uid=root;Pwd=1234;");
            });
            //services.AddDbContext<MyApiAuthorizationDbContext>(
            //options =>
            //{
            //    options.UseMySQL("Server=92.249.44.156;Database=u104383566_store;Uid=u104383566_store;Pwd=Magdum@123;");
            //});
            var opt = new DbContextOptionsBuilder();
            opt.UseMySQL("Server=localhost;Database=db_1;Uid=root;Pwd=1234;");
            var c = new MyDbContext(opt.Options);
            c.Database.EnsureCreated();
            services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<MyDbContext>()
            //.AddEntityFrameworkStores<MyApiAuthorizationDbContext>()
            .AddDefaultTokenProviders();
            services.AddOptions();
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                //options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                // Default SignIn settings.
                // options.SignIn.RequireConfirmedEmail = false;
                //options.SignIn.RequireConfirmedPhoneNumber = false;
            });
            services.Configure<PasswordHasherOptions>(option =>
            {
                option.IterationCount = 12000;
            });
            services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "http://localhost:5001/login";
                options.UserInteraction.LoginReturnUrlParameter = "ReturnUrl";
                options.UserInteraction.ErrorUrl = "http://localhost:5001/error";
                options.UserInteraction.LogoutUrl = "http://localhost:5001/logout";
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Endpoints.EnableAuthorizeEndpoint = true;
                options.Endpoints.EnableDiscoveryEndpoint = true;
                options.Endpoints.EnableTokenEndpoint = true;
                options.Endpoints.EnableUserInfoEndpoint = true;

            })
            //.AddApiAuthorization<User, MyApiAuthorizationDbContext>()

            //.AddInMemoryApiScopes(StaticDataHelper.GetApiScopes())xxx

            .AddInMemoryApiResources(StaticDataHelper.GetApis())
            .AddInMemoryIdentityResources(StaticDataHelper.GetIdentityResources())
            .AddInMemoryClients(StaticDataHelper.GetClients())
            .AddClientStore<ClientStoreHelper>()
            .AddAspNetIdentity<User>()
            .AddDeveloperSigningCredential();

            services.AddAuthentication().AddIdentityServerJwt();//ntc???
                                                                //.AddGoogle("Google", options =>
                                                                //{
                                                                //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //    options.ClientId = Configuration["Secret:GoogleClientId"];
            //    options.ClientSecret = Configuration["Secret:GoogleClientSecret"];
            //});
            //.AddOpenIdConnect("aad", "Sign-in with Azure AD", options =>
            //{
            //    options.Authority = "https://login.microsoftonline.com/common";
            //    options.ClientId = "https://leastprivilegelabs.onmicrosoft.com/38196330-e766-4051-ad10-14596c7e97d3";

            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

            //    options.ResponseType = "id_token";
            //    options.CallbackPath = "/signin-aad";
            //    options.SignedOutCallbackPath = "/signout-callback-aad";
            //    options.RemoteSignOutPath = "/signout-aad";

            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = false,
            //        ValidAudience = "165b99fd-195f-4d93-a111-3e679246e6a9",

            //        NameClaimType = "name",
            //        RoleClaimType = "role"
            //    };
            //});
            //.AddLocalApi(options =>
            //{
            //    options.ExpectedScope = "api";
            //});

            // preserve OIDC state in cache (solves problems with AAD and URL lenghts)
            //services.AddOidcStateDataFormatterCache("aad");
            var urls=Configuration.GetSection("ApplicationSettings:ReturnUrls").Get<string[]>();
            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.WithOrigins(urls);
                    policy.AllowCredentials();
                });
            });
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.RequireHeaderSymmetry = false;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.AddTransient<IReturnUrlParser, SDMS.AuthenticationApi.Helper.ReturnUrlParser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                await next.Invoke();
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && !context.Request.Path.Value.Contains("/api"))
                {
                    context.Request.Path = new PathString("/index.html");
                    await next.Invoke();
                }
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();

            //app.UseHttpsRedirection();
            var fileProvider = new CompositeFileProvider
                (
                    new List<IFileProvider>()
                    {
                        //new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp","dist")),
                        new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp","dist","WebApp")),
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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowAnyOrigin();
            });
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
            app.UseIdentityServer();
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
