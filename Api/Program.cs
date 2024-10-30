using IpInformation;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {

        AppSettings settings = SettingsLoader.LoadSettings<AppSettings>("appsettings.json");
        
        webBuilder.UseUrls(settings.ApplicationUrl);
        webBuilder.UseStartup<Startup>();
    })
    .Build()
    .Run();