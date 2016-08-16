using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.Threading;
using EvilBaschdi.Core.Wpf;
using FolderArchiver.Core;
using MahApps.Metro.Controls;

namespace FolderArchiver
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly IMetroStyle _style;
        private readonly IAppSettings _appSettings;
        private string _initialDirectory;
        private int _overrideProtection;

        public MainWindow()
        {
            _appSettings = new AppSettings();
            var coreSettings = new CoreSettings();
            InitializeComponent();
            _style = new MetroStyle(this, Accent, Dark, Light, coreSettings);
            _style.Load();
            Load();
        }

        private void Load()
        {
            ArchiveFolder.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory);

            _initialDirectory = _appSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory;

            _overrideProtection = 1;
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InitialDirectory.Text))
            {
                _appSettings.InitialDirectory = InitialDirectory.Text;
                Load();
            }
        }

        private void ArchiveFoldersOnClick(object sender, RoutedEventArgs e)
        {
            var multiThreadingHelper = new MultiThreadingHelper();
            var filePath = new FilePath(multiThreadingHelper);
            var files = filePath.GetFileList(_initialDirectory);

            var counter = 0;

            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var createTime = File.GetCreationTime(path);
                    var directory = path.Replace(fileName, string.Empty);
                    var archiveTime = $@"{createTime.Year}\{createTime.Month.ToString().PadLeft(2, '0')}";
                    var archiveDirectory = $@"{directory}\{archiveTime}";
                    var archiveFilename = $@"{archiveDirectory}\{fileName}";
                    //debug
                    if (!path.Contains(archiveTime))
                    {
                        if (!Directory.Exists(archiveDirectory))
                        {
                            Directory.CreateDirectory(archiveDirectory);
                        }

                        File.Move(path, archiveFilename);
                        counter++;
                    }
                }
            }

            var pluralHelper = counter != 1
                ? "files were"
                : "file was";

            ArchiveFolderContent.Text = counter != 0
                ? $"{counter} {pluralHelper} archived."
                : "Nothing has changed.";
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var browser = new ExplorerFolderBrower
                          {
                              SelectedPath = _initialDirectory
                          };
            browser.ShowDialog();
            _appSettings.InitialDirectory = browser.SelectedPath;
            Load();
        }

        #region Flyout

        private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private void ToggleFlyout(int index, bool stayOpen = false)
        {
            var activeFlyout = (Flyout) Flyouts.Items[index];
            if (activeFlyout == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyout in
                Flyouts.Items.Cast<Flyout>()
                       .Where(nonactiveFlyout => nonactiveFlyout.IsOpen && nonactiveFlyout.Name != activeFlyout.Name))
            {
                nonactiveFlyout.IsOpen = false;
            }

            if (activeFlyout.IsOpen && stayOpen)
            {
                activeFlyout.IsOpen = true;
            }
            else
            {
                activeFlyout.IsOpen = !activeFlyout.IsOpen;
            }
        }

        #endregion Flyout

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SaveStyle();
        }

        private void Theme(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetTheme(sender, e);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetAccent(sender, e);
        }

        #endregion MetroStyle
    }
}