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
        private readonly Page View;
        private readonly PlantTask PlantTaskToEdit;

        private bool isEditingMode { get; set; }
        public bool IsEditingMode
        {
            get => isEditingMode;
            set
            {
                isEditingMode = value;
                OnPropertyChanged();
            }
        }

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

        public string Title { get => IsEditingMode ? PlantTaskToEdit.Name : "New Plant"; }

        public string CommitButtonLabel => this.IsEditingMode ? "Save" : "Add";

        public ICommand CommitTaskCommand { get; private set; }
        public ICommand AddPlantsCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ToggleDayCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }

        public NewTaskViewModel(Page view, bool isEditingMode, PlantTask taskToEdit = null)
        {
            this.View = view;
            this.IsEditingMode = isEditingMode;
            this.PlantTaskToEdit = taskToEdit;
            if (taskToEdit != null)
            {
                this.FillFormWithExistingPlantTask();
            }
            else
            {
                PlantList = new List<Plant>();
                var defaultDate = DateTime.Now.AddHours(1);
                Time = new TimeSpan(defaultDate.Hour, defaultDate.Minute, 0);
                Date = defaultDate;
            }

            AddPlantsCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new PlantSelectorPage(PlantList));
            });
            CommitTaskCommand = new Command(async () =>
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

                if (this.IsEditingMode)
                {
                    plantTask.Id = this.PlantTaskToEdit.Id;
                    await PlantActivityService.ModifyPlantTaskAsync(plantTask, this.PlantList);
                }
                else
                {
                    await PlantActivityService.AddPlantTaskAsync(plantTask, this.PlantList);
                }

                MessagingCenter.Send(this as object, "TaskListChanged");
                MessagingCenter.Send(this as object, "SelectedDayActivitiesMightHaveChanged");
                await Application.Current.MainPage.Navigation.PopAsync();
            });
            BackCommand = new Command(async () => await NavigationStack.PopAsync());
            ToggleDayCommand = new Command((_dayToggle) =>
            {
                DayToggle dayToggle = (DayToggle)_dayToggle;
                dayToggle.IsOn = !dayToggle.IsOn;
            });
            DeleteTaskCommand = new Command(async () =>
            {
                string response = await View.DisplayActionSheet("Are you sure you want to delete this task? All future activities of this task will be deleted. This cannot be undone.", "Cancel", "Delete");
                if (response == "Delete")
                {
                    await PlantActivityService.RemoveTaskAsync(this.PlantTaskToEdit);
                    MessagingCenter.Send(this as object, "TaskListChanged");
                    await this.NavigationStack.PopAsync();
                }
            });

            MessagingCenter.Subscribe<object, List<Plant>>(this, "PlantsSelected", (viewModel, selectedPlants) =>
            {
                PlantList = selectedPlants;
            });
        }

        private void FillFormWithExistingPlantTask()
        {
            this.TaskName = this.PlantTaskToEdit.Name;
            this.DescriptionText = this.PlantTaskToEdit.Description;
            this.Monday.IsOn = this.PlantTaskToEdit.OnMonday;
            this.Tuesday.IsOn = this.PlantTaskToEdit.OnTuesday;
            this.Wednesday.IsOn = this.PlantTaskToEdit.OnWednesday;
            this.Thursday.IsOn = this.PlantTaskToEdit.OnThursday;
            this.Friday.IsOn = this.PlantTaskToEdit.OnFriday;
            this.Saturday.IsOn = this.PlantTaskToEdit.OnSaturday;
            this.Sunday.IsOn = this.PlantTaskToEdit.OnSunday;
            this.EveryXDays = this.PlantTaskToEdit.EveryXDays == 0 ? "" : this.PlantTaskToEdit.EveryXDays.ToString();
            this.EveryXMonths = this.PlantTaskToEdit.EveryXMonths == 0 ? "" : this.PlantTaskToEdit.EveryXMonths.ToString();
            this.Time = this.PlantTaskToEdit.FirstOccurrenceDate.TimeOfDay;
            this.Date = this.PlantTaskToEdit.FirstOccurrenceDate.Date;

            this.PlantList = PlantActivityService.GetPlantsOfTask(this.PlantTaskToEdit);
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
