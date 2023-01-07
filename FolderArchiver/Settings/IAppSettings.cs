namespace FolderArchiver.Settings;

/// <summary>
///     AppSettings
/// </summary>
public interface IAppSettings
{
    /// <summary>
    ///     Folder to archive
    /// </summary>
    string InitialDirectory { get; set; }
}