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
                GlobalProps[NotificationService.kIsCarryingForgottenTasksForward] = value;

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

        public ICommand ReviewCommand { get; private set; }

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

            //Retrieve saved value of camera roll save, or set default.
            bool couldGetSavePhotos = GlobalProps.TryGetValue(MediaService.kIsSavingPhotosToPhotoLibrary, out object isSavingPhotosToCameraRollAsObject);
            if (!couldGetSavePhotos)
            {
                GlobalProps[MediaService.kIsSavingPhotosToPhotoLibrary] = false;
            }
            this.IsCarryingForgottenTasksForward = couldGetSavePhotos ? bool.Parse(isSavingPhotosToCameraRollAsObject.ToString()) : false;

            //Retrieve saved value of carry-forwarding of activities, or set default.
            bool couldGetCarryForward = GlobalProps.TryGetValue(NotificationService.kIsCarryingForgottenTasksForward, out object isCarryingForwardAsObject);
            if (!couldGetCarryForward)
            {
                GlobalProps[NotificationService.kIsCarryingForgottenTasksForward] = true;
            }
            this.IsCarryingForgottenTasksForward = couldGetCarryForward ? (bool)isCarryingForwardAsObject : true;

            ReviewCommand = new Command(() =>
            {
                AppReviewService.InitiateReviewRequest();
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
