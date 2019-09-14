using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using SkiaSharp;

namespace PlantAlarm.ViewModels
{
    public class PlantDetailsPhotosViewModel : INotifyPropertyChanged
    {
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
                Title = $"Photo {Photos.IndexOf(selectedPhoto)} / {Photos.Count}";
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

        public PlantDetailsPhotosViewModel(Plant plant, PlantPhoto _selectedPhoto)
        {
            this.Plant = plant;

            var photoList = PlantService.GetPhotosOfPlant(plant);
            this.Photos = new ObservableCollection<PlantPhoto>(photoList);

            this.SelectedPhoto = _selectedPhoto;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
