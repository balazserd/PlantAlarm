using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using PlantAlarm.Views;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        private readonly Page View;
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;
        public List<CalendarDay> CalendarDays { get; private set; }

        private ObservableCollection<TodayPageActivityItem> activitiesForDay { get; set; }
        public ObservableCollection<TodayPageActivityItem> ActivitiesForDay
        {
            get => activitiesForDay;
            set
            {
                activitiesForDay = value;
                OnPropertyChanged();
            }
        }

        private CalendarDay selectedDay { get; set; }
        public CalendarDay SelectedDay
        {
            get => selectedDay;
            set {
                selectedDay = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectedDayChangedCommand { get; private set; }
        public ICommand ActivitySelectedCommand { get; private set; }
        public ICommand PlantImageTappedCommand { get; private set; }

        public TodayViewModel(Page view)
        {
            this.View = view;
            ActivitiesForDay = new ObservableCollection<TodayPageActivityItem>();

            var listOfDays = Enumerable.Range(-120, 241) //Will generate plus/minus 4 months. TODO: maybe a better calendar option.
                .Select(i => 
                    new CalendarDay
                    {
                        Date = DateTime.Today.AddDays(i)
                    });

            CalendarDays = listOfDays.ToList();
            SelectedDay = CalendarDays.Single(cd => cd.Date.Date == DateTime.Today.Date); //Default selection should be today.

            SelectedDayChangedCommand = new Command(async (dateTimeObject) =>
            {
                var date = (dateTimeObject as CalendarDay).Date;
                var activityList = await PlantActivityService.GetUpcomingActivitiesAsync(date);
                var actLi = new List<TodayPageActivityItem>(1);

                foreach (var activity in activityList)
                {
                    var todayPageActivity = new TodayPageActivityItem();
                    todayPageActivity.PlantActivityItem = activity;

                    todayPageActivity.ActivityTappedCommand = new Command(() =>
                    {
                        this.ActivitySelectedCommand.Execute(todayPageActivity);
                    });

                    todayPageActivity.ShowSkipDelayActionSheetCommand = new Command(async () =>
                    {
                        string chosenAction = await view.DisplayActionSheet("Choose an option", "Cancel", null, "Skip", "Delay");
                        switch (chosenAction)
                        {
                            case "Skip":
                                todayPageActivity.SkipCommand.Execute(null);
                                this.ActivitiesForDay.Remove(todayPageActivity);
                                break;
                            case "Delay":
                                todayPageActivity.DelayCommand.Execute(null);
                                this.ActivitiesForDay.Remove(todayPageActivity);
                                break;
                            default:
                                break;
                        }
                    });

                    var plantsOfActivity = await PlantActivityService.GetPlantsOfActivityAsync(activity);
                    todayPageActivity.Plants = (await Task.WhenAll(
                        plantsOfActivity
                        .Select(async (plant) =>
                        {
                            var plantItem = new TodayPagePlantItem();
                            plantItem.Plant = plant;

                            var photosOfPlants = await PlantService.GetPhotosOfPlantAsync(plant, true);
                            plantItem.Photo = photosOfPlants.FirstOrDefault();

                            plantItem.PlantImageTappedCommand = this.PlantImageTappedCommand;

                            return plantItem;
                        })
                    ))
                    .ToList();

                    todayPageActivity.Name = (await PlantActivityService.GetTaskOfActivityAsync(activity)).Name;
                    actLi.Add(todayPageActivity);
                }

                ActivitiesForDay = new ObservableCollection<TodayPageActivityItem>(actLi);
            });

            ActivitySelectedCommand = new Command(async(_todayPageActivityItem) =>
            {
                var plantActivityItem = (_todayPageActivityItem as TodayPageActivityItem).PlantActivityItem;
                var plantTask = await PlantActivityService.GetTaskOfActivityAsync(plantActivityItem);

                await NavigationStack.PushAsync(new TaskDetailsPage(plantTask));
            });

            PlantImageTappedCommand = new Command(async (_plant) =>
            {
                var plant = _plant as Plant;
                await NavigationStack.PushAsync(new PlantDetailsPage(plant));
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    #region mini-ViewModels
    //WARNING! Only for internal use. 
    public class TodayPageActivityItem : BindableObject
    {
        public PlantActivityItem PlantActivityItem { get; set; }
        public string Name { get; set; }
        public List<TodayPagePlantItem> Plants { get; set; }

        public Color ActivityStatusButtonBackgroundColor => this.PlantActivityItem.IsCompleted ? Color.FromHex("#7CC45D") : Color.FromHex("#FF7369");
        public Color ActivityStatusButtonBorderColor => this.PlantActivityItem.IsCompleted ? Color.FromHex("#5B8F44") : Color.FromHex("#D1463B");
        public string ActivityStatusText => this.PlantActivityItem.IsCompleted ? "Done" : "Due";

        public ICommand ActivityTappedCommand { get; set; }
        public ICommand IsCompletedChangedCommand { get; set; }
        public ICommand ShowSkipDelayActionSheetCommand { get; set; }
        public ICommand SkipCommand { get; set; }
        public ICommand DelayCommand { get; set; }

        public TodayPageActivityItem()
        {
            IsCompletedChangedCommand = new Command(() =>
            {
                this.PlantActivityItem.IsCompleted = !this.PlantActivityItem.IsCompleted;
                Task.Run(() => PlantActivityService.ModifyActivityAsync(this.PlantActivityItem));

                OnPropertyChanged(nameof(ActivityStatusButtonBackgroundColor));
                OnPropertyChanged(nameof(ActivityStatusButtonBorderColor));
                OnPropertyChanged(nameof(ActivityStatusText));
            });

            SkipCommand = new Command(() =>
            {
                Task.Run(() => PlantActivityService.RemoveActivityAsync(this.PlantActivityItem));
            });

            DelayCommand = new Command(() =>
            {
                this.PlantActivityItem.Time = this.PlantActivityItem.Time.AddDays(1);
                Task.Run(() => PlantActivityService.ModifyActivityAsync(this.PlantActivityItem));
            });
        }
    }

    public class TodayPagePlantItem : BindableObject
    {
        public Plant Plant { get; set; }
        public PlantPhoto Photo { get; set; }
        public ICommand PlantImageTappedCommand { get; set; }

        public void CallOnPropertyChangedForPhoto()
        {
            OnPropertyChanged(nameof(Photo));
        }
    }

    public class CalendarDay
    {
        private DateTime date { get; set; }
        public DateTime Date
        {
            get => date;
            set
            {
                date = value;
                Year = date.Year;
                Month = date.Month;
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Month);
                Day = date.Day;
                DayName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek).ToUpper();
            }
        }

        public int Year { get; private set; }
        public int Month { get; private set; }
        public string MonthName { get; private set; }
        public int Day { get; private set; }
        public string DayName { get; private set; }
    }
    #endregion
}
