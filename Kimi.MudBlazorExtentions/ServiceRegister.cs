using Microsoft.Extensions.DependencyInjection;

namespace Kimi.MudBlazorExtentions;

public static class ServiceRegister
{
    public static void UseKimiMudBlazorExtentions(this IServiceCollection service)
    {
        service.AddScoped<KimiJsInterop>();
    }
}
