using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.About.Core;
using EvilBaschdi.About.Core.Models;
using EvilBaschdi.About.Wpf;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.Core.Wpf;
using EvilBaschdi.Core.Wpf.Browsers;
using EvilBaschdi.Settings.ByMachineAndUser;
using FolderArchiver.Settings;
using MahApps.Metro.Controls;

namespace FolderArchiver;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : MetroWindow
{
    private readonly IAppSettings _appSettings;
    private string _initialDirectory;

    /// <inheritdoc />
    public MainWindow()
    {
        InitializeComponent();

        IApplicationStyle applicationStyle = new ApplicationStyle(true);
        applicationStyle.Run();

        IAppSettingsFromJsonFile appSettingsFromJsonFile = new AppSettingsFromJsonFile();
        IAppSettingsFromJsonFileByMachineAndUser appSettingsFromJsonFileByMachineAndUser = new AppSettingsFromJsonFileByMachineAndUser();
        IAppSettingByKey appSettingByKey = new AppSettingByKey(appSettingsFromJsonFile, appSettingsFromJsonFileByMachineAndUser);
        _appSettings = new AppSettings(appSettingByKey);

        Load();
    }

    private void Load()
    {
        ArchiveFolder.SetCurrentValue(IsEnabledProperty, !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) &&
                                                         Directory.Exists(_appSettings.InitialDirectory));

        _initialDirectory = _appSettings.InitialDirectory;
        InitialDirectory.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, _initialDirectory ?? string.Empty);
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
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Indeterminate);
        SetCurrentValue(CursorProperty, Cursors.Wait);

        var task = Task<string>.Factory.StartNew(ArchiveFolders);
        await task;

        ArchiveFolderContent.SetCurrentValue(System.Windows.Controls.TextBlock.TextProperty, task.Result);

        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Normal);
        SetCurrentValue(CursorProperty, Cursors.Arrow);
    }

    private string ArchiveFolders()
    {
        var filePath = new FileListFromPath();
        var files = filePath.ValueFor(_initialDirectory, new());

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
        ICurrentAssembly currentAssembly = new CurrentAssembly();
        IAboutContent aboutContent = new AboutContent(currentAssembly);
        IAboutViewModel aboutModel = new AboutViewModel(aboutContent);
        IApplyMicaBrush applyMicaBrush = new ApplyMicaBrush();
        var aboutWindow = new AboutWindow(aboutModel, applyMicaBrush);

        aboutWindow.ShowDialog();
    }

    private static DateTime FileDate(string path)
    {
        var dateOfRecording = GetExtendedProperty(path, 12);
        var mediumCreated = GetExtendedProperty(path, 208);

        var dateTime = File.GetLastWriteTime(path);

        var extendedProperty = string.Empty;

        if (!string.IsNullOrWhiteSpace(mediumCreated))
        {
            extendedProperty = mediumCreated;
        }

        if (!string.IsNullOrWhiteSpace(dateOfRecording))
        {
            extendedProperty = dateOfRecording;
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