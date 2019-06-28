using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using PlantAlarm.Models;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class NotificationService_iOS : INotificationServiceProvider
    {
        public void CreateDailyReminders(List<List<PlantActivityItem>> listOfTasksForEveryDay, byte atHour = 8)
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            //Create notifications for each day.
            for (int i = 0; i < listOfTasksForEveryDay.Count; i++)
            {
                var date = DateTime.Now.AddDays(i);

                NSDateComponents dateComponents = new NSDateComponents
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Hour = atHour
                };

                var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
                var content = new UNMutableNotificationContent
                {
                    Title = "Your plants need you!",
                    Body = $"You have {listOfTasksForEveryDay[i].Count} tasks for today.",
                    Sound = UNNotificationSound.Default,
                };

                var request = UNNotificationRequest.FromIdentifier(
                    $"Daily_Notification_{date.Year}_{date.Month}_{date.Day}",
                    content,
                    trigger);

                notificationCenter.AddNotificationRequest(request, (error) => { /*TODO handle */ });
            }
        }
    }
}
