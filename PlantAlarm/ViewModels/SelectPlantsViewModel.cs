using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;

namespace PlantAlarm.ViewModels
{
    public class SelectPlantsViewModel : INotifyPropertyChanged 
    {
        public ObservableCollection<Plant> PlantList { get; private set; }

        public SelectPlantsViewModel()
        {
            PlantList = new ObservableCollection<Plant>(PlantService.GetPlantsAsync().Result);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
