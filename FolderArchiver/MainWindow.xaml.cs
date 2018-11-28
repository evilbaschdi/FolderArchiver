using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Extensions;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Model;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Metro;
using FolderArchiver.Core;
using FolderArchiver.Properties;
using MahApps.Metro.Controls;

namespace FolderArchiver
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly IApplicationStyle _applicationStyle;
        private readonly IAppSettings _appSettings;
        private string _initialDirectory;
        private int _overrideProtection;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            IApplicationStyleSettings applicationStyleSettings = new ApplicationStyleSettings(appSettingsBase);
            IThemeManagerHelper themeManagerHelper = new ThemeManagerHelper();
            _applicationStyle = new ApplicationStyle(this, Accent, ThemeSwitch, applicationStyleSettings, themeManagerHelper);
            _applicationStyle.Load(true);

            _appSettings = new AppSettings(appSettingsBase);
            var linkerTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            LinkerTime.Content = linkerTime.ToString(CultureInfo.InvariantCulture);
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
            if (!Directory.Exists(InitialDirectory.Text))
            {
                return;
            }

            _appSettings.InitialDirectory = InitialDirectory.Text;
            Load();
        }

        private async void ArchiveFoldersOnClick(object sender, RoutedEventArgs e)
        {
            await RunArchiveFoldersAsync();
        }

        private async Task RunArchiveFoldersAsync()
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            Cursor = Cursors.Wait;

            var task = Task<string>.Factory.StartNew(ArchiveFolders);
            await task;

            ArchiveFolderContent.Text = task.Result;

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            Cursor = Cursors.Arrow;
        }

        private string ArchiveFolders()
        {
            var multiThreadingHelper = new MultiThreading();
            var filePath = new FileListFromPath(multiThreadingHelper);
            var files = filePath.ValueFor(_initialDirectory, new FileListFromPathFilter());

            var counter = 0;

            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var createTime = File.GetCreationTime(path);
                var directory = path.Replace(fileName, string.Empty);
                var archiveTime = $@"{createTime.Year}\{createTime.Month.ToString().PadLeft(2, '0')}";
                var archiveDirectory = $@"{directory}\{archiveTime}";
                var archiveFilename = $@"{archiveDirectory}\{fileName}";
                //debug
                if (path.Contains(archiveTime))
                {
                    continue;
                }

                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }

                File.Move(path, archiveFilename);
                counter++;
            }

            var pluralHelper = counter != 1
                ? "files were"
                : "file was";

            return counter != 0
                ? $"{counter} {pluralHelper} archived."
                : "Nothing has changed.";
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var browser = new ExplorerFolderBrowser
                          {
                              SelectedPath = _initialDirectory
                          };
            browser.ShowDialog();
            _appSettings.InitialDirectory = browser.SelectedPath;
            Load();
        }

        #region Fly-out

        private void ToggleSettingsFlyOutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyOut(0);
        }

        private void ToggleFlyOut(int index, bool stayOpen = false)
        {
            var activeFlyOut = (Flyout) Flyouts.Items[index];
            if (activeFlyOut == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyOut in
                Flyouts.Items.Cast<Flyout>()
                       .Where(nonactiveFlyOut => nonactiveFlyOut.IsOpen && nonactiveFlyOut.Name != activeFlyOut.Name))
            {
                nonactiveFlyOut.IsOpen = false;
            }

            if (activeFlyOut.IsOpen && stayOpen)
            {
                activeFlyOut.IsOpen = true;
            }
            else
            {
                activeFlyOut.IsOpen = !activeFlyOut.IsOpen;
            }
        }

        #endregion Fly-out

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SaveStyle();
        }

        private void Theme(object sender, EventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SetTheme(sender);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }

            _applicationStyle.SetAccent(sender, e);
        }

        #endregion MetroStyle
    }
}