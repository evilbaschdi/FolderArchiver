using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EvilBaschdi.About.Avalonia;
using EvilBaschdi.Core.Avalonia.DependencyInjection;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Primitives;
using FolderArchiver.Internal;

namespace FolderArchiver.ViewModels;

/// <summary>
///     The main window view model.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IArchiveFolders _archiveFolders;

    /// <summary>
    ///     Constructor
    /// </summary>
    public MainWindowViewModel([NotNull] IArchiveFolders archiveFolders)
    {
        _archiveFolders = archiveFolders ?? throw new ArgumentNullException(nameof(archiveFolders));
        AboutWindowCommand = ReactiveCommand.CreateFromTask(AboutWindowCommandAction);
        ArchiveFolderCommand = ReactiveCommand.CreateFromTask(ArchiveFolderCommandAction);
    }

    /// <summary>
    ///     Gets or Sets the about window command.
    /// </summary>
    public ReactiveCommand<RxVoid, RxVoid> AboutWindowCommand { get; set; }

    /// <summary>
    ///     Gets or Sets the archive command.
    /// </summary>
    public ReactiveCommand<RxVoid, RxVoid> ArchiveFolderCommand { get; set; }

    /// <summary>
    ///     Binding
    /// </summary>
    public string ArchiveFolderContentText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Click to archive folder";

    private async Task ArchiveFolderCommandAction()
    {
        ArchiveFolderContentText = await _archiveFolders.ValueAsync();
    }

    private static async Task AboutWindowCommandAction()
    {
        var aboutWindow = ApplicationServices.GetRequiredService<AboutWindow>();
        var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        if (mainWindow != null)
        {
            await aboutWindow.ShowDialog(mainWindow);
        }
    }
}
