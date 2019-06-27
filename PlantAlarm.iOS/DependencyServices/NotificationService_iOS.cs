using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using PlantAlarm.Models;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class NotificationService_iOS : INotificationService
    {
        public void CreateDailyReminders(List<List<TaskBase>> listOfTasksForEveryDay)
        {
            var notificationCenter = UNUserNotificationCenter.Current;

            //Create notifications for the next 63 days.
            for (int i = 0; i < 62; i++)
            {
                var date = DateTime.Now.AddDays(i);

                NSDateComponents dateComponents = new NSDateComponents
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Hour = 8 //TODO allow user to set time.
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
