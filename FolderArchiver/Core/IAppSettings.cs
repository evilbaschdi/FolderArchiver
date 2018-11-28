namespace FolderArchiver.Core
{
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
}