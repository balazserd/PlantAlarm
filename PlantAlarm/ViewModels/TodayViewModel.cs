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
        public List<CalendarDay> CalendarDays { get; private set; }

        private CalendarDay selectedDay { get; set; }
        public CalendarDay SelectedDay
        {
            get => selectedDay;
            set { selectedDay = value; OnPropertyChanged(); }
        }

        public TodayViewModel()
        {
            var listOfDays = Enumerable.Range(-120, 241) //Will generate plus/minus 4 months. TODO: maybe a better calendar option.
                .Select(i => 
                    new CalendarDay
                    {
                        Date = DateTime.Today.AddDays(i)
                    });

            CalendarDays = listOfDays.ToList();
            SelectedDay = CalendarDays.Single(cd => cd.Date.Date == DateTime.Today.Date); //Default selection should be today.
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
