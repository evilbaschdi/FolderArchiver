using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EvilBaschdi.Core.Avalonia.DependencyInjection;
using FluentAvalonia.UI.Windowing;
using FolderArchiver.Settings;
using FolderArchiver.ViewModels;

namespace FolderArchiver;

/// <inheritdoc />
public partial class MainWindow : FAAppWindow
{
    private readonly IInitialDirectoryFromSettings _initialDirectoryFromSettings;

    /// <summary>
    ///     Constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        var topLevel = ApplicationServices.GetRequiredService<ITopLevel>();
        topLevel.Value = GetTopLevel(this);

        _initialDirectoryFromSettings = ApplicationServices.GetRequiredService<IInitialDirectoryFromSettings>();

        Load();
    }

    private void Load()
    {
        SetInitialDirectory(_initialDirectoryFromSettings.Value, false);
    }

    // ReSharper disable once UnusedMember.Local
    private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
    {
        var typedDirectory = NormalizeWindowsLocalPath(InitialDirectory.Text);
        if (!Directory.Exists(typedDirectory))
        {
            SetInitialDirectory(_initialDirectoryFromSettings.Value, false);
            return;
        }

        SetInitialDirectory(typedDirectory, true);
    }

    // ReSharper disable once UnusedMember.Local
    private async void BrowseClick(object sender, RoutedEventArgs e)
    {
        var storageProvider = GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var folderPickerOpenOptions = new FolderPickerOpenOptions
                                      {
                                          Title = "Choose folder to archive",
                                          AllowMultiple = false
                                      };
        var folderPicker = await storageProvider.OpenFolderPickerAsync(folderPickerOpenOptions);
        var storageFolder = folderPicker.FirstOrDefault();
        var fullPath = NormalizeWindowsLocalPath(FullPathOrName(storageFolder));

        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return;
        }

        SetInitialDirectory(fullPath, true);
    }

    private static string FullPathOrName(IStorageItem item) => item is null ? string.Empty : item.Path.LocalPath;

    private static string NormalizeWindowsLocalPath(string path)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(path))
        {
            return path ?? string.Empty;
        }

        return path.Length > 2 && path[0] == '/' && char.IsLetter(path[1]) && path[2] == ':'
            ? path[1..]
            : path;
    }

    private void SetInitialDirectory(string path, bool persist)
    {
        var normalizedPath = NormalizeWindowsLocalPath(path);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            InitialDirectory.Text = string.Empty;
            ArchiveFolderButton.IsEnabled = false;
            return;
        }

        InitialDirectory.Text = normalizedPath;
        if (!Directory.Exists(normalizedPath))
        {
            ArchiveFolderButton.IsEnabled = false;
            return;
        }

        ArchiveFolderButton.IsEnabled = true;
        if (persist)
        {
            _initialDirectoryFromSettings.Value = normalizedPath;
        }
    }
}