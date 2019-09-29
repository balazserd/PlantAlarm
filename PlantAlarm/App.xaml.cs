﻿using System;
using System.IO;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PlantAlarm
{
    public partial class App : Application
    {
        public static LocalDbConnection LocalDbConnection;

        public App()
        {
            InitializeComponent();

            //Database connections
            LocalDbConnection = new LocalDbConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantAlarmSQLite.db3"));

            string localFolderUrl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");
            if (!Directory.Exists(localFolderUrl))
            {
                Directory.CreateDirectory(localFolderUrl);
            }

            //Create missing activities
            _ = PlantActivityService.CreateAllMissingActivitiesForNext60Days();

            Application.Current.Properties.TryGetValue(NotificationService.kNotificationTime, out object sNotiTime);
            TimeSpan? notiTime = TimeSpan.TryParse(sNotiTime.ToString(), out TimeSpan _notiTime) ? _notiTime : (TimeSpan?)null;
            _ = NotificationService.AddDailyNotifications(notiTime);

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
