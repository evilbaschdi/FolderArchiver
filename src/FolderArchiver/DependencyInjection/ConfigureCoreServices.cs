using EvilBaschdi.Core.Settings.ByMachineAndUser;
using FolderArchiver.Internal;
using FolderArchiver.Settings;
using Microsoft.Extensions.DependencyInjection;

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