﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using SkiaSharp;

namespace PlantAlarm.ViewModels
{
    public class PlantDetailsViewModel : INotifyPropertyChanged
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

        public PlantDetailsViewModel(Plant plant)
        {
            this.Plant = plant;

            var photoList = PlantService.GetPhotosOfPlant(plant);
            this.Photos = new ObservableCollection<PlantPhoto>(photoList);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
