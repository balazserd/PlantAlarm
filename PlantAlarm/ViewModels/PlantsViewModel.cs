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
        private INavigation Navigation { get; set; }

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

        public PlantsViewModel(INavigation navigation)
        {
            Navigation = navigation;

            ShowNewPlantPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(new NewPlantPage());
            });

            var plantList = PlantService.GetPlants();
            var photoList = PlantService.GetAllPhotos();

            PlantItems = new ObservableCollection<PlantItem>();

            foreach (var plant in plantList)
            {
                var plantItem = new PlantItem();
                var plantMainPhoto = photoList.FirstOrDefault(photo => photo.PlantFk == plant.Id && photo.IsPrimary);

                plantItem.Plant = plant;
                plantItem.MainPhoto = plantMainPhoto;

                PlantItems.Add(plantItem);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PlantItem
    {
        public Plant Plant { get; set; }
        public PlantPhoto MainPhoto { get; set; }
    }
}
