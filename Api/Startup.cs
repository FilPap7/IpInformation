using Common.Cache;
using IpInformation.Helpers;
using Microsoft.EntityFrameworkCore;

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
                options.UseSqlServer(settings.ConnectionStrings.DefaultConnection);
            });

            services.AddScoped<ICacheService, CacheService>();

            services.AddSingleton<IHostedService, ScheduledDatabaseUpdateService>();

            services.AddHealthChecks();
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

            // Don't Redirect wrong urls to Index.html
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