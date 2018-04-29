using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DutchTreat.Services;
using DutchTreat.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DutchTreat
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _env = env;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<StoreUser, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
            })
                    .AddEntityFrameworkStores<DutchContext>();

            services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = _config["Tokens:Issuer"],
                        ValidAudience = _config["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
                    };
                });

            services.AddDbContext<DutchContext>(cfg =>
            {
                cfg.UseSqlServer(_config.GetConnectionString("DutchConnectionString"));
            });
            services.AddAutoMapper();
            services.AddTransient<IMailService, NullMailService>();
            services.AddTransient<DutchSeeder>();
            services.AddScoped<IDutchRepository, DutchRepository>();
            services.AddMvc(opt =>
            {
                if(_env.IsProduction())
                {
                    opt.Filters.Add(new RequireHttpsAttribute());
                }
            })
                .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(cfg =>
            {
                cfg.MapRoute("Default", 
                    "{controller}/{action}/{id?}", 
                    new { controller = "App", Action = "Index"});
            });

            if(env.IsDevelopment())
            {
                // Seed the database
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var seeder = scope.ServiceProvider.GetService<DutchSeeder>();
                    seeder.Seed().Wait();
                }
            }
        }
    }
}
