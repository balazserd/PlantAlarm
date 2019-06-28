using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Models;

namespace PlantAlarm.DependencyServices
{
    //WARNING!
    //These methods should never be used upfront via DependencyService.Get<T>().
    //Always call methods through NotificationService. This is just a provider for that class.
    public interface INotificationServiceProvider
    {
        void CreateDailyReminders(List<List<PlantActivityItem>> tasksForEveryDay, byte atHour = 8);
    }
}
