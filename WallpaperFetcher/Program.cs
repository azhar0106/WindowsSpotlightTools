using System;

namespace WallpaperFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            new Fetcher().FetchWallpapers();
            Console.ReadKey();
        }
    }
}
