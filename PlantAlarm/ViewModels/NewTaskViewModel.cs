using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Views;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class NewTaskViewModel : INotifyPropertyChanged
    {
        private List<Plant> plantList { get; set; }
        public List<Plant> PlantList
        {
            get => plantList;
            set
            {
                plantList = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPlantsText));
            }
        }

        public string TaskName { get; set; }

        public string SelectedPlantsText
        {
            get
            {
                return PlantList == null || PlantList.Count == 0 ?
                    "Tap here to select plants" :
                    $"{PlantList.Count} plants selected";
            }
        }

        public ICommand AddTaskCommand { get; private set; }
        public ICommand AddPlantsCommand { get; private set; }

        public NewTaskViewModel()
        {
            PlantList = new List<Plant>();
            AddPlantsCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new PlantSelectorPage(PlantList));
            });

            MessagingCenter.Subscribe<object, List<Plant>>(this, "PlantsSelected", (viewModel, selectedPlants) =>
            {
                PlantList = selectedPlants;
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
