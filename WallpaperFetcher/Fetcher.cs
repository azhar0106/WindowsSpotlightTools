using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WallpaperFetcher
{
    public class Fetcher
    {
        private const string DESKTOP_WALLPAPER_DIR = "Desktop";
        private const string MOBILE_WALLPAPER_DIR = "Mobile";
        private const string REGKEY_SPOTLIGHT = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative";
        private const string REGNAME_ASSET_DIR = "HotspotImageFolderPath";
        private const string CDM_DIR = "Microsoft.Windows.ContentDeliveryManager_";
        private const string ASSETS_DIR = @"LocalState\Assets";

        public string SpotlightWallpaperLocation { get; set; }
        public List<string> DesktopWallpaperList { get; set; }
        public List<string> MobileWallpaperList { get; set; }

        public Fetcher()
        {
            DesktopWallpaperList = new List<string>();
            MobileWallpaperList = new List<string>();
            SpotlightWallpaperLocation = null;
        }

        public void FetchWallpapers()
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
            Console.WriteLine("Trying to find Asset directory from registry...");

            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(REGKEY_SPOTLIGHT, false);
            if (reg == null)
            {
                Console.WriteLine($"Reg key: '{REGKEY_SPOTLIGHT}' not found.");
                return;
            }

            var assetsDir = (string)reg.GetValue(REGNAME_ASSET_DIR);
            if (assetsDir == null)
            {
                Console.WriteLine($"Reg name: '{REGNAME_ASSET_DIR}' not found.");
                return;
            }

            Console.WriteLine($"Directory location found: {assetsDir}");
            if (!Directory.Exists(assetsDir))
            {
                Console.WriteLine("Asset directory does not exist.");
            }

            Console.WriteLine($"Asset directory found: {assetsDir}");

            SpotlightWallpaperLocation = assetsDir;
        }

        private void FindAssetDirLocally()
        {
            Console.WriteLine("Attempting to find Asset directory locally...");
            var localAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var packagesDir = Path.Combine(localAppDataDir, "Packages");
            if (!Directory.Exists(packagesDir))
            {
                Console.WriteLine($"Directory '{packagesDir}' does not exist.");
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
                Console.WriteLine($"Directory: '{packagesDir}\\{CDM_DIR}*' does not exist.");
            }

            if (!Directory.Exists(cdmDir))
            {
                Console.WriteLine($"Directory '{cdmDir}' does not exist.");
                return;
            }

            var assetsDir = Path.Combine(cdmDir, ASSETS_DIR);
            if (!Directory.Exists(assetsDir))
            {
                Console.WriteLine($"Directory '{assetsDir}' does not exist.");
                return;
            }

            Console.WriteLine($"Asset directory found: {assetsDir}");

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

                if (img.Height == 1080) //TODO: Find a way to get screen res in a console app.
                {
                    DesktopWallpaperList.Add(file);
                }
                else if(img.Width == 1080)
                {
                    MobileWallpaperList.Add(file);
                }
            }
        }

        private void CopyWallpapers()
        {
            //TODO: Find a way to identify if self-contained or not.
            // var assemblyFile = Assembly.GetEntryAssembly().Location; // Doesn't work for self-contained exe.
            var assemblyFile = Process.GetCurrentProcess().MainModule.FileName;
            var exeDir = Path.GetDirectoryName(assemblyFile);

            string desktopDir = Path.Combine(exeDir, DESKTOP_WALLPAPER_DIR);
            string mobileDir = Path.Combine(exeDir, MOBILE_WALLPAPER_DIR);

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
                    Console.WriteLine("Could not create directory to copy wallpapers: " + ex.Message);
                }
            }

            foreach (var file in source)
            {
                string dest = Path.Combine(destinationDir, Path.GetFileNameWithoutExtension(file));
                dest += ".jpg";

                try
                {
                    Console.WriteLine($"Copying file '{file}' to '{dest}'");
                    File.Copy(file, dest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Could not copy '{0}' to '{1}'", file, destinationDir) + ex.Message);
                }
            }
        }
    }
}
