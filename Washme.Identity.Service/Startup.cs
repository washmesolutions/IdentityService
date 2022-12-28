using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Washme.Identity.Service.Data;
using Washme.Identity.Service.Models;

namespace Washme.Identity.Service
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
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("IdentityServerDatabase");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            //register ASP.NET Identity DbContext 
            services.AddDbContext<ApplicationDbContext>(builder =>
                builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationAssembly)));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                if (!double.TryParse(Configuration["UserSettings:DefaultLockoutTimeSpan"], out double defaultLockoutTimeSpan))
                {
                    defaultLockoutTimeSpan = 2;
                }

                if (!int.TryParse(Configuration["UserSettings:MaxFailedAccessAttempts"], out int maxFailedAccessAttempts))
                {
                    maxFailedAccessAttempts = 3;
                }

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(defaultLockoutTimeSpan);
                options.Lockout.MaxFailedAccessAttempts = maxFailedAccessAttempts;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Authentication = new AuthenticationOptions()
                {
                    CookieLifetime = TimeSpan.FromHours(10), // ID server cookie timeout set to 10 hours
                    CookieSlidingExpiration = true
                };
            })
              //.AddSigningCredential(certificate)
              .AddDeveloperSigningCredential()
              // ASP.NET Identity integration
              .AddAspNetIdentity<ApplicationUser>()
              .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
                // EF client, scope, and persisted grant stores
              .AddConfigurationStore(options =>
              {
                  options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
              })
                .AddOperationalStore(options =>
                {
                  options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly));
              });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Washme.IdentityProvider", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AESA.IdentityProvider.EFE v1"));
            }

            //context.Database.Migrate();

            //DatabaseInitializer.PopulateIdentityServer(app);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            InitializeDatabase(app);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();


                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                if (!userManager.Users.Any())
                {
                    foreach (var testUser in Config.GetUsers())
                    {
                        var identityUser = new ApplicationUser()
                        {
                            Id = testUser.SubjectId,
                            UserName = testUser.Username,
                        };

                        userManager.CreateAsync(identityUser, "Password123!").Wait();
                        userManager.AddClaimsAsync(identityUser, testUser.Claims.ToList()).Wait();
                    }
                }
            }
        }
    }
}
