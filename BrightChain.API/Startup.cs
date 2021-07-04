using BrightChain.API.Areas.Identity;
using BrightChain.API.Data;
using BrightChain.EntityFrameworkCore.Data;
using BrightChain.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace BrightChain.API
{
    public class Startup
    {

        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            //services.AddPersistence<BrightChainBlockDbContext>(configuration: this.Configuration);
            //services.AddPersistence<BrightChainAPIUserContext>(configuration: this.Configuration);
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddDefaultIdentity<BrightChainUser>(options =>
                options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<BrightChainBlockDbContext>();


            //services.AddIdentity<BrightChainUser, MyRole>()
            //    .AddEntityFrameworkStores<BrightChainAPIUserContext>()
            //    .AddUserStore<MyUserStore>()
            //    .AddRoleStore<MyRoleStore>()
            //    .AddRoleManager<MyRoleManager>()
            //    .AddDefaultTokenProviders();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<BrightBlockService>();
            services.AddMediatR(Assembly.GetExecutingAssembly());
            #region API Versioning
            // Add API Versioning to the Project
            services.AddApiVersioning(setupAction: config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
