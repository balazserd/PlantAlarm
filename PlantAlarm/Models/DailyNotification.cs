using System;
using System.Collections.Generic;

namespace PlantAlarm.Models
{
    public class DailyNotification
    {
        public List<TaskBase> TasksToPerform { get; set; }
        public DateTime Day { get; set; }
    }
}
