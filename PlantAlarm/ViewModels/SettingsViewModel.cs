using System;
using System.Collections.Generic;
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
        private readonly IDictionary<string, object> GlobalProps = Application.Current.Properties;

        private TimeSpan notificationTime { get; set; }
        public TimeSpan NotificationTime
        {
            get => notificationTime;
            set
            {
                if (value != notificationTime)
                {
                    notificationTime = value;
                    GlobalProps[NotificationService.kNotificationTime] = value.ToString();

                    bool.TryParse(GlobalProps[NotificationService.kNotificationsEnabled].ToString(), out bool notiAllowed);
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

                GlobalProps[NotificationService.kNotificationsEnabled] = value.ToString();
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
                GlobalProps[NotificationService.kIsCarryingForgottenTasksForward] = value.ToString();

                OnPropertyChanged();
            }
        }

        public bool IsSavingPhotosToCameraRoll
        {
            get {
                if (MediaService.IsPermittedToAccessPhotoLibraries())
                {
                    return MediaService.IsSavingPhotosToPhotoLibrary();
                }
                GlobalProps[MediaService.kIsSavingPhotosToPhotoLibrary] = false;
                return false;
            }
            set
            {
                if (MediaService.IsPermittedToAccessPhotoLibraries())
                {
                    GlobalProps[MediaService.kIsSavingPhotosToPhotoLibrary] = value;
                }
                else
                {
                    MediaService.RequestPermissionToPhotoLibraries();

                    if (MediaService.IsPermittedToAccessPhotoLibraries())
                    {
                        GlobalProps[MediaService.kIsSavingPhotosToPhotoLibrary] = value;
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() => MediaService.ShowExplanatoryTextAfterDenyingPhotoLibraryRequest());
                    }
                }

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
            
            if (GlobalProps.TryGetValue(NotificationService.kNotificationTime, out object notiTimeAsObject))
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

            if (!GlobalProps.TryGetValue(MediaService.kIsSavingPhotosToPhotoLibrary, out object _isSavingPhotosToCameraRoll))
            {
                GlobalProps[MediaService.kIsSavingPhotosToPhotoLibrary] = false;
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
