using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
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

        private void RefreshSource()
        {
            var plantList = PlantService.GetPlants();
            var photoList = PlantService.GetAllPhotos();

            var _plantItems = new ObservableCollection<PlantItem>();

            foreach (var plant in plantList)
            {
                var plantItem = new PlantItem();
                var plantMainPhoto = photoList.FirstOrDefault(photo => photo.PlantFk == plant.Id && photo.IsPrimary);

                plantItem.Plant = plant;
                plantItem.MainPhoto = plantMainPhoto;
                plantItem.ShowPlantDetailsPageCommand = new Command(() => this.ShowPlantDetailsPageCommandBase.Execute(plant));

                _plantItems.Add(plantItem);
            }

            PlantItems = _plantItems;
        }
    }

    public class PlantItem
    {
        public ICommand ShowPlantDetailsPageCommand { get; set; }
        public Plant Plant { get; set; }
        public PlantPhoto MainPhoto { get; set; }
    }
}
