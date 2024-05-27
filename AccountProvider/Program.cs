using AccountProvider.Services;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContext<DataContext>(x => x.UseSqlServer(context.Configuration.GetConnectionString("Accountdb")));

        services.AddIdentity<UserAccount, IdentityRole>(options => {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();

        services.AddHttpClient();
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddScoped<GenerateToken>();
    })
    .Build();

host.Run();
