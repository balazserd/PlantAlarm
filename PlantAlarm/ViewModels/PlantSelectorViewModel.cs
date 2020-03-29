using System;
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

        private ObservableCollection<PlantSelectionItem> plants { get; set; }
        public ObservableCollection<PlantSelectionItem> Plants
        {
            get => plants;
            set
            {
                plants = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public PlantSelectorViewModel(List<Plant> alreadySelectedPlants)
        {
            var listOfPlants = PlantService.GetPlants();
            var listOfPhotos = PlantService.GetAllPhotos();

            var plantItems = listOfPlants.Select(plant =>
            {
                var photos = listOfPhotos.Where(photo => photo.PlantFk == plant.Id);

                return new PlantSelectionItem()
                {
                    Photos = photos.ToList(),
                    PrimaryPhoto = photos?.FirstOrDefault(photo => photo.IsPrimary),
                    Plant = plant
                };
            });

            Plants = new ObservableCollection<PlantSelectionItem>(plantItems);
            Device.StartTimer(TimeSpan.FromMilliseconds(600), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var plantItem in Plants)
                    {
                        plantItem.IsSelected = alreadySelectedPlants.Any(selPlant => selPlant.Id == plantItem.Plant.Id);
                    }
                });
                return false;
            });

            AddCommand = new Command(async () =>
            {
                var selectedPlants = Plants
                    .Cast<PlantSelectionItem>()
                    .Where(psi => psi.IsSelected)
                    .Select(psi => psi.Plant)
                    .ToList();

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

        public ICommand ItemTappedCommand { get; private set; }

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

        public PlantSelectionItem()
        {
            ItemTappedCommand = new Command(() =>
            {
                this.IsSelected = !this.IsSelected;
            });
        }
    }
}
