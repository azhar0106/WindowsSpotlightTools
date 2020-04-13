using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace WallpaperFetcherGui
{
    public class WallpaperFetcher
    {
        private const string REGKEY_SPOTLIGHT = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative";
        private const string REGNAME_ASSET_DIR = "HotspotImageFolderPath";
        private const string CDM_DIR = "Microsoft.Windows.ContentDeliveryManager_";
        private const string ASSETS_DIR = @"LocalState\Assets";

        private SettingsModel Settings { get; set; }
        private Action<string> Log { get; set; }
        private string SpotlightWallpaperLocation { get; set; }
        private List<string> DesktopWallpaperList { get; set; } = new List<string>();
        private List<string> MobileWallpaperList { get; set; } = new List<string>();

        public WallpaperFetcher(SettingsModel settings, Action<string> logger)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Log = logger ?? ((s) => { });
        }

        public void Fetch()
        {
            FindWallpaperLocation();
            CreateWallpaperList();
            CopyWallpapers();
        }

        private void FindWallpaperLocation()
        {
            FindAssetDirFromRegistry();
            if (SpotlightWallpaperLocation != null)
            {
                return;
            }

            FindAssetDirLocally();
        }

        private void FindAssetDirFromRegistry()
        {
            Log("Trying to find Asset directory from registry...");

            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(REGKEY_SPOTLIGHT, false);
            if (reg == null)
            {
                Log($"Reg key: '{REGKEY_SPOTLIGHT}' not found.");
                return;
            }

            var assetsDir = (string)reg.GetValue(REGNAME_ASSET_DIR);
            if (assetsDir == null)
            {
                Log($"Reg name: '{REGNAME_ASSET_DIR}' not found.");
                return;
            }

            Log($"Directory location found: {assetsDir}");
            if (!Directory.Exists(assetsDir))
            {
                Log("Asset directory does not exist.");
            }

            Log($"Asset directory found: {assetsDir}");

            SpotlightWallpaperLocation = assetsDir;
        }

        private void FindAssetDirLocally()
        {
            Log("Attempting to find Asset directory locally...");
            var localAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var packagesDir = Path.Combine(localAppDataDir, "Packages");
            if (!Directory.Exists(packagesDir))
            {
                Log($"Directory '{packagesDir}' does not exist.");
                return;
            }
            var cdmDir = Directory.EnumerateDirectories(packagesDir)
                                  .FirstOrDefault(d =>
                                  {
                                      var dn = Path.GetFileName(d);
                                      return dn.StartsWith(CDM_DIR);
                                  });
            if (cdmDir == null)
            {
                Log($"Directory: '{packagesDir}\\{CDM_DIR}*' does not exist.");
            }

            if (!Directory.Exists(cdmDir))
            {
                Log($"Directory '{cdmDir}' does not exist.");
                return;
            }

            var assetsDir = Path.Combine(cdmDir, ASSETS_DIR);
            if (!Directory.Exists(assetsDir))
            {
                Log($"Directory '{assetsDir}' does not exist.");
                return;
            }

            Log($"Asset directory found: {assetsDir}");

            SpotlightWallpaperLocation = assetsDir;
            return;
        }

        private void CreateWallpaperList()
        {
            if (SpotlightWallpaperLocation == null)
            {
                return;
            }

            string[] files = Directory.GetFiles(SpotlightWallpaperLocation);
            foreach (var file in files)
            {
                Image img;
                try
                {
                    img = Image.FromFile(file);
                }
                catch
                {
                    continue;
                }

                if (img.Height == Settings.ResolutionVertical) //TODO: Find a way to get screen res in a console app.
                {
                    DesktopWallpaperList.Add(file);
                }
                else if (img.Width == Settings.ResolutionVertical)
                {
                    MobileWallpaperList.Add(file);
                }
            }
        }

        private void CopyWallpapers()
        {
            //TODO: Find a way to identify if self-contained or not.
            //var assemblyFile = Assembly.GetEntryAssembly().Location; // Doesn't work for self-contained exe.
            //var assemblyFile = Process.GetCurrentProcess().MainModule.FileName;
            //var exeDir = Path.GetDirectoryName(assemblyFile);

            string desktopDir = Path.Combine(Settings.BaseDirLocation, Settings.DesktopDirName);
            string mobileDir = Path.Combine(Settings.BaseDirLocation, Settings.MobileDirName);

            CopyWallpapers(DesktopWallpaperList, desktopDir);
            CopyWallpapers(MobileWallpaperList, mobileDir);
        }

        private void CopyWallpapers(List<string> source, string destinationDir)
        {
            if (!Directory.Exists(destinationDir))
            {
                try
                {
                    Directory.CreateDirectory(destinationDir);
                }
                catch (Exception ex)
                {
                    Log("Could not create directory to copy wallpapers: " + ex.Message);
                }
            }

            foreach (var file in source)
            {
                string dest = Path.Combine(destinationDir, Path.GetFileNameWithoutExtension(file));
                dest += ".jpg";

                try
                {
                    Log($"Copying file '{file}' to '{dest}'");
                    File.Copy(file, dest);
                }
                catch (Exception ex)
                {
                    Log(string.Format("Could not copy '{0}' to '{1}'", file, destinationDir) + ex.Message);
                }
            }
        }
    }
}
