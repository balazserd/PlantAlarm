using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        UNUserNotificationCenter NotificationCenter = UNUserNotificationCenter.Current;

        public void CreateDailyReminders(List<PlantActivityItem>[] listOfTasksForEveryDay, byte atHour = 8, byte atMinute = 0)
        {
            //Create notifications for each day.
            for (int i = 0; i < listOfTasksForEveryDay.Count(); i++)
            {
                var date = DateTime.Now.AddDays(i);
                
                NSDateComponents dateComponents = new NSDateComponents
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Hour = atHour,
                    Minute = atMinute,
                };

                var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
                var content = new UNMutableNotificationContent
                {
                    Title = "Your plants need you!",
                    Body = $"You have {listOfTasksForEveryDay[i].Count} tasks for today. Tap to see what you have to do in order to keep your (hopefully still) green friends happy.",
                    Sound = UNNotificationSound.Default,
                    CategoryIdentifier = "DailyReminder",
                };

                var request = UNNotificationRequest.FromIdentifier(
                    $"Daily_Notification_{date.Year}_{date.Month}_{date.Day}",
                    content,
                    trigger);

                NotificationCenter.AddNotificationRequestAsync(request).Wait();
            }
        }

        public async Task RemoveDailyReminders(DateTime from, DateTime to)
        {
            var allNotificationRequests = await NotificationCenter.GetPendingNotificationRequestsAsync();

            var removableRequests = allNotificationRequests
                .Where(req =>
                {
                    NSDate notificationDate = (req.Trigger as UNCalendarNotificationTrigger).NextTriggerDate;
                    bool notBefore = notificationDate.SecondsSinceReferenceDate >= ((NSDate)from.Date).SecondsSinceReferenceDate;
                    bool notAfter = notificationDate.SecondsSinceReferenceDate <= ((NSDate)to.Date).SecondsSinceReferenceDate;

                    return notBefore && notAfter;
                })
                .ToList();

            NotificationCenter.RemovePendingNotificationRequests(removableRequests.Select(req => req.Identifier).ToArray());
        }
    }
}
