using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
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
        public string DescriptionText { get; set; }

        public bool IsOnMonday { get; set; }
        public bool IsOnTuesday { get; set; }
        public bool IsOnWednesday { get; set; }
        public bool IsOnThursday { get; set; }
        public bool IsOnFriday { get; set; }
        public bool IsOnSaturday { get; set; }
        public bool IsOnSunday { get; set; }

        public string EveryXDays { get; set; }
        public string EveryXMonths { get; set; }

        public TimeSpan Time { get; set; }
        public DateTime Date { get; set; }

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
            var defaultDate = DateTime.Now.AddHours(1);
            Time = new TimeSpan(defaultDate.Hour, defaultDate.Minute, 0);
            Date = defaultDate;

            PlantList = new List<Plant>();
            AddPlantsCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new PlantSelectorPage(PlantList));
            });
            AddTaskCommand = new Command(async () =>
            {
                byte.TryParse(EveryXDays, out byte daysRecur);
                byte.TryParse(EveryXMonths, out byte monthsRecur);

                var plantTask = new PlantTask
                {
                    Description = DescriptionText,
                    EveryXDays = daysRecur,
                    EveryXMonths = monthsRecur,
                    FirstOccurrenceDate = new DateTime(Date.Year, Date.Month, Date.Day, Time.Hours, Time.Minutes, 0),
                    IsRepeating = IsOnMonday || IsOnTuesday || IsOnWednesday || IsOnThursday || IsOnFriday ||
                            IsOnSaturday || IsOnSunday || !string.IsNullOrEmpty(EveryXDays) || !string.IsNullOrEmpty(EveryXMonths),
                    OnMonday = IsOnMonday,
                    OnTuesday = IsOnTuesday,
                    OnWednesday = IsOnWednesday,
                    OnThursday = IsOnThursday,
                    OnFriday = IsOnFriday,
                    OnSaturday = IsOnSaturday,
                    OnSunday = IsOnSunday,
                };
                await PlantActivityService.AddPlantTaskAsync(plantTask);

                var connectionList = new List<PlantTaskPlantConnection>();
                foreach (var plant in PlantList)
                {
                    var plantTaskPlantConnection = new PlantTaskPlantConnection
                    {
                        PlantFk = plant.Id,
                        PlantTaskFk = plantTask.Id
                    };

                    connectionList.Add(plantTaskPlantConnection);
                }
                await PlantActivityService.AddPlantTaskPlantConnectionsAsync(connectionList);

                await Application.Current.MainPage.Navigation.PopAsync();
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
