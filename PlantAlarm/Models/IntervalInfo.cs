using System;
namespace PlantAlarm.Models
{
    public class IntervalInfo
    {
        //If used weekly.
        public bool OnMonday { get; set; }
        public bool OnTuesday { get; set; }
        public bool OnWednesday { get; set; }
        public bool OnThursday { get; set; }
        public bool OnFriday { get; set; }
        public bool OnSaturday { get; set; }
        public bool OnSunday { get; set; }

        //If periodical, but not weekly.
        public byte? EveryXDays { get; set; }

        //If used monthly.
        public byte? OnDayOfMonth { get; set; }
        public byte? EveryXMonths { get; set; } //Set 12 for yearly.
    }
}
