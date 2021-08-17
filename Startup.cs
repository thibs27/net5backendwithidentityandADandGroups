using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using net5backendwithidentityandADandGroups.Infrastructure;
using net5backendwithidentityandADandGroups.Services;

namespace net5backendwithidentityandADandGroups
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
            var initialScopes = new string[] { "User.Read", "GroupMember.Read.All" };
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite
                options.HandleSameSiteCookieCompatibility();
            });

            // Sign-in users with the Microsoft identity platform
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                   .AddMicrosoftIdentityWebApp(
               options =>
               {
                   Configuration.Bind("AzureAd", options);
                   options.Events = new OpenIdConnectEvents();
                   options.Events.OnTokenValidated = async context =>
                   {
                        //Calls method to process groups overage claim.
                        var overageGroupClaims = await GraphHelper.GetSignedInUsersGroups(context);
                   };
               })
                   .EnableTokenAcquisitionToCallDownstreamApi(options => Configuration.Bind("AzureAd", options), initialScopes)
                   .AddMicrosoftGraph(Configuration.GetSection("GraphAPI"))
                   .AddInMemoryTokenCaches();

            // Adding authorization policies that enforce authorization using group values.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GroupAdmin",
                policy => policy.Requirements.Add(new GroupPolicyRequirement(Configuration["Groups:GroupAdmin"])));
                options.AddPolicy("GroupMember",
              policy => policy.Requirements.Add(new GroupPolicyRequirement(Configuration["Groups:GroupMember"])));
            });
            services.AddSingleton<IAuthorizationHandler, GroupPolicyHandler>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
            services.AddRazorPages();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}