using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Models;

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
        void CreateDailyReminders(List<PlantActivityItem>[] listOfTasksForEveryDay, byte atHour = 8, byte atMinute = 0);

        /// <summary>
        /// Removes all daily reminders between the two specified dates.
        /// </summary>
        /// <param name="from">The first day to remove the daily notification for, inclusive.</param>
        /// <param name="to">The last day to remove the daily notification for, inslucive.</param>
        Task RemoveDailyReminders(DateTime from, DateTime to);
    }
}
