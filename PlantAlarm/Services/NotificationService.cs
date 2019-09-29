using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Enums;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    //This is the service that should be called instead of DependencyService.Get<INotificationServiceProvider>().
    public static class NotificationService
    { 
        private static readonly INotificationServiceProvider platformNotiSvc = DependencyService.Get<INotificationServiceProvider>();

        /// <summary>
        /// The key which stores in the App Properties dictionary whether notifications are enabled.
        /// </summary>
        public static string kNotificationsEnabled = "AreNotificationsEnabled";

        /// <summary>
        /// The key which stores in the App Properties dictionary the time at which notifications are shown during the day.
        /// </summary>
        public static string kNotificationTime = "NotificationTime";

        /// <summary>
        /// The key which stores in the App Properties dictionary whether forgotten tasks should be carried over.
        /// </summary>
        public static string kIsCarryingForgottenTasksForward = "IsCarryingForgottenTasksForward";

        public static async Task AddDailyNotifications(TimeSpan? timeOfDay = null)
        {
            var activities = await PlantActivityService.GetUpcomingActivitiesByDayAsync(DateTime.Today, DateTime.Today.AddDays(60));

            Task remindersTask = timeOfDay != null ?
                platformNotiSvc.CreateDailyReminders(activities, (byte)timeOfDay?.Hours, (byte)timeOfDay?.Minutes) :
                platformNotiSvc.CreateDailyReminders(activities);

            await remindersTask;
        }

        public static async Task RemoveDailyNotifications() => await platformNotiSvc.RemoveDailyReminders();

        public static NotificationPermissionState AreNotificationsEnabled() => platformNotiSvc.AreNotificationsEnabled();

        public static void ExplainNotificationPermissionHandling(Action completionhandler) => platformNotiSvc.ExplainNotificationPermissionHandling(completionhandler);

        public static void AskForNotificationPermission() => platformNotiSvc.AskForNotificationPermission();
    }
}
