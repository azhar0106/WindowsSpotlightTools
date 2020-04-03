using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WallpaperFetcher
{
    public class Fetcher
    {
        const string DESKTOP_WALLPAPER_FOLDER = "Desktop";
        const string MOBILE_WALLPAPER_FOLDER = "Mobile";
        const string SPOTLIGHT_REGISTRY_KEY__LOCATION = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative";
        const string SPOTLIGHT_REGISTRY_NAME__WALLPAPER_LOCATION = "HotspotImageFolderPath";


        private string m_spotlightWallpaperLocation;
        public string SpotlightWallpaperLocation
        {
            get { return m_spotlightWallpaperLocation; }
            set { m_spotlightWallpaperLocation = value; }
        }

        private List<string> m_desktopWallpaperList;
        public List<string> DesktopWallpaperList
        {
            get { return m_desktopWallpaperList; }
            set { m_desktopWallpaperList = value; }
        }

        private List<string> m_mobileWallpaperList;
        public List<string> MobileWallpaperList
        {
            get { return m_mobileWallpaperList; }
            set { m_mobileWallpaperList = value; }
        }


        public Fetcher()
        {
            m_desktopWallpaperList = new List<string>();
            m_mobileWallpaperList = new List<string>();
            m_spotlightWallpaperLocation = null;

            GetWallpaperLocation();
            CreateWallpaperList();
        }
        

        private void GetWallpaperLocation()
        {
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(SPOTLIGHT_REGISTRY_KEY__LOCATION, false);
            string loc = null;
            if (reg == null)
            {
                Console.WriteLine("Registry data not found.");
                return;
            }
            loc = (string)reg.GetValue(SPOTLIGHT_REGISTRY_NAME__WALLPAPER_LOCATION);

            SpotlightWallpaperLocation = loc;

            if (loc == null)
            {
                Console.WriteLine("ERROR: Could not locate Spotlight Wallpapers.");
            }
        }

        private void CreateWallpaperList()
        {
            if (string.IsNullOrWhiteSpace(SpotlightWallpaperLocation))
            {
                Console.WriteLine("Invalid Spotlight wallpaper location.");
                return;
            }

            string[] files = Directory.GetFiles(SpotlightWallpaperLocation);
            foreach (var file in files)
            {
                Image img = null;
                try
                {
                    img = Image.FromFile(file);
                }
                catch
                {
                    continue;
                }

                if (img.Height == 1080)
                {
                    m_desktopWallpaperList.Add(file);
                }
                else if (img.Width == 1080)
                {
                    m_mobileWallpaperList.Add(file);
                }
            }
        }

        private void CopyWallpapers()
        {
            string exeFolder = null;
            exeFolder = Assembly.GetEntryAssembly().Location;
            exeFolder = Path.GetDirectoryName(exeFolder);

            string desktopFolder = Path.Combine(exeFolder, DESKTOP_WALLPAPER_FOLDER);
            string mobileFolder = Path.Combine(exeFolder, MOBILE_WALLPAPER_FOLDER);

            CopyWallpapers(m_desktopWallpaperList, desktopFolder);
            CopyWallpapers(m_mobileWallpaperList, mobileFolder);
        }

        private void CopyWallpapers(List<string> source, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                try
                {
                    Directory.CreateDirectory(destinationFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not create directory to copy wallpapers: " + ex.Message);
                }
            }

            foreach (var file in source)
            {
                string dest = Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(file));
                dest += ".jpg";

                try
                {
                    File.Copy(file, dest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Could not copy '{0}' to '{1}'", file, destinationFolder) + ex.Message);
                }
            }
        }

        public void FetchWallpapers()
        {
            CopyWallpapers();
        }
    }
}
