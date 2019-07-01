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
        public static async Task AddActivitiesFromTaskAsync(PlantTask task)
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
                    if ((thisDay.DayOfWeek == DayOfWeek.Monday && (task.OnMonday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Tuesday && (task.OnTuesday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Wednesday && (task.OnWednesday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Thursday && (task.OnThursday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Friday && (task.OnFriday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Saturday && (task.OnSaturday ?? false)) ||
                        (thisDay.DayOfWeek == DayOfWeek.Sunday && (task.OnSunday ?? false)) ||

                        //If it is that day of the month.
                        (task.OnDayOfMonth == thisDay.Day) ||

                        //If it should occur every X days and {[number of days passed since the first occurrence] mod X} = 0.
                        ((thisDay - task.FirstOccurrenceDate).TotalDays % task.EveryXDays == 0) ||

                        //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                        (((thisDay.Year - task.FirstOccurrenceDate.Year) * 12) + thisDay.Month - task.FirstOccurrenceDate.Month % task.EveryXMonths == 0 &&
                           thisDay.Day == task.FirstOccurrenceDate.Day)) 
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
        public static async Task RemoveActivitiesOfTaskAsync(PlantTask task)
        {
            var activitiesToDelete = await db.Table<PlantActivityItem>().Where(act => act.PlantTaskFk == task.Id).DeleteAsync();
        }

        /// <summary>
        /// Modifies the given activities in the database. For example, can mark them as done.
        /// </summary>
        /// <param name="activities">The list of activities to modify.</param>
        public static async Task ModifyActivitiesAsync(List<PlantActivityItem> activities)
        {
            await db.UpdateAllAsync(activities);
        }

        /// <summary>
        /// Returns the upcoming activities for the next few days.
        /// </summary>
        /// <param name="from">The first day for which to return the activities (inclusive).</param>
        /// <param name="to">The last day for which to return the activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>> GetUpcomingActivitiesAsync(DateTime from, DateTime to)
        {
            var activities = db.Table<PlantActivityItem>()
                .Where(act => act.Time.Date >= from.Date && act.Time.Date <= to.Date)
                .ToListAsync();

            return await activities;
        }

        /// <summary>
        /// Returns the upcoming activities for the given day.
        /// </summary>
        /// <param name="day">The day for which to return the activities.</param>
        public static async Task<List<PlantActivityItem>> GetUpcomingActivitiesAsync(DateTime day)
        {
            var activities = db.Table<PlantActivityItem>()
                .Where(act => act.Time.Date == day.Date)
                .ToListAsync();

            return await activities;
        }

        /// <summary>
        /// Returns the upcoming activities for the given days, grouping them based on day into an array.
        /// </summary>
        /// <param name="from">The first day for which to return the activities (inclusive).</param>
        /// <param name="to">The last day for which to return the activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>[]> GetUpcomingActivitiesByDayAsync(DateTime from, DateTime to)
        {
            var activities = await GetUpcomingActivities(from, to);
            int numberOfDays = (int)Math.Ceiling((to - from).TotalDays);

            var result = new List<PlantActivityItem>[numberOfDays];

            for (int i = 0; i <= numberOfDays; i++)
            {
                result[i] = activities.Where(act => act.Time.Day == from.Date.AddDays(i).Day).ToList();
            }

            return result;
        }
        #endregion

        #region PRIVATE methods
        private static void AddActivityItemForDay(List<PlantActivityItem> taskList, int plantTaskId, DateTime day)
        {
            taskList.Add(new PlantActivityItem
            {
                IsCompleted = false,
                PlantTaskFk = plantTaskId,
                Time = day
            });
        }
        #endregion
    }
}
