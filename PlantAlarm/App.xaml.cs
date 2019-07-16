using System;
using System.IO;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PlantAlarm
{
    public partial class Application : Xamarin.Forms.Application
    {
        public static LocalDbConnection LocalDbConnection;

        public Application()
        {
            InitializeComponent();

            LocalDbConnection = new LocalDbConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantAlarmSQLite.db3"));

            string localFolderUrl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");
            if (!Directory.Exists(localFolderUrl))
            {
                Directory.CreateDirectory(localFolderUrl);
            }

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
