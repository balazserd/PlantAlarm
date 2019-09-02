using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Enums;
using PlantAlarm.iOS.DependencyServices;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotificationService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class NotificationService_iOS : INotificationServiceProvider
    {
        UNUserNotificationCenter NotificationCenter = UNUserNotificationCenter.Current;
        private const string DAILY_NOTIFICATIONS = "DailyReminder";

        public async Task CreateDailyReminders(List<PlantActivityItem>[] listOfTasksForEveryDay, byte atHour = 8, byte atMinute = 0)
        {
            //First, remove all still pending notifications.
            await RemoveDailyReminders();

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
                    Minute = atMinute
                };

                var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
                var content = new UNMutableNotificationContent
                {
                    Title = "Your plants need you!",
                    Body = $"You have {listOfTasksForEveryDay[i].Count} task{(i > 1 ? "s" : "")} for today. Tap to see what you have to do in order to keep your (hopefully still) green friends happy.",
                    Sound = UNNotificationSound.Default,
                    CategoryIdentifier = DAILY_NOTIFICATIONS,
                };

                var request = UNNotificationRequest.FromIdentifier(
                    $"Daily_Notification_{date.Year}_{date.Month}_{date.Day}",
                    content,
                    trigger);

                await NotificationCenter.AddNotificationRequestAsync(request);
            }
        }

        public async Task RemoveDailyReminders()
        {
            var allNotificationRequests = await NotificationCenter.GetPendingNotificationRequestsAsync();

            var removableRequests = allNotificationRequests
                .Where(req => req.Content.CategoryIdentifier == DAILY_NOTIFICATIONS)
                .ToList();

            NotificationCenter.RemovePendingNotificationRequests(removableRequests.Select(req => req.Identifier).ToArray());
        }

        public NotificationPermissionState AreNotificationsEnabled()
        {
            UNNotificationSettings settings = UNUserNotificationCenter.Current.GetNotificationSettingsAsync().Result;

            switch (settings.AuthorizationStatus)
            {
                case UNAuthorizationStatus.Authorized: return NotificationPermissionState.Allowed;
                case UNAuthorizationStatus.Denied: return NotificationPermissionState.Denied;
                case UNAuthorizationStatus.NotDetermined: 
                case UNAuthorizationStatus.Provisional: return NotificationPermissionState.Unknown;
                default: throw new IndexOutOfRangeException("Unrecognized Notification Permission state.");
            }
        }

        public void AskForNotificationPermission()
        {
            UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                    (approved, error) => { });
        }

        public void ExplainNotificationPermissionHandling(Action completionHandler)
        {
            var alert = UIAlertController.Create(
                "Warning",
                "Notifications can only be allowed after denying them from the Settings App, under PlantAlarm > Notifications.",
                UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create(
                "Not now",
                UIAlertActionStyle.Cancel,
                null));
            alert.AddAction(UIAlertAction.Create(
                "Open Settings",
                UIAlertActionStyle.Default,
                (uiAlertAction) => {
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
                }));

            var window = new UIWindow(UIScreen.MainScreen.Bounds);
            var viewController = new UIViewController();
            viewController.View.BackgroundColor = UIColor.Clear;
            window.RootViewController = viewController;
            window.WindowLevel = UIWindowLevel.Alert + 1;
            window.MakeKeyAndVisible();

            viewController.PresentViewController(alert, true, () =>
            {
                window.DangerousAutorelease();
                completionHandler?.Invoke();
            });
        }
    }
}
