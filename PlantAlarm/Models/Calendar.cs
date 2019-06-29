using System;
using System.Globalization;

namespace PlantAlarm.Models
{
    public class Calendar
    {
        public Calendar()
        {
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
