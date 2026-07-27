using EvilBaschdi.Core.Settings.ByMachineAndUser;
using Microsoft.Extensions.DependencyInjection;
using FolderArchiver.Internal;
using FolderArchiver.Settings;

namespace FolderArchiver.DependencyInjection;

/// <summary />
public static class ConfigureCoreServices
{
    /// <summary />
    public static void AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAppSettingByKey, AppSettingByKey>();
        services.AddSingleton<IAppSettingsFromJsonFile, AppSettingsFromJsonFile>();
        services.AddSingleton<IAppSettingsFromJsonFileByMachineAndUser, AppSettingsFromJsonFileByMachineAndUser>();

        services.AddSingleton<IInitialDirectoryFromSettings, InitialDirectoryFromSettings>();
        services.AddSingleton<IArchiveFolders, ArchiveFolders>();
    }
}
