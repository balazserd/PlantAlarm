using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PlantAlarm.Models;

namespace PlantAlarm.ViewModels
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CalendarDay> CalendarDays { get; private set; }

        private CalendarDay selectedDay { get; set; }
        public CalendarDay SelectedDay
        {
            get => selectedDay;
            set { OnPropertyChanged(); selectedDay = value; }
        }

        public TodayViewModel()
        {
            var listOfDays = Enumerable.Range(-60, 121) //Will generate from 60 days into past to 60 days into future.
                .Select(i => 
                    new CalendarDay
                    {
                        Date = DateTime.Today.AddDays(i)
                    });

            CalendarDays = new ObservableCollection<CalendarDay>(listOfDays);
            SelectedDay = CalendarDays.Single(cd => cd.Date.Date == DateTime.Today.Date); //Default selection should be today.
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
