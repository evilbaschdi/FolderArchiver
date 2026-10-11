using EvilBaschdi.About.Avalonia;
using EvilBaschdi.About.Avalonia.Models;
using FolderArchiver.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FolderArchiver.DependencyInjection;

/// <summary />
public static class ConfigureWindowsAndViewModels
{
    /// <summary />
    public static void AddWindowsAndViewModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAboutViewModelExtended, AboutViewModelExtended>();
        services.AddTransient<AboutWindow>();

        services.AddSingleton<ITopLevel, MainWindowTopLevel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}