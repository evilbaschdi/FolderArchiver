using EvilBaschdi.About.Core;
using EvilBaschdi.Core;
using EvilBaschdi.Core.Avalonia.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FolderArchiver.DependencyInjection;

/// <summary />
public static class ConfigureAvaloniaServices
{
    /// <summary />
    public static void AddAvaloniaServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICurrentAssembly, CurrentAssembly>();
        services.AddSingleton<IAboutContent, AboutContent>();
    }
}
