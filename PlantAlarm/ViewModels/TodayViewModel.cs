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

        public TodayViewModel()
        {
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

                    var plantsOfActivity = await PlantActivityService.GetPlantsOfActivity(activity);
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

                    todayPageActivity.Name = (await PlantActivityService.GetTaskOfActivity(activity)).Name;
                    actLi.Add(todayPageActivity);
                }

                ActivitiesForDay = new ObservableCollection<TodayPageActivityItem>(actLi);
            });

            ActivitySelectedCommand = new Command(async(_plantActivityItem) =>
            {
                var plantActivityItem = _plantActivityItem as PlantActivityItem;
            });

            PlantImageTappedCommand = new Command(async (_plant) =>
            {
                var plant = _plant as Plant;
                await Application.Current.MainPage.Navigation.PushAsync(new PlantDetailsPage(plant));
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    //WARNING! Only for internal use.
    public class TodayPageActivityItem : BindableObject
    {
        public PlantActivityItem PlantActivityItem { get; set; }
        public string Name { get; set; }
        public List<TodayPagePlantItem> Plants { get; set; }
    }

    public class TodayPagePlantItem : BindableObject
    {
        public Plant Plant { get; set; }
        public PlantPhoto Photo { get; set; }
        public ICommand PlantImageTappedCommand { get; set; }

        public void CallOnPropertChangedForPhoto()
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
}
