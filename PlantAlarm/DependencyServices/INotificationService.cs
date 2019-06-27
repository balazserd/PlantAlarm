using System;
using System.Collections.Generic;
using PlantAlarm.Models;

namespace PlantAlarm.DependencyServices
{
    public interface INotificationService
    {
        void CreateDailyReminders(List<List<TaskBase>> tasksForEveryDay);
    }
}
