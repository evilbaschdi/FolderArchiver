using EvilBaschdi.Core.Settings.ByMachineAndUser;

namespace FolderArchiver.Settings;

/// <inheritdoc />
public class InitialDirectoryFromSettings : IInitialDirectoryFromSettings
{
    private const string Key = "InitialDirectory";
    private readonly IAppSettingByKey _appSettingByKey;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="appSettingByKey"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public InitialDirectoryFromSettings(IAppSettingByKey appSettingByKey)
    {
        _appSettingByKey = appSettingByKey ?? throw new ArgumentNullException(nameof(appSettingByKey));
    }

    /// <inheritdoc cref="string" />
    public string Value
    {
        get => _appSettingByKey.ValueFor(Key);
        set => _appSettingByKey.RunFor(Key, value);
    }
}