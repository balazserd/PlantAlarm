using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
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
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;

        private List<Plant> plantList { get; set; }
        public List<Plant> PlantList
        {
            get => plantList;
            set
            {
                plantList = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPlantsText));
                OnPropertyChanged(nameof(SelectedPlantsTextOpacity));
            }
        }

        public string TaskName { get; set; }
        public string DescriptionText { get; set; }

        public DayToggle Monday { get; set; } = new DayToggle();
        public DayToggle Tuesday { get; set; } = new DayToggle();
        public DayToggle Wednesday { get; set; } = new DayToggle();
        public DayToggle Thursday { get; set; } = new DayToggle();
        public DayToggle Friday { get; set; } = new DayToggle();
        public DayToggle Saturday { get; set; } = new DayToggle();
        public DayToggle Sunday { get; set; } = new DayToggle();

        public string EveryXDays { get; set; }
        public string EveryXMonths { get; set; }

        public TimeSpan Time { get; set; }
        public DateTime Date { get; set; }

        public string SelectedPlantsText
        {
            get
            {
                return PlantList == null || PlantList.Count == 0
                    ? "Tap here to select plants"
                    : $"{PlantList.Count} plant{(PlantList.Count > 1 ? "s" : "")} selected";
            }
        }

        public double SelectedPlantsTextOpacity
        {
            get => PlantList == null || PlantList.Count == 0
                ? 0.4
                : 1.0;
        }

        public ICommand AddTaskCommand { get; private set; }
        public ICommand AddPlantsCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ToggleDayCommand { get; private set; }

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
                    Name = TaskName,
                    Description = DescriptionText,
                    EveryXDays = daysRecur,
                    EveryXMonths = monthsRecur,
                    FirstOccurrenceDate = new DateTime(Date.Year, Date.Month, Date.Day, Time.Hours, Time.Minutes, 0),
                    IsRepeating = Monday.IsOn || Tuesday.IsOn || Wednesday.IsOn || Thursday.IsOn || Friday.IsOn ||
                            Saturday.IsOn || Sunday.IsOn || !string.IsNullOrEmpty(EveryXDays) || !string.IsNullOrEmpty(EveryXMonths),
                    OnMonday = Monday.IsOn,
                    OnTuesday = Tuesday.IsOn,
                    OnWednesday = Wednesday.IsOn,
                    OnThursday = Thursday.IsOn,
                    OnFriday = Friday.IsOn,
                    OnSaturday = Saturday.IsOn,
                    OnSunday = Sunday.IsOn,
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
            BackCommand = new Command(async () => await NavigationStack.PopAsync());
            ToggleDayCommand = new Command((_dayToggle) =>
            {
                DayToggle dayToggle = (DayToggle)_dayToggle;
                dayToggle.IsOn = !dayToggle.IsOn;
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

        public class DayToggle : BindableObject
        {
            private bool isOn { get; set; }
            public bool IsOn
            {
                get => isOn;
                set
                {
                    isOn = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }

            public Color BackgroundColor
            {
                get => IsOn
                    ? Color.FromHex("#947900")
                    : Color.FromHex("#FAF3D0");
            }

            public Color TextColor
            {
                get => IsOn
                    ? Color.FromHex("#FAF3D0")
                    : Color.FromHex("#947900");
            }
        }
    }
}
