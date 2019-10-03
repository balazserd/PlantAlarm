using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    public static class PlantActivityService
    {
        private static readonly SQLiteAsyncConnection asyncDb = App.LocalDbConnection.AsyncDb;
        private static readonly SQLiteConnection Db = App.LocalDbConnection.Db;

        #region PUBLIC methods
        /// <summary>
        /// Adds the PlantTask to the local storage, asynchronously. Then, creates the associated PlantActivityItems.
        /// </summary>
        /// <param name="task">The PlantTask to add.</param>
        /// <returns></returns>
        public static async Task AddPlantTaskAsync(PlantTask task)
        {
            await asyncDb.InsertAsync(task);
            await AddActivitiesFromTaskAsync(task, DateTime.Today);
        }

        /// <summary>
        /// Removes a PlantTask and its future PlantActivityItems and all PlantConnections.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task RemoveTaskAsync(PlantTask task)
        {
            await asyncDb.DeleteAsync(task);
            await RemoveActivitiesOfTaskAsync(task);
            await RemovePlantTaskPlantConnectionsForPlantTaskAsync(task);
        }

        /// <summary>
        /// Removes all tasks specified. Also removes all non-completed PlantActivityItems and all PlantConnections associated with them.
        /// </summary>
        /// <param name="tasks">The tasks to remove.</param>
        /// <returns></returns>
        public static async Task RemoveTasksAsync(List<PlantTask> tasks)
        {
            await asyncDb.Table<PlantTask>()
                .Where(plantTask => tasks.Any(t => t.Id == plantTask.Id))
                .DeleteAsync();
            await RemovesActivitesOfTasksAsync(tasks);
            await RemovePlantTaskPlantConnectionsForPlantTasksAsync(tasks);
        }

        /// <summary>
        /// Adds all PlantTask and Plant connection items to the local storage, asynchronously.
        /// </summary>
        /// <param name="connections">The list of connections to add.</param>
        /// <returns></returns>
        public static async Task AddPlantTaskPlantConnectionsAsync(List<PlantTaskPlantConnection> connections)
        {
            await asyncDb.InsertAllAsync(connections);
        }

        /// <summary>
        /// Removes the Plant association items for the PlantTask.
        /// </summary>
        /// <param name="task">The PlantTask to remove Plant connections for.</param>
        /// <returns></returns>
        public static async Task RemovePlantTaskPlantConnectionsForPlantTaskAsync(PlantTask task)
        {
            await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => conn.PlantTaskFk == task.Id)
                .DeleteAsync();
        }

        /// <summary>
        /// Removes the Plant association items for the specified PlantTasks.
        /// </summary>
        /// <param name="tasks">The tasks for which to remove all PlantConnection items.</param>
        /// <returns></returns>
        private static async Task RemovePlantTaskPlantConnectionsForPlantTasksAsync(List<PlantTask> tasks)
        {
            await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => tasks.Any(t => t.Id == conn.PlantTaskFk))
                .DeleteAsync();
        }

        /// <summary>
        /// Adds activities to the local storage from the specified Task for the next 61 days, last day inclusive, today inclusive.
        /// Tasks after firstDay will be deleted, even if they are marked as completed.
        /// </summary>
        /// <param name="task">The PlantTask to create the activities for.</param>
        /// <param name="firstDay">The first (possible) day to add an activity for.</param>
        public static async Task AddActivitiesFromTaskAsync(PlantTask task, DateTime firstDay)
        {
            var allActivities = await GetUpcomingActivitiesAsync(DateTime.Today, DateTime.Today.AddDays(60));

            //First, we need to delete all activities that are after 'firstDay'.
            await asyncDb.Table<PlantActivityItem>()
                .Where(act => act.PlantTaskFk == task.Id && act.Time >= firstDay)
                .DeleteAsync();

            //Now add for the next 61 days.
            List<PlantActivityItem> resultList = new List<PlantActivityItem>();

            for (DateTime day = firstDay; day <= DateTime.Today.AddDays(60); day = day.AddDays(1))
            {
                if (task.IsRepeating)
                {
                    //Check the task's each recurring option.
                    //If any of the options indicate that the task should be performed this day, it gets added to the list.

                    //Single Days.
                    if ((day.DayOfWeek == DayOfWeek.Monday && task.OnMonday) ||
                        (day.DayOfWeek == DayOfWeek.Tuesday && task.OnTuesday) ||
                        (day.DayOfWeek == DayOfWeek.Wednesday && task.OnWednesday) ||
                        (day.DayOfWeek == DayOfWeek.Thursday && task.OnThursday) ||
                        (day.DayOfWeek == DayOfWeek.Friday && task.OnFriday) ||
                        (day.DayOfWeek == DayOfWeek.Saturday && task.OnSaturday) ||
                        (day.DayOfWeek == DayOfWeek.Sunday && task.OnSunday) ||

                        //If it should occur every X days and {[number of days passed since the first occurrence] mod X} = 0.
                        (task.EveryXDays > 0 && (day - task.FirstOccurrenceDate).Days % task.EveryXDays == 0) ||

                        //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                        (task.EveryXMonths > 0 && ((day.Year - task.FirstOccurrenceDate.Year) * 12) + day.Month - task.FirstOccurrenceDate.Month % task.EveryXMonths == 0 &&
                           day.Day == task.FirstOccurrenceDate.Day))
                    {
                        AddActivityItemForDay(resultList, task.Id, day);
                    }
                }
                else
                {
                    if (day.Date == task.FirstOccurrenceDate) AddActivityItemForDay(resultList, task.Id, day);
                }
            }

            await asyncDb.InsertAllAsync(resultList);
            await RecreateDailyReminders();
        }

        /// <summary>
        /// Creates all activities for the next 60 days that haven't been created yet.
        /// You should NOT call this method when the task has changed since the last AddActivitiesFromTaskAsync() call.
        /// </summary>
        /// <returns></returns>
        public static async Task CreateAllMissingActivitiesForNext60Days()
        {
            var tasks = await asyncDb.Table<PlantTask>().ToListAsync();
            var allActivities = await GetUpcomingActivitiesAsync(DateTime.Today, DateTime.Today.AddDays(60));

            //Select latest activities per task.
            var latestActivitiesTimePerTask = allActivities
                .GroupBy(act => act.PlantTaskFk)
                .Select(actOfTask => new {
                    PlantTask = GetTaskOfActivity(actOfTask.Select(act => act).First()),
                    Time = actOfTask.Max(act => act.Time).AddDays(1)
                })
                .ToList();

            //Select task Ids that have no activities in future - these will not be found in GetUpcomingActivitieAsync()
            // - then create an item from these, adding today as the first day to create an activity for.
            var tasksWithNoActivitiesInFuture = tasks
                .Where(t => !latestActivitiesTimePerTask
                    .Select(ac => ac.PlantTask.Id)
                    .Any(id => id == t.Id))
                .Select(t => new
                {
                    PlantTask = t,
                    Time = DateTime.Today
                });

            //Combine the two lists.
            var tasksToCreateActivitiesForCombined = tasksWithNoActivitiesInFuture
                .Union(latestActivitiesTimePerTask);

            foreach (var activity in tasksToCreateActivitiesForCombined)
            {
                await AddActivitiesFromTaskAsync(activity.PlantTask, activity.Time);
            }
        }

        /// <summary>
        /// Removes all activities for the specified task from the local storage.
        /// </summary>
        /// <param name="task">The PlantTask which's activities should be removed.</param>
        public static async Task RemoveActivitiesOfTaskAsync(PlantTask task)
        {
            await asyncDb.Table<PlantActivityItem>()
                .Where(act => act.PlantTaskFk == task.Id && act.Time > DateTime.Now)
                .DeleteAsync();
            await RecreateDailyReminders();
        }

        /// <summary>
        /// Removes the future activites of the specified tasks.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static async Task RemovesActivitesOfTasksAsync(List<PlantTask> tasks)
        {
            var activityItems = await asyncDb.Table<PlantActivityItem>()
                .ToListAsync();

            var itemsToDelete = activityItems
                .Where(ai => ai.Time > DateTime.Now && tasks.Any(t => t.Id == ai.PlantTaskFk))
                .Select(ai => ai.Id);

            await asyncDb.Table<PlantActivityItem>()
                .Where(ai => itemsToDelete.Any(del => del == ai.Id))
                .DeleteAsync();

            await RecreateDailyReminders();
        }

        public static async Task ModifyActivityAsync(PlantActivityItem activity, bool shouldRecreateDailyReminders = true)
        {
            await asyncDb.UpdateAsync(activity);

            if (shouldRecreateDailyReminders)
                await RecreateDailyReminders();
        }

        /// <summary>
        /// Modifies the given activities in the database. For example, can mark them as done.
        /// </summary>
        /// <param name="activities">The list of activities to modify.</param>
        public static async Task ModifyActivitiesAsync(List<PlantActivityItem> activities, bool shouldRecreateDailyReminders = true)
        {
            await asyncDb.UpdateAllAsync(activities);

            if (shouldRecreateDailyReminders)
                await RecreateDailyReminders();
        }

        /// <summary>
        /// Returns the upcoming activities for the next few days.
        /// </summary>
        /// <param name="from">The first day for which to return the activities (inclusive).</param>
        /// <param name="to">The last day for which to return the activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>> GetUpcomingActivitiesAsync(DateTime from, DateTime to)
        {
            var activities = await asyncDb.Table<PlantActivityItem>()
                .ToListAsync();

            return activities
                .Where(act => act.Time.Date >= from.Date && act.Time.Date <= to.Date)
                .ToList();
        }

        /// <summary>
        /// Returns the upcoming activities for the given day.
        /// </summary>
        /// <param name="day">The day for which to return the activities.</param>
        public static async Task<List<PlantActivityItem>> GetUpcomingActivitiesAsync(DateTime day)
        {
            var activities = await asyncDb.Table<PlantActivityItem>()
                .ToListAsync();

            return activities
                .Where(act => act.Time.Date == day.Date)
                .ToList();
        }

        /// <summary>
        /// Returns the upcoming activities for the given days, grouping them based on day into an array.
        /// </summary>
        /// <param name="from">The first day for which to return the activities (inclusive).</param>
        /// <param name="to">The last day for which to return the activities (inclusive).</param>
        public static async Task<List<PlantActivityItem>[]> GetUpcomingActivitiesByDayAsync(DateTime from, DateTime to)
        {
            var activities = await GetUpcomingActivitiesAsync(from, to);
            int numberOfDays = (int)Math.Ceiling((to - from).TotalDays);

            var result = new List<PlantActivityItem>[numberOfDays + 1];

            for (int i = 0; i <= numberOfDays; i++)
            {
                result[i] = activities.Where(act => act.Time.Day == from.Date.AddDays(i).Day).ToList();
            }

            return result;
        }

        /// <summary>
        /// Gets (up to) 7 upcoming tasks for the plant, ordering earlier tasks higher.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<ExtendedPlantActivityViewModel> GetUpcomingActivitiesOfPlant(Plant p)
        {
            var taskConnectionsOfPlant = Db.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantFk == p.Id)
                .ToList();

            var tasksOfPlant = Db.Table<PlantTask>()
                .ToList()
                .Where(pt => taskConnectionsOfPlant.Any(tc => tc.PlantTaskFk == pt.Id))
                .ToList();

            var upcomingActivities = Db.Table<PlantActivityItem>()
                .ToList()
                .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) && pai.Time.Date >= DateTime.Today)
                .Select(pai =>
                {
                    return new ExtendedPlantActivityViewModel()
                    {
                        PlantTask = tasksOfPlant.First(t => t.Id == pai.PlantTaskFk),
                        PlantActivityItem = pai
                    };
                });

            return upcomingActivities.OrderBy(extAct => extAct.PlantActivityItem.Time).Take(5).ToList();
        }

        /// <summary>
        /// Gets the list of plants the given Activity must be performed on, asynchronously.
        /// </summary>
        /// <param name="activity">The Activity for which the associated plants must be returned.</param>
        /// <returns></returns>
        public static async Task<List<Plant>> GetPlantsOfActivityAsync(PlantActivityItem activity)
        {
            var plantTask = await GetTaskOfActivityAsync(activity);

            var plantConnections = await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantTaskFk == plantTask.Id)
                .ToListAsync();

            var plants = await asyncDb.Table<Plant>()
                .ToListAsync();

            return plants
                .Where(p => plantConnections
                    .Select(pc => pc.PlantFk)
                    .Contains(p.Id))
                .ToList();
        }

        /// <summary>
        /// Gets the list of plants the given Activity must be performed on.
        /// </summary>
        /// <param name="activity">The Activity for which the associated plants must be returned.</param>
        /// <returns></returns>
        public static List<Plant> GetPlantsOfActivity(PlantActivityItem activity)
        {
            var plantTask = GetTaskOfActivity(activity);

            var plantConnections = Db.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantTaskFk == plantTask.Id)
                .ToList();

            var plants = Db.Table<Plant>()
                .ToList();

            return plants
                .Where(p => plantConnections
                    .Select(pc => pc.PlantFk)
                    .Contains(p.Id))
                .ToList();
        }

        public static async Task<PlantTask> GetTaskOfActivityAsync(PlantActivityItem activity)
        {
            PlantTask plantTask;
            try
            {
                plantTask = await asyncDb.Table<PlantTask>()
                    .FirstAsync(task => task.Id == activity.PlantTaskFk);
            }
            catch (Exception)
            {
                throw new PlantActivityServiceException("Could not retrieve the single PlantTask from which this PlantActivityItem was created.");
            }

            return plantTask;
        }

        public static PlantTask GetTaskOfActivity(PlantActivityItem activity)
        {
            PlantTask plantTask;
            try
            {
                plantTask = Db.Table<PlantTask>()
                    .First(task => task.Id == activity.PlantTaskFk);
            }
            catch (Exception)
            {
                throw new PlantActivityServiceException("Could not retrieve the single PlantTask from which this PlantActivityItem was created.");
            }

            return plantTask;
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

        private static async Task RecreateDailyReminders()
        {
            await NotificationService.AddDailyNotifications();
        }
        #endregion
    }

    public class PlantActivityServiceException : Exception
    {
        public PlantActivityServiceException(string message) : base(message) { }
    }

    public class PlantTaskEqualityComparer : EqualityComparer<PlantTask>
    {
        public override bool Equals(PlantTask x, PlantTask y)
        {
            return x.Id == y.Id;
        }

        public override int GetHashCode(PlantTask obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
