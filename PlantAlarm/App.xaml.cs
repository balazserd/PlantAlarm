using System;
using System.IO;
using PlantAlarm.DatabaseModels;
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

            LocalDbConnection = new LocalDbConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantAlarmSQLite.db3"));

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
