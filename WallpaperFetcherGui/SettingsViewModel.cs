using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace WallpaperFetcherGui
{
    public class SettingsViewModel : ViewModelBase
    {
        private WallpaperFetcher _wallpaperFetcher;

        public SettingsModel Model { get; set; }

        public int ResolutionHorizontal
        {
            get => Model.ResolutionHorizontal;
            set => Model.ResolutionHorizontal = value;
        }
        public int ResolutionVertical
        {
            get => Model.ResolutionVertical;
            set => Model.ResolutionVertical = value;
        }

        public string Logs { get; set; }

        public ICommand CurrentResCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand FetchCommand { get; set; }

        public SettingsViewModel()
        {
            Model = SettingsModel.Load();
            CurrentResCommand = new DelegateCommand(SetCurrentRes);
            SaveCommand = new DelegateCommand(Save);
            FetchCommand = new DelegateCommand(Fetch);
        }

        private void SetCurrentRes()
        {
            (Model.ResolutionHorizontal, Model.ResolutionVertical) = SettingsModel.GetCurrentRes();

            NotifyPropertyChanged(nameof(ResolutionHorizontal));
            NotifyPropertyChanged(nameof(ResolutionVertical));
        }

        private void Fetch()
        {
            _wallpaperFetcher = new WallpaperFetcher(Model, Log);
            _wallpaperFetcher.Fetch();
        }

        private void Save()
        {
            SettingsModel.Save(Model);
        }

        private void Log(string message)
        {
            Logs += message + Environment.NewLine;
            NotifyPropertyChanged(nameof(Logs));
        }
    }
}
