using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;

namespace PlantAlarm.ViewModels
{
    public class PlantSelectorViewModel : INotifyPropertyChanged 
    {
        public ObservableCollection<PlantSelectionItem> PlantList { get; private set; }

        public PlantSelectorViewModel()
        {
            var listOfPlantSelectionItems = PlantService.GetPlantsAsync()
                .Result
                .Select(p => new PlantSelectionItem
                {
                    Plant = p,
                    PhotosOfPlant = PlantService.GetPhotosOfPlantAsync(p).Result
                });

            PlantList = new ObservableCollection<PlantSelectionItem>(listOfPlantSelectionItems);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PlantSelectionItem
    {
        public Plant Plant { get; set; }
        public List<PlantPhoto> PhotosOfPlant { get; set; }
        public PlantPhoto PrimaryPhotoOfPlant => PhotosOfPlant.FirstOrDefault(photo => photo.IsPrimary);
    }
}
