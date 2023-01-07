using EvilBaschdi.Settings.ByMachineAndUser;

namespace FolderArchiver.Settings;

/// <inheritdoc />
public class AppSettings : IAppSettings
{
    private const string Key = "InitialDirectory";
    private readonly IAppSettingByKey _appSettingByKey;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="appSettingByKey"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AppSettings([NotNull] IAppSettingByKey appSettingByKey)
    {
        _appSettingByKey = appSettingByKey ?? throw new ArgumentNullException(nameof(appSettingByKey));
    }

    /// <inheritdoc />
    public string InitialDirectory
    {
        get => _appSettingByKey.ValueFor(Key);
        set => _appSettingByKey.RunFor(Key, value);
    }
}