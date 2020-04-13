using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace WallpaperFetcherGui
{
    public class SettingsModel
    {
        public int ResolutionHorizontal { get; set; }
        public int ResolutionVertical { get; set; }
        public string BaseDirLocation { get; set; }
        public string DesktopDirName { get; set; }
        public string MobileDirName { get; set; }

        private static readonly string ConfigDir = "WindowsSpotlightWallpaperFetcher";
        private static readonly string ConfigFile = "Settings.xml";

        private static string GetConfigFile()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configDirPath = Path.Combine(localAppData, ConfigDir);
            if (!Directory.Exists(configDirPath))
            {
                Directory.CreateDirectory(configDirPath);
            }
            var configFile = Path.Combine(configDirPath, ConfigFile);
            return configFile;
        }

        public static void Save(SettingsModel model)
        {
            var file = GetConfigFile();

            try
            {
                using (var fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
                using (var textWriter = new StreamWriter(fileStream, Encoding.Unicode))
                using (var xmlWriter = new XmlTextWriter(textWriter) { Formatting = Formatting.Indented })
                {
                    var xmlSer = new XmlSerializer(typeof(SettingsModel));
                    xmlSer.Serialize(xmlWriter, model);
                }
            } catch
            {
                return;
            }
        }

        public static SettingsModel Load()
        {
            var file = GetConfigFile();

            if (!File.Exists(file))
            {
                return GetDefaultSettings();
            }

            SettingsModel model = null;

            try
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var textReader = new StreamReader(fileStream, Encoding.Unicode))
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    var xmlSer = new XmlSerializer(typeof(SettingsModel));
                    model = (SettingsModel)xmlSer.Deserialize(xmlReader);
                }

                return model;
            }
            catch
            {
                return GetDefaultSettings();
            }
        }

        private static SettingsModel GetDefaultSettings()
        {
            var model = new SettingsModel
            {
                BaseDirLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                DesktopDirName = "Desktop",
                MobileDirName = "Mobile",
            };
            (model.ResolutionHorizontal, model.ResolutionVertical) = GetCurrentRes();
            
            return model;
        }

        public static (int, int) GetCurrentRes()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiX = (int)dpiXProperty.GetValue(null, null);
            var scale = dpiX / 96.0;

            var horRes = (int)(SystemParameters.PrimaryScreenWidth * scale);
            var verRes = (int)(SystemParameters.PrimaryScreenHeight * scale);

            return (horRes, verRes);
        }
    }
}
