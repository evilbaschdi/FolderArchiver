using System.Globalization;
using System.Runtime.InteropServices;
using EvilBaschdi.Core.Internal;
using FolderArchiver.Settings;

namespace FolderArchiver.Internal;

/// <inheritdoc />
public class ArchiveFolders : IArchiveFolders
{
    private readonly IInitialDirectoryFromSettings _initialDirectoryFromSettings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="initialDirectoryFromSettings"></param>
    public ArchiveFolders(IInitialDirectoryFromSettings initialDirectoryFromSettings)
    {
        _initialDirectoryFromSettings = initialDirectoryFromSettings ?? throw new ArgumentNullException(nameof(initialDirectoryFromSettings));
    }

    /// <inheritdoc />
    public Task<string> ValueAsync(CancellationToken cancellationToken) => Task.Run(Archive, cancellationToken);

    private string Archive()
    {
        var initialDirectory = _initialDirectoryFromSettings.Value;
        if (string.IsNullOrWhiteSpace(initialDirectory) || !Directory.Exists(initialDirectory))
        {
            return "Nothing has changed.";
        }

        var filePath = new FileListFromPath();
        var files = filePath.ValueFor(initialDirectory, new());

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
            var archiveDirectory = $@"{initialDirectory}\{archiveTime}";
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
                    Console.WriteLine(e.Message);
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

    private static DateTime FileDate(string path)
    {
        var dateOfRecording = GetExtendedProperty(path, 12);
        var mediumCreated = GetExtendedProperty(path, 208);

        var dateTime = File.GetLastWriteTime(path);

        var extendedProperty = string.Empty;

        if (!string.IsNullOrWhiteSpace(mediumCreated) && DateTime.TryParse(mediumCreated, out _))
        {
            extendedProperty = mediumCreated;
        }

        if (!string.IsNullOrWhiteSpace(dateOfRecording) && DateTime.TryParse(dateOfRecording, out _))
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

#pragma warning disable CA1416
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
#pragma warning restore CA1416
}