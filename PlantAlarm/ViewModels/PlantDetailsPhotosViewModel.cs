using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using SkiaSharp;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class PlantDetailsPhotosViewModel : INotifyPropertyChanged
    {
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;
        private readonly Page View;

        private Plant plant { get; set; }
        public Plant Plant
        {
            get => plant;
            set
            {
                plant = value;
                OnPropertyChanged();
            }
        }

        private string title { get; set; }
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private PlantPhoto selectedPhoto { get; set; }
        public PlantPhoto SelectedPhoto
        {
            get => selectedPhoto;
            set
            {
                selectedPhoto = value;
                Title = $"Photo {Photos.IndexOf(Photos.First(p => p.Id == selectedPhoto.Id)) + 1} / {Photos.Count}";
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlantPhoto> photos { get; set; }
        public ObservableCollection<PlantPhoto> Photos
        {
            get => photos;
            set
            {
                photos = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeletePhotoCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public PlantDetailsPhotosViewModel(Plant plant, PlantPhoto _selectedPhoto, Page view)
        {
            this.View = view;
            this.Plant = plant;

            var photoList = PlantService.GetPhotosOfPlant(plant);
            this.Photos = new ObservableCollection<PlantPhoto>(photoList);

            this.SelectedPhoto = _selectedPhoto;

            BackCommand = new Command(async () =>
            {
                await this.NavigationStack.PopAsync();
            });
            DeletePhotoCommand = new Command(async () =>
            {
                switch(await this.View.DisplayActionSheet("Are you sure you want to delete this photo? This cannot be undone.", "Cancel", "Delete", new string[] { }))
                {
                    case "Delete": break;
                    default: return;
                }

                this.Photos.Remove(this.SelectedPhoto);
                OnPropertyChanged(nameof(Title));
                await PlantService.RemovePlantPhotoAsync(this.SelectedPhoto);
                MessagingCenter.Send(this as object, "PhotoRemoved");
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
