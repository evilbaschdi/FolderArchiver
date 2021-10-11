using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Model;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Controls.About;
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
        private string _initialDirectory;
        private readonly IRoundCorners _roundCorners;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            _roundCorners = new RoundCorners();
            IApplicationStyle style = new ApplicationStyle(_roundCorners, true);
            style.Run();

            _appSettings = new AppSettings(appSettingsBase);

            Load();
        }

        private void Load()
        {
            ArchiveFolder.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) &&
                                      Directory.Exists(_appSettings.InitialDirectory);

            _initialDirectory = _appSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory ?? string.Empty;
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
            var filePath = new FileListFromPath();
            var files = filePath.ValueFor(_initialDirectory, new FileListFromPathFilter());

            var counter = 0;

            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                var fileDate = FileDate(path);
                var archiveTime = $@"{fileDate.Year}\{fileDate.Month.ToString().PadLeft(2, '0')}";
                var archiveDirectory = $@"{_initialDirectory}\{archiveTime}";
                var archiveFilename = $@"{archiveDirectory}\{fileName}";


                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }

                if (!path.Equals(archiveFilename) && !File.Exists(archiveFilename))
                {
                    try
                    {
                        File.Move(path, archiveFilename);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }


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
            IAboutContent aboutWindowContent =
                new AboutContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\b.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _roundCorners)
                              };

            aboutWindow.ShowDialog();
        }

        private static DateTime FileDate(string path)
        {
            var dateOfRecording = GetExtendedProperty(path, 12);
            var mediumCreated = GetExtendedProperty(path, 208);

            var dateTime = File.GetCreationTime(path);

            var extendedProperty = string.Empty;

            if (!string.IsNullOrWhiteSpace(dateOfRecording))
            {
                extendedProperty = dateOfRecording;
            }

            if (!string.IsNullOrWhiteSpace(mediumCreated))
            {
                extendedProperty = mediumCreated;
            }

            if (string.IsNullOrWhiteSpace(extendedProperty))
            {
                return dateTime;
            }

            var cultureInfo = CultureInfo.CurrentCulture;
            var clean = new string(extendedProperty.Where(c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)).ToArray());
            return DateTime.Parse(clean.Trim(), cultureInfo);
        }

        private static string GetExtendedProperty(string filePath, int property)
        {
            var directory = Path.GetDirectoryName(filePath);
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            if (shellAppType is null)
            {
                return string.Empty;
            }

            dynamic shellApp = Activator.CreateInstance(shellAppType);
            if (shellApp == null)
            {
                return string.Empty;
            }

            var shellFolder = shellApp.NameSpace(directory);
            var fileName = Path.GetFileName(filePath);
            var folderItem = shellFolder.ParseName(fileName);

            var value = shellFolder.GetDetailsOf(folderItem, property);

            Marshal.ReleaseComObject(shellApp);
            Marshal.ReleaseComObject(shellFolder);
            return value;
        }
    }
}