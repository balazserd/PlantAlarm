using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.Enums;
using PlantAlarm.Services;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        private bool isInTheProcessOfShowingExplanationAlert = false;
        private readonly Page view;

        private TimeSpan notificationTime { get; set; }
        public TimeSpan NotificationTime
        {
            get => notificationTime;
            set
            {
                if (value != notificationTime)
                {
                    notificationTime = value;
                    Application.Current.Properties[NotificationService.kNotificationTime] = value.ToString();

                    bool.TryParse(Application.Current.Properties[NotificationService.kNotificationsEnabled].ToString(), out bool notiAllowed);
                    if (notiAllowed)
                    {
                        _ = NotificationService.AddDailyNotifications(notificationTime);
                    }
                }
            }
        }

        public double ReminderLinesOpacity
        {
            get => AreNotificationsEnabled ? 1.0 : 0.5;
        }

        private bool areNotificationsEnabled { get; set; }
        public bool AreNotificationsEnabled
        {
            get => areNotificationsEnabled;
            set
            {
                areNotificationsEnabled = value;

                if (!isInTheProcessOfShowingExplanationAlert)
                {
                    HandleFakePermissionSwitchChange();
                }

                Application.Current.Properties[NotificationService.kNotificationsEnabled] = value.ToString();
                if (value)
                {
                    _ = NotificationService.AddDailyNotifications();
                }
                else
                {
                    _ = NotificationService.RemoveDailyNotifications();
                }

                OnPropertyChanged(nameof(ReminderLinesOpacity));
                OnPropertyChanged();
            }
        }

        private bool isCarryingForgottenTasksForward { get; set; }
        public bool IsCarryingForgottenTasksForward
        {
            get => isCarryingForgottenTasksForward;
            set
            {
                isCarryingForgottenTasksForward = value;
                Application.Current.Properties[NotificationService.kIsCarryingForgottenTasksForward] = value.ToString();

                OnPropertyChanged();
            }
        }

        public ICommand ShowReminderPresentationTimeExplanation { get; private set; }
        public ICommand ShowCarryForwardExplanation { get; private set; }

        public SettingsViewModel(Page page)
        {
            view = page;
            areNotificationsEnabled = NotificationService.AreNotificationsEnabled() == NotificationPermissionState.Allowed;
            OnPropertyChanged(nameof(ReminderLinesOpacity));
            
            if (Application.Current.Properties.TryGetValue(NotificationService.kNotificationTime, out object notiTimeAsObject))
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

            ShowCarryForwardExplanation = new Command(async () =>
            {
                await view.DisplayAlert(
                    "Carry-forwarding Tasks",
                    "When you miss a task, the next time you log in, it will be carried over to that day's tasks (if that given day hasn't already got a task like that).",
                    "OK");
            });

            ShowReminderPresentationTimeExplanation = new Command(async () =>
            {
                await view.DisplayAlert(
                    "Reminder time",
                    "You will receive a push notification at this time of the day if you have tasks to complete.",
                    "OK");
            });
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
