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

        public static async Task AddDailyNotifications(int forTheNextXDays)
        {
            var activities = await PlantActivityService.GetUpcomingActivitiesByDayAsync(DateTime.Today, DateTime.Today.AddDays(forTheNextXDays));

            platformNotiSvc.CreateDailyReminders(activities);
        }
    }
}
