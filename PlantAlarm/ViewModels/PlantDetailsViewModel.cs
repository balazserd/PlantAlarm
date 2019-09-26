﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using PlantAlarm.Views;
using SkiaSharp;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class PlantDetailsViewModel : INotifyPropertyChanged
    {
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;

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

        private ObservableCollection<ProgressPhotoViewModel> photoViewModels { get; set; }
        public ObservableCollection<ProgressPhotoViewModel> PhotoViewModels
        {
            get => photoViewModels;
            set
            {
                photoViewModels = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ExtendedPlantActivityViewModel> upcomingActivities { get; set; }
        public ObservableCollection<ExtendedPlantActivityViewModel> UpcomingActivities
        {
            get => upcomingActivities;
            set
            {
                upcomingActivities = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenPhotoCarouselCommand { get; private set; }

        public PlantDetailsViewModel(Plant plant)
        {
            OpenPhotoCarouselCommand = new Command(async (_tappedPhoto) =>
            {
                var tappedPhoto = _tappedPhoto as PlantPhoto;
                await NavigationStack.PushAsync(new PlantDetailsPhotosPage(this.plant, tappedPhoto));
            });

            this.Plant = plant;

            var photoVmList = PlantService.GetPhotosOfPlant(plant)
                .Select(ph => new ProgressPhotoViewModel(ph, OpenPhotoCarouselCommand));

            this.PhotoViewModels = new ObservableCollection<ProgressPhotoViewModel>(photoVmList);

            var activities = PlantActivityService.GetUpcomingActivitiesOfPlant(plant);
            var extendedActivities = activities
                .Select(act =>
                {
                    var plantsOfActivity = PlantActivityService.GetPlantsOfActivity(act.PlantActivityItem);
                    var primaryPhotosOfPlants = PlantService.GetPrimaryPhotosOfPlants(plantsOfActivity);

                    act.PrimaryPhotoOfPlantsInTask = primaryPhotosOfPlants;
                    return act;
                })
                .ToList();

            this.UpcomingActivities = new ObservableCollection<ExtendedPlantActivityViewModel>(extendedActivities);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public class ProgressPhotoViewModel
        {
            public PlantPhoto Photo { get; set; }
            public ICommand TappedCommand { get; set; }

            public ProgressPhotoViewModel(PlantPhoto photo, ICommand tappedCommand)
            {
                this.Photo = photo;
                this.TappedCommand = new Command(() => tappedCommand.Execute(photo));
            }
        }
    }

    public class ExtendedPlantActivityViewModel : BindableObject
    {
        public PlantActivityItem PlantActivityItem { get; set; }
        public PlantTask PlantTask { get; set; }
        public List<PlantPhoto> PrimaryPhotoOfPlantsInTask { get; set; }
    }
}