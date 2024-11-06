using Common.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Configuration;

namespace IpInformation
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            AppSettings settings = SettingsLoader.LoadSettings<AppSettings>("appsettings.json");
            services.AddSingleton(settings);

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddDbContext<DataAccess.Data.IpInformationDbContext>(options =>
            {
                options.UseSqlServer(settings.ConnectionStrings.WorkConnectionString);
            });

            services.AddHealthChecks();

            services.AddScoped<ICacheService, CacheService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            // app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Map root URL to index.html
                endpoints.MapFallbackToController("Index", "Home");
            });
        }
    }
}