using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TournamentPage.Data;
using TournamentPage.Models;
using TournamentPage.Services;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Owin;
using TournamentPage.Controllers;

namespace TournamentPage
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<NorwegianIdentityErrorDescriber>() /* overrides the english Identity error messages */
                .AddDefaultTokenProviders();

            services.AddMvc();
            services.AddHangfire(config=>config.UseSqlServerStorage(Configuration.GetConnectionString("HangFireConnectionString")));

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<BackgroundService>();

            //Add Hangfire services
            
        }

          // Called by Configure(). This needs an async function because
        // all the userManager and roleManager functions are async.
        public async Task CreateUsersAndRoles(IServiceScope serviceScope)
        {
            var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            // First create the admin role
            await roleManager.CreateAsync(new IdentityRole("Admin"));

            // Then add one admin user
            var adminUser1 = new ApplicationUser { UserName = "mariuf13@student.uia.no", Email = "mariuf13@student.uia.no", FirstName = "Marius", LastName = "Flottorp",
            NickName = "mflott", ProfilePicture = "/default.jpg", PostalCode = "4877", Address = "Tønnevoldsgate 26", BirthDate = DateTime.Parse("1990-08-02 00:00:00"), Town = "Grimstad", PhoneNumber = "95244241", RegisterDate = DateTime.Now, Gender = "Mann"};
            await userManager.CreateAsync(adminUser1, "Password1.");
            await userManager.AddToRoleAsync(adminUser1, "Admin");

            var adminUser2 = new ApplicationUser { UserName = "sondsg12@student.uia.no", Email = "sondsg12@student.uia.no", FirstName = "Sondre", LastName = "Grimsrud",
            NickName = "sondre", ProfilePicture = "/default.jpg", PostalCode = "4877", Address = "Home", BirthDate = DateTime.Parse("1989-08-02 00:00:00"), Town = "Grimstad", PhoneNumber = "99999999", RegisterDate = DateTime.Now, Gender = "Mann"};
            await userManager.CreateAsync(adminUser2, "Password1.");
            await userManager.AddToRoleAsync(adminUser2, "Admin");

            var adminUser3 = new ApplicationUser { UserName = "bennyb07@student.uia.no", Email = "bennyb07@student.uia.no", FirstName = "Benny", LastName = "Byremo",
            NickName = "benny", ProfilePicture = "/default.jpg", PostalCode = "4877", Address = "Home", BirthDate = DateTime.Parse("2000-08-02 00:00:00"), Town = "Grimstad", PhoneNumber = "99999999", RegisterDate = DateTime.Now, Gender = "Mann"};
            await userManager.CreateAsync(adminUser3, "Password1.");
            await userManager.AddToRoleAsync(adminUser3, "Admin");

            var adminUser4 = new ApplicationUser { UserName = "ivanad08@student.uia.no", Email = "ivanad08@student.uia.no", FirstName = "Ivana", LastName = "Divic",
            NickName = "ivana", ProfilePicture = "/default.jpg", PostalCode = "4877", Address = "Home", BirthDate = DateTime.Parse("2001-08-02 00:00:00"), Town = "Grimstad", PhoneNumber = "99999999", RegisterDate = DateTime.Now, Gender = "Kvinne"};
            await userManager.CreateAsync(adminUser4, "Password1.");
            await userManager.AddToRoleAsync(adminUser4, "Admin");


            // Add one regular user
            var userUser = new ApplicationUser { UserName = "user@example.com", Email = "user@example.com", FirstName = "regular", LastName = "user",
            NickName = "regular testuser", ProfilePicture = "/default.jpg", PostalCode = "4877", Address = "Home", Town = "Grimstad", PhoneNumber = "99999999", RegisterDate = DateTime.Now, Gender = "Mann"};
            await userManager.CreateAsync(userUser, "Password1.");
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseHangfireServer();
            // Map Dashboard to the `http://<your-app>/hangfire` URL.
            app.UseHangfireDashboard();

            if (env.IsDevelopment())
            {

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var db = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                    var um = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Create the standard users and roles BEFORE the rest of the test data.
                    CreateUsersAndRoles(serviceScope).Wait();

                    // Add regular test data here
                    db.RegisterType.AddRange(new List<RegisterType>(){
                        new RegisterType("Lag"),
                        new RegisterType("Individuell"),
                        new RegisterType("Lag og individuell")
                    });
                    db.TournamentType.AddRange(new List<TournamentType>(){
                        new TournamentType("Gruppespill"),
                        new TournamentType("Sluttspill"),
                        new TournamentType("Gruppespill og sluttspill")
                    });
                    db.SportType.AddRange(new List<SportType>(){
                        new SportType("Håndball"),
                        new SportType("Fotball"),
                        new SportType("Basketball"),
                        new SportType("Tennis"),
                        new SportType("Svømming"),
                        new SportType("Annet")
                    });
                    db.GenderType.AddRange(new List<GenderType>(){
                        new GenderType("Jenter"),
                        new GenderType("Gutter"),
                        new GenderType("Begge")
                    });

                    db.SaveChanges();

                    /* Syntax for creating a new Recurring job from the BackgroundService class */
                    RecurringJob.AddOrUpdate<BackgroundService>("RegisterCheck",s => s.CheckTournamentRegisterDate(), Cron.Minutely);
                }

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                
                app.UseBrowserLink();

               

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
