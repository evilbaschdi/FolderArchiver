namespace FolderArchiver.Core
{
    public class AppSettings : IAppSettings
    {
        public string InitialDirectory
        {
            get => string.IsNullOrWhiteSpace(Properties.Settings.Default.InitialDirectory)
                ? ""
                : Properties.Settings.Default.InitialDirectory;
            set
            {
                Properties.Settings.Default.InitialDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}