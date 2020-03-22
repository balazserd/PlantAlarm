﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class PlantSelectorViewModel : INotifyPropertyChanged
    {
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;

        public ObservableCollection<PlantSelectionItem> Plants { get; private set; }

        private ObservableCollection<object> selectedPlantItems { get; set; }
        public ObservableCollection<object> SelectedPlantItems
        {
            get => selectedPlantItems;
            set
            {
                selectedPlantItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public PlantSelectorViewModel(List<Plant> alreadySelectedPlants)
        {
            SelectedPlantItems = new ObservableCollection<object>();

            var listOfPlants = PlantService.GetPlants();
            var listOfPhotos = PlantService.GetAllPhotos();

            var plantItems = listOfPlants.Select(plant =>
            {
                var photos = listOfPhotos.Where(photo => photo.PlantFk == plant.Id);

                return new PlantSelectionItem
                {
                    Photos = photos.ToList(),
                    PrimaryPhoto = photos?.FirstOrDefault(photo => photo.IsPrimary),
                    Plant = plant,
                    IsSelected = alreadySelectedPlants.Any(selPlant => selPlant.Id == plant.Id)
                };
            });

            Plants = new ObservableCollection<PlantSelectionItem>(plantItems);

            var preSelection = Plants.Where(
                plantItem => alreadySelectedPlants.Any(
                    selPlant => selPlant.Id == plantItem.Plant.Id))
                .Cast<object>()
                .ToList();

            SelectedPlantItems = new ObservableCollection<object>(preSelection);

            SelectionChangedCommand = new Command((plantsSelected) =>
            {
                var selectedPlants = (plantsSelected as IList<object>).ToList().Cast<PlantSelectionItem>();
                var selectedIdsList = selectedPlants.Select(pi => pi.Plant.Id).ToList();

                foreach (var plantItem in Plants)
                {
                    plantItem.IsSelected =
                        selectedIdsList.FirstOrDefault(selId => selId == plantItem.Plant.Id) != default(int);
                }
            });

            AddCommand = new Command(async () =>
            {
                var selectedPlants = SelectedPlantItems.Select(plantItem => (plantItem as PlantSelectionItem).Plant).ToList();
                MessagingCenter.Send(this as object, "PlantsSelected", selectedPlants);

                await Application.Current.MainPage.Navigation.PopAsync();
            });

            BackCommand = new Command(async () => await NavigationStack.PopAsync());
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PlantSelectionItem : BindableObject
    {
        public Plant Plant { get; set; }
        public List<PlantPhoto> Photos { get; set; }
        public PlantPhoto PrimaryPhoto { get; set; }

        private bool isSelected { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
