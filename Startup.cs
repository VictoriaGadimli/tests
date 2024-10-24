using Microsoft.Extensions.DependencyInjection;
using ProjectX.Services;

namespace ProjectX;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<RestClient>(_ =>
            new RestClient("baseurl"));

        services.AddTransient<UserService>();
    }
}