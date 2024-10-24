using Microsoft.Extensions.DependencyInjection;
using ProjectX.Services;

namespace ProjectX;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //To do: Move to config.json base url
        services.AddTransient<RestClient>(_ =>
            new RestClient("https://api.green.westeurope.azurecontainerapps.io")); 

        services.AddTransient<UserService>();
    }
}