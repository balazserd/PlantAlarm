using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Enums;

namespace PlantAlarm.DependencyServices
{
    //WARNING!
    //These methods should never be used upfront via DependencyService.Get<T>().
    //Always call methods through NotificationService. This is just a provider for that class.
    public interface INotificationServiceProvider
    {
        /// <summary>
        /// Creates daily notifications ahead based on the supplied array of lists of activities.
        /// </summary>
        /// <param name="listOfTasksForEveryDay">The list containing the activities for each day. It is assumed that the first item in the list is the activities for today,
        /// and every subsequent element is the collection of activities for the next day.</param>
        /// <param name="atHour">The hour at which the notification should be displayed. Defaults to 8.</param>
        /// <param name="atMinute">The minute at which the notification should be displayed. Defaults to 0.</param>
        Task CreateDailyReminders(List<PlantActivityItem>[] listOfTasksForEveryDay, byte atHour = 8, byte atMinute = 0);

        /// <summary>
        /// Removes all daily reminders.
        /// </summary>
        Task RemoveDailyReminders();

        /// <summary>
        /// Checks whether notifications have been enabled.
        /// </summary>
        /// <returns></returns>
        NotificationPermissionState AreNotificationsEnabled();

        /// <summary>
        /// Asks for permissions of notifications.
        /// </summary>
        /// <returns></returns>
        void AskForNotificationPermission();

        /// <summary>
        /// Presents an alert, explaining that Notification settings can only be modified from the Settings app after the initial answer. Then, executes the completionHandler.
        /// </summary>
        /// <param name="completionHandler">The code to execute when the method returns.</param>
        void ExplainNotificationPermissionHandling(Action completionHandler);
    }
}
