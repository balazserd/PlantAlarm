using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using SQLite;

namespace PlantAlarm.Services
{
    public static class PlantActivityService
    {
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        #region PUBLIC methods
        /// <summary>
        /// Adds activities to the local storage from the specified Task for the next 2 months, last day inclusive.
        /// </summary>
        /// <param name="task">The PlantTask to create the activities for.</param>
        public static async Task AddActivitiesFromTask(PlantTask task)
        {
            List<PlantActivityItem> resultList = new List<PlantActivityItem>();

            var today = DateTime.Today;

            int days = (int)Math.Ceiling((DateTime.Now.AddMonths(2) - DateTime.Now).TotalDays);
            for (int i = 0; i < days; i++)
            {
                var thisDay = today.AddDays(i);

                if (task.IsRepeating)
                {
                    //Check the task's each recurring option.
                    //If any of the options indicate that the task should be performed this day, it gets added to the list.
                    //Single Days.
                    if (thisDay.DayOfWeek == DayOfWeek.Monday && (task.OnMonday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Tuesday && (task.OnTuesday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Wednesday && (task.OnWednesday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Thursday && (task.OnThursday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Friday && (task.OnFriday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Saturday && (task.OnSaturday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);
                    if (thisDay.DayOfWeek == DayOfWeek.Sunday && (task.OnSunday ?? false)) AddActivityItemForDay(resultList, task.Id, thisDay);

                    //If it is that day of the month.
                    if (task.OnDayOfMonth == thisDay.Day) AddActivityItemForDay(resultList, task.Id, thisDay);

                    //If it should occur every X days and {[number of days passed since the first occurrence] mod X} = 0.
                    if ((thisDay - task.FirstOccurrenceDate).TotalDays % task.EveryXDays == 0) AddActivityItemForDay(resultList, task.Id, thisDay);

                    //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                    if (((thisDay.Year - task.FirstOccurrenceDate.Year) * 12) + thisDay.Month - task.FirstOccurrenceDate.Month % task.EveryXMonths == 0 &&
                            thisDay.Day == task.FirstOccurrenceDate.Day)
                    {
                        AddActivityItemForDay(resultList, task.Id, thisDay);
                    }
                }
                else
                {
                    if (thisDay.Date == task.FirstOccurrenceDate) AddActivityItemForDay(resultList, task.Id, thisDay);
                }
            }

            await db.InsertAllAsync(resultList);
        }

        /// <summary>
        /// Removes all activities for the specified task from the local storage.
        /// </summary>
        /// <param name="task">The PlantTask which's activities should be removed.</param>
        public static async Task RemoveActivitiesOfTask(PlantTask task)
        {
            var activitiesToDelete = await db.Table<PlantActivityItem>().Where(act => act.PlantTaskFk == task.Id).DeleteAsync();
        }

        /// <summary>
        /// Returns the upcoming activities for the next few days.
        /// </summary>
        /// <param name="NumberOfDays">The number of days into the future until which to collect activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>> GetUpcomingActivities(int NumberOfDays)
        {
            var activities = db.Table<PlantActivityItem>()
                .Where(act => act.Time >= DateTime.Now && act.Time <= DateTime.Now.AddDays(NumberOfDays))
                .ToListAsync();

            return await activities;
        }

        /// <summary>
        /// Returns the upcoming activities for the next few days, grouping them based on day into an array.
        /// </summary>
        /// <param name="NumberOfDays">The number of days into the future until which to collect activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>[]> GetUpcomingActivitiesByDay(int NumberOfDays)
        {
            var activities = await GetUpcomingActivities(NumberOfDays);

            var result = new List<PlantActivityItem>[NumberOfDays + 1];

            for (int i = 0; i <= NumberOfDays; i++)
            {
                result[i] = activities.Where(act => act.Time.Day == DateTime.Now.AddDays(i).Day).ToList();
            }

            return result;
        }
        #endregion

        #region PRIVATE methods
        private static void AddActivityItemForDay(List<PlantActivityItem> taskBag, int plantTaskId, DateTime day)
        {
            taskBag.Add(new PlantActivityItem
            {
                IsCompleted = false,
                PlantTaskFk = plantTaskId,
                Time = day
            });
        }
        #endregion
    }
}
