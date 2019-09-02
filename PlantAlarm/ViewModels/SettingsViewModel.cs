using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlantAlarm.Enums;
using PlantAlarm.Services;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool isInTheProcessOfShowingExplanationAlert = false;

        private TimeSpan notificationTime { get; set; }
        public TimeSpan NotificationTime
        {
            get => notificationTime;
            set
            {
                if (value != notificationTime)
                {
                    notificationTime = value;
                    Application.Current.Properties[NotificationService.NotificationTimeKey] = value.ToString();

                    bool.TryParse(Application.Current.Properties[NotificationService.NotificationsEnabledKey].ToString(), out bool notiAllowed);
                    if (notiAllowed)
                    {
                        _ = NotificationService.AddDailyNotifications(notificationTime);
                    }
                }
            }
        }

        private double reminderLinesOpacity { get; set; }
        public double ReminderLinesOpacity
        {
            get => reminderLinesOpacity;
            set
            {
                reminderLinesOpacity = value;
                OnPropertyChanged();
            }
        }

        private bool areNotificationsEnabled { get; set; }
        public bool AreNotificationsEnabled
        {
            get => areNotificationsEnabled;
            set
            {
                areNotificationsEnabled = value;
                ReminderLinesOpacity = value ? 1.0 : 0.5;

                if (!isInTheProcessOfShowingExplanationAlert)
                {
                    HandleFakePermissionSwitchChange();
                }

                Application.Current.Properties[NotificationService.NotificationsEnabledKey] = value.ToString();
                if (value)
                {
                    _ = NotificationService.AddDailyNotifications();
                }
                else
                {
                    _ = NotificationService.RemoveDailyNotifications();
                }

                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            areNotificationsEnabled = NotificationService.AreNotificationsEnabled() == NotificationPermissionState.Allowed;
            
            if (Application.Current.Properties.TryGetValue(NotificationService.NotificationTimeKey, out object notiTimeAsObject))
            {
                if (TimeSpan.TryParse(notiTimeAsObject.ToString(), out TimeSpan timeOfNotification))
                {
                    notificationTime = timeOfNotification;
                }
            }
            else
            {
                notificationTime = TimeSpan.FromHours(8);
            }
        }

        private void HandleFakePermissionSwitchChange()
        {
            var notificationPermissionState = NotificationService.AreNotificationsEnabled();
            if (notificationPermissionState == NotificationPermissionState.Allowed)
            {
                //If notifications are enabled, we can handle it in-house, nothing to do.
                return;
            }
            if (notificationPermissionState == NotificationPermissionState.Unknown)
            {
                //If it is unknown, we should ask for them.
                NotificationService.AskForNotificationPermission();
                areNotificationsEnabled = NotificationService.AreNotificationsEnabled() == NotificationPermissionState.Allowed;
                OnPropertyChanged(nameof(AreNotificationsEnabled));
            }
            else
            {
                //If the notifications are denied for the app, explain that we cannot do anything in-house.
                isInTheProcessOfShowingExplanationAlert = true;
                AreNotificationsEnabled = false;
                NotificationService.ExplainNotificationPermissionHandling(() =>
                {
                    isInTheProcessOfShowingExplanationAlert = false;
                });
            }

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
