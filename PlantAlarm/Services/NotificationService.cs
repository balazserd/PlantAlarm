using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Models;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    //This is the service that should be called instead of DependencyService.Get<INotificationServiceProvider>().
    public static class NotificationService
    {
        private static readonly INotificationServiceProvider platformNotiSvc = DependencyService.Get<INotificationServiceProvider>();
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        static async Task AddDailyNotifications(int forTheNextXDays)
        {
            var activities = await GetUpcomingActivitiesByDay(forTheNextXDays);

            platformNotiSvc.CreateDailyReminders(activities);
        }

        static async Task<List<PlantActivityItem>> GetUpcomingActivities(int NumberOfDays)
        {
            var activities = db.Table<PlantActivityItem>()
                .Where(act => act.Time >= DateTime.Now && act.Time <= DateTime.Now.AddDays(NumberOfDays))
                .ToListAsync();

            return await activities;
        }

        static async Task<List<PlantActivityItem>[]> GetUpcomingActivitiesByDay(int NumberOfDays)
        {
            var activities = await GetUpcomingActivities(NumberOfDays);

            var result = new List<PlantActivityItem>[NumberOfDays + 1];

            for (int i = 0; i <= NumberOfDays; i++)
            {
                result[i] = activities.Where(act => act.Time.Day == DateTime.Now.AddDays(i).Day).ToList();
            }

            return result;
        }
    }
}
