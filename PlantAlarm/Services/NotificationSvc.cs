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
    public class NotificationSvc
    {
        INotificationService platformNotiSvc = DependencyService.Get<INotificationService>();
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        public NotificationSvc()
        {
        }

        static async Task<List<List<PlantActivityItem>>> GetUpcomingActivities(int NumberOfDays)
        {
            var onetimeTaskList = await db.Table<PlantTask>().Where(pt => !pt.IsRepeating).ToListAsync();
            var recurringTaskList = await db.Table<PlantTask>().Where(pt => pt.IsRepeating).ToListAsync();

            //Can't use simple list, it is not thread safe.
            //As order is not guaranteed, we store the day in the second value of the tuple.
            ConcurrentBag<(List<PlantActivityItem>, int)> resultList = new ConcurrentBag<(List<PlantActivityItem>, int)>();

            var today = DateTime.Today;

            Parallel.For(0,
                NumberOfDays + 1,
                (i) =>
                {
                    var tasksForDay = new List<PlantActivityItem>();
                    var thisDay = today.AddDays(i);

                    foreach (var plantTask in recurringTaskList)
                    {
                        //Single Days.
                        if (thisDay.DayOfWeek == DayOfWeek.Monday && (plantTask.OnMonday ?? false))       AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Tuesday && (plantTask.OnTuesday ?? false))     AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Wednesday && (plantTask.OnWednesday ?? false)) AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Thursday && (plantTask.OnThursday ?? false))   AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Friday && (plantTask.OnFriday ?? false))       AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Saturday && (plantTask.OnSaturday ?? false))   AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        if (thisDay.DayOfWeek == DayOfWeek.Sunday && (plantTask.OnSunday ?? false))       AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);

                        //If it is that day of the month.
                        if (plantTask.OnDayOfMonth == thisDay.Day) AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);

                        //If it should occur every X days and {[number of days passed since the first occurrence] mod X} = 0.
                        if ((thisDay - plantTask.FirstOccurrenceDate).TotalDays % plantTask.EveryXDays == 0) AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);

                        //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                        if (((thisDay.Year - plantTask.FirstOccurrenceDate.Year) * 12) + thisDay.Month - plantTask.FirstOccurrenceDate.Month % plantTask.EveryXMonths == 0 &&
                              thisDay.Day == plantTask.FirstOccurrenceDate.Day)
                        {
                            AddActivityItemForDay(tasksForDay, plantTask.Id, thisDay);
                        }
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
