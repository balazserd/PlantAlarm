﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Helpers;
using PlantAlarm.Services;
using PlantAlarm.Views;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class PlantsViewModel : INotifyPropertyChanged
    {
        private INavigation Navigation = Application.Current.MainPage.Navigation;

        private ObservableCollection<PlantItem> plantItems { get; set; }
        public ObservableCollection<PlantItem> PlantItems
        {
            get => plantItems;
            set
            {
                plantItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowNewPlantPageCommand { get; private set; }
        private ICommand ShowPlantDetailsPageCommandBase { get; set; }

        public PlantsViewModel()
        {
            this.RefreshSource();

            ShowNewPlantPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(new NewPlantPage());
            });
            ShowPlantDetailsPageCommandBase = new Command(async (_plant) =>
            {
                var plant = _plant as Plant;
                await Navigation.PushAsync(new PlantDetailsPage(plant));
            });

            MessagingCenter.Subscribe<object>(this, "PlantAdded", (vm) =>
            {
                this.RefreshSource();
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RefreshSource()
        {
            var plantList = PlantService.GetPlants();
            var photoList = PlantService.GetAllPhotos();
            var lockObject = new object();

            var _plantItems = new List<PlantItem>();

            foreach (var plant in plantList)
            {
                var plantItem = new PlantItem(plant);
                var plantMainPhoto = photoList.FirstOrDefault(photo => photo.PlantFk == plant.Id && photo.IsPrimary);

                plantItem.MainPhoto = plantMainPhoto;
                plantItem.ShowPlantDetailsPageCommand = new Command(() => this.ShowPlantDetailsPageCommandBase.Execute(plant));

                lock (lockObject)
                {
                    _plantItems.Add(plantItem);
                }
            }

            PlantItems = new ObservableCollection<PlantItem>(_plantItems
                .OrderByDescending(pi => pi.HasMissedTasks)
                .ThenByDescending(pi => pi.HasTasksDueToday)
                .ThenByDescending(pi => pi.StreakDays > 0)
                .ThenBy(pi => pi.StreakDays));
        }
    }

    public class PlantItem : BindableObject
    {
        public ICommand ShowPlantDetailsPageCommand { get; set; }
        public Plant Plant { get; set; }

        private PlantPhoto mainPhoto { get; set; }
        public PlantPhoto MainPhoto
        {
            get => mainPhoto;
            set
            {
                mainPhoto = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPhoto));
                OnPropertyChanged(nameof(HasNoPhoto));
            }
        }

        public string Monogram
        {
            get => Plant.GetMonogram();
        }

        public bool HasPhoto
        {
            get => MainPhoto != null;
        }

        public bool HasNoPhoto
        {
            get => !HasPhoto;
        }

        public DateTime? LastMissedActivityTime { get; set; }
        public DateTime? LastCompletedActivityTime { get; set; }
        public DateTime? FirstCompletedActivityTimeAfterLastMissedActivity { get; set; }

        public int StreakDays =>
            (LastCompletedActivityTime?.Date - LastMissedActivityTime?.Date)?.Days > 0 ?
            ((DateTime.Today.Date - FirstCompletedActivityTimeAfterLastMissedActivity?.Date)?.Days ?? 0) :
            0;

        public bool HasMissedTasks => (LastMissedActivityTime ?? DateTime.MinValue) > (LastCompletedActivityTime ?? DateTime.MinValue);
        public bool HasTasksDueToday { get; set; }
        public bool IsOnStreak => StreakDays > 0;

        public PlantItem(Plant plant)
        {
            this.Plant = plant;

            var nextActivities = PlantActivityService.GetUpcomingActivitiesOfPlant(plant);
            DateTime? earliestNextActivityDate = nextActivities.Count == 0 ?
                (DateTime?)null :
                nextActivities.Select(extPai => extPai.PlantActivityItem.Time).Min();

            this.HasTasksDueToday = earliestNextActivityDate?.Date == DateTime.Today;

            this.LastMissedActivityTime = PlantActivityService.GetLatestMissedActivityOfPlant(plant)?.Time;
            this.LastCompletedActivityTime = PlantActivityService.GetLatestCompletedActivityOfPlant(plant)?.Time;
            this.FirstCompletedActivityTimeAfterLastMissedActivity = PlantActivityService.GetFirstCompletedActivityAfterLastMissedActivity(plant)?.Time;
        }
    }
}
