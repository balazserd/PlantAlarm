using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Enums;
using PlantAlarm.Models;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    //This is the service that should be called instead of DependencyService.Get<INotificationServiceProvider>().
    public static class NotificationService
    {
        private static readonly INotificationServiceProvider platformNotiSvc = DependencyService.Get<INotificationServiceProvider>();

        public static async Task AddDailyNotifications()
        {
            var activities = await PlantActivityService.GetUpcomingActivitiesByDayAsync(DateTime.Today, DateTime.Today.AddDays(30));

            await platformNotiSvc.CreateDailyReminders(activities);
        }

        public static NotificationPermissionState AreNotificationsEnabled() => platformNotiSvc.AreNotificationsEnabled();

        public static void ExplainNotificationPermissionHandling(Action completionhandler) => platformNotiSvc.ExplainNotificationPermissionHandling(completionhandler);

        public static void AskForNotificationPermission() => platformNotiSvc.AskForNotificationPermission();
    }
}
