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
    public static class NotificationService
    {
        private static readonly INotificationServiceProvider platformNotiSvc = DependencyService.Get<INotificationServiceProvider>();
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        static async Task AddDailyNotifications(int forTheNextXDays)
        {
            var activities = await GetUpcomingActivities(forTheNextXDays);

            platformNotiSvc.CreateDailyReminders(activities);
        }

        static async Task<List<List<PlantActivityItem>>> GetUpcomingActivities(int NumberOfDays)
        {
            var onetimeTaskList = await db.Table<PlantTask>().Where(pt => !pt.IsRepeating).ToListAsync();
            var recurringTaskList = await db.Table<PlantTask>().Where(pt => pt.IsRepeating).ToListAsync();

            //Can't use simple list, it is not thread safe.
            //As order is not guaranteed, we store the day in the second value of the tuple.
            ConcurrentBag<(List<PlantActivityItem>, int)> resultList = new ConcurrentBag<(List<PlantActivityItem>, int)>();

            var today = DateTime.Today;

            Parallel.For(0, //From today
                NumberOfDays + 1, //To today + X days
                (i) =>
                {
                    var tasksForDay = new List<PlantActivityItem>();
                    var thisDay = today.AddDays(i);

                    foreach (var recurringTask in recurringTaskList)
                    {
                        //Single Days.
                        if (thisDay.DayOfWeek == DayOfWeek.Monday && (recurringTask.OnMonday ?? false))       AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Tuesday && (recurringTask.OnTuesday ?? false))     AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Wednesday && (recurringTask.OnWednesday ?? false)) AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Thursday && (recurringTask.OnThursday ?? false))   AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Friday && (recurringTask.OnFriday ?? false))       AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Saturday && (recurringTask.OnSaturday ?? false))   AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Sunday && (recurringTask.OnSunday ?? false))       AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);

                        //If it is that day of the month.
                        if (recurringTask.OnDayOfMonth == thisDay.Day) AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);

                        //If it should occur every X days and {[number of days passed since the first occurrence] mod X} = 0.
                        if ((thisDay - recurringTask.FirstOccurrenceDate).TotalDays % recurringTask.EveryXDays == 0) AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);

                        //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                        if (((thisDay.Year - recurringTask.FirstOccurrenceDate.Year) * 12) + thisDay.Month - recurringTask.FirstOccurrenceDate.Month % recurringTask.EveryXMonths == 0 &&
                              thisDay.Day == recurringTask.FirstOccurrenceDate.Day)
                        {
                            AddActivityItemForDay(tasksForDay, recurringTask.Id, thisDay);
                        }
                    }

                    foreach (var onetimeTask in onetimeTaskList)
                    {
                        if (thisDay.Date == onetimeTask.FirstOccurrenceDate) AddActivityItemForDay(tasksForDay, onetimeTask.Id, thisDay);
                    }

                    resultList.Add(Tuple.Create(tasksForDay, i).ToValueTuple());
                });

            var result = resultList
                .OrderBy(tup => tup.Item2)
                .Select(tup => tup.Item1)
                .ToList();

            return result;
        }

        private static void AddActivityItemForDay(List<PlantActivityItem> taskList, int plantTaskId, DateTime day)
        {
            taskList.Add(new PlantActivityItem()
            {
                IsCompleted = false,
                PlantTaskId = plantTaskId,
                Time = day
            });
        }
    }
}
