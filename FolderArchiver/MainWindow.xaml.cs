using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Model;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Metro;
using EvilBaschdi.CoreExtended.Mvvm;
using EvilBaschdi.CoreExtended.Mvvm.View;
using EvilBaschdi.CoreExtended.Mvvm.ViewModel;
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
        private readonly IAppSettings _appSettings;
        private readonly IThemeManagerHelper _themeManagerHelper;
        private string _initialDirectory;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            _themeManagerHelper = new ThemeManagerHelper();
            IApplicationStyle applicationStyle = new ApplicationStyle(_themeManagerHelper);
            applicationStyle.Load(true);

            _appSettings = new AppSettings(appSettingsBase);

            Load();
        }

        private void Load()
        {
            ArchiveFolder.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory);

            _initialDirectory = _appSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory;
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

        private void AboutWindowClick(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            IAboutWindowContent aboutWindowContent = new AboutWindowContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\b.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _themeManagerHelper)
                              };

            aboutWindow.ShowDialog();
        }
    }
}