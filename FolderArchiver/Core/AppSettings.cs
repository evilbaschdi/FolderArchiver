using System;
using EvilBaschdi.CoreExtended.AppHelpers;

namespace FolderArchiver.Core
{
    public class AppSettings : IAppSettings
    {
        private readonly IAppSettingsBase _appSettingsBase;

        public AppSettings(IAppSettingsBase appSettingsBase)
        {
            _appSettingsBase = appSettingsBase ?? throw new ArgumentNullException(nameof(appSettingsBase));
        }

        public string InitialDirectory
        {
            get => _appSettingsBase.Get<string>("InitialDirectory");
            set => _appSettingsBase.Set("InitialDirectory", value);
        }
    }
}