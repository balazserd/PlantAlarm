using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Models;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    //This is the service that should be called instead of DependencyService.Get<INotificationServiceProvider>().
    public static class NotificationService
    {
        private static readonly INotificationServiceProvider platformNotiSvc = DependencyService.Get<INotificationServiceProvider>();
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        static async Task AddDailyNotifications(int forTheNextXDays)
        {
            var activities = await PlantActivityService.GetUpcomingActivitiesByDay(forTheNextXDays);

            platformNotiSvc.CreateDailyReminders(activities);
        }
    }
}
