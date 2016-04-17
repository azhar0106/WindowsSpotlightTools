using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotlightWallpaperFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            WallpaperFetcher wf = new WallpaperFetcher();
            wf.FetchWallpapers();
        }
    }
}
