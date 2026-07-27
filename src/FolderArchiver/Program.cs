using Avalonia;
using EvilBaschdi.About.Avalonia.DependencyInjection;
using EvilBaschdi.Core.Avalonia.AppBuilderImplementations;
using FolderArchiver.DependencyInjection;

namespace FolderArchiver;

internal class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => new AppBuilderImplementationToUseReactiveUIWithMicrosoftDependencyResolver<App>()
           .ValueFor(serviceCollection =>
                     {
                         serviceCollection.AddCoreServices();
                         serviceCollection.AddAboutServices();
                         serviceCollection.AddAvaloniaServices();
                         serviceCollection.AddWindowsAndViewModels();
                     })
#if DEBUG
           .WithDeveloperTools()
#endif
    ;
}
