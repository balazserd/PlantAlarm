﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
        /// <param name="plantsToPerformTaskOn">The list of Plants this task should be performed on.</param>
        /// <returns>A Task object that, when finished, creates the resources mentioned above.</returns>
        public static async Task AddPlantTaskAsync(PlantTask task, List<Plant> plantsToPerformTaskOn)
        {
            //Inserting the new PlantTask into DB.
            await asyncDb.InsertAsync(task); //This will populate the id field.

            //Creating the PlantTask - Plant connections.
            var connectionList = new List<PlantTaskPlantConnection>();
            foreach (var plant in plantsToPerformTaskOn)
            {
                var plantTaskPlantConnection = new PlantTaskPlantConnection
                {
                    PlantFk = plant.Id,
                    PlantTaskFk = task.Id
                };

                connectionList.Add(plantTaskPlantConnection);
            }
            await PlantActivityService.AddPlantTaskPlantConnectionsAsync(connectionList);

            //Adding activities for the task + recreating the daily reminders.
            await AddActivitiesFromTaskAsync(task, DateTime.Today);
        }

        public static async Task ModifyPlantTaskAsync(PlantTask task, List<Plant> plantsToPerformTaskOn)
        {
            var oldConnections = await PlantActivityService.GetPlantTaskPlantConnectionsAsync(task);

            //Remove old connections.
            var plantIdsToRemove = oldConnections
                .Select(oldConn => oldConn.PlantFk)
                .Except(plantsToPerformTaskOn.Select(p => p.Id));

            var connectionsToRemove = (await asyncDb
                .Table<PlantTaskPlantConnection>()
                .Where(conn => conn.PlantTaskFk == task.Id) //Only this task
                .ToListAsync())
                .Where(conn => plantIdsToRemove.Any(id => id == conn.PlantFk)); //Only the plants to remove

            await Task.WhenAll(connectionsToRemove
                .Select(conn => asyncDb.DeleteAsync(conn)));
            
            //Add new connections.
            var connectionsToAdd = plantsToPerformTaskOn
                .Select(p => p.Id)
                .Except(oldConnections.Select(oldConn => oldConn.PlantFk))
                .Select(id => new PlantTaskPlantConnection()
                {
                    PlantFk = id,
                    PlantTaskFk = task.Id
                });

            await asyncDb.InsertAllAsync(connectionsToAdd);

            //Update the task and recreate the activities.
            await asyncDb.UpdateAsync(task);
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

        public static async Task<List<PlantTaskPlantConnection>> GetPlantTaskPlantConnectionsAsync(PlantTask task)
        {
            var dconnections = await asyncDb.Table<PlantTaskPlantConnection>()
                .ToListAsync();

            var connections = await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => conn.PlantTaskFk == task.Id)
                .ToListAsync();

            return connections;
        }

        public static async Task<List<PlantTaskPlantConnection>> GetPlantTaskPlantConnectionsAsync(Plant plant)
        {
            var dconnections = await asyncDb.Table<PlantTaskPlantConnection>()
                .ToListAsync();

            var connections = await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => conn.PlantFk == plant.Id)
                .ToListAsync();

            return connections;
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
        /// Removes the PlantTask association items for the Plant.
        /// </summary>
        /// <param name="task">The Plant to remove PlantTask connections for.</param>
        /// <returns></returns>
        public static async Task RemovePlantTaskPlantConnectionsForPlantTaskAsync(Plant plant)
        {
            await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => conn.PlantFk == plant.Id)
                .DeleteAsync();
        }

        /// <summary>
        /// Removes the Plant association items for the specified PlantTasks.
        /// </summary>
        /// <param name="tasks">The tasks for which to remove all PlantConnection items.</param>
        /// <returns></returns>
        public static async Task RemovePlantTaskPlantConnectionsForPlantTasksAsync(List<PlantTask> tasks)
        {
            await asyncDb.Table<PlantTaskPlantConnection>()
                .Where(conn => tasks.Any(t => t.Id == conn.PlantTaskFk))
                .DeleteAsync();
        }

        public static List<PlantTask> GetAllTasks()
        {
            return Db.Table<PlantTask>().ToList();
        }

        public static async Task<List<PlantTask>> GetAllTasksAsync()
        {
            return await asyncDb.Table<PlantTask>().ToListAsync();
        }

        public static async Task UpdateTask(PlantTask plantTask)
        {
            await asyncDb.UpdateAsync(plantTask);

            var activityItems = await asyncDb.Table<PlantActivityItem>()
                .Where(act => act.PlantTaskFk == plantTask.Id)
                .ToListAsync();
            var activityForToday = activityItems
                .FirstOrDefault(act => act.Time.Date == DateTime.Today);

            //If today's activity is marked completed, we will remove it. Otherwise not.
            var firstDateToRemove = (activityForToday?.IsCompleted ?? false) ? DateTime.Today.AddDays(1) : DateTime.Today;

            await AddActivitiesFromTaskAsync(plantTask, firstDateToRemove);
        }

        public static async Task<List<PlantTask>> GetTasksOfPlantAsync(Plant plant)
        {
            var connections = await GetPlantTaskPlantConnectionsAsync(plant);
            var allTasks = await GetAllTasksAsync();

            var tasksOfPlant = allTasks
                .Where(t => connections.Any(conn => conn.PlantTaskFk == t.Id));

            return tasksOfPlant.ToList();
        }

        /// <summary>
        /// Adds activities to the local storage from the specified Task for the next 61 days, last day inclusive, today inclusive.
        /// Activities after firstDay will be deleted, even if they are marked as completed.
        /// </summary>
        /// <param name="task">The PlantTask to create the activities for.</param>
        /// <param name="firstDay">The first (possible) day to add an activity for.</param>
        public static async Task AddActivitiesFromTaskAsync(PlantTask task, DateTime firstDay)
        {
            //First, we need to delete all activities that are after 'firstDay'. (This can run on seperate thread - will be waited later on.)
            Task deleteTask = Task.Run(async() =>
            {
                await asyncDb.Table<PlantActivityItem>()
                    .Where(act => act.PlantTaskFk == task.Id && act.Time >= firstDay)
                    .DeleteAsync();
            });

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
                        (task.EveryXDays > 0 &&
                         day >= task.FirstOccurrenceDate.Date &&
                        (day - task.FirstOccurrenceDate.Date).Days % task.EveryXDays == 0) ||

                        //If it should occur every X month and this the Xth month's same day as it was for the first occurence.
                        (task.EveryXMonths > 0 &&
                           //If this day is after the first occurrence...
                           day >= task.FirstOccurrenceDate.Date &&
                           //If it should occur in this month...
                           ((day.Year - task.FirstOccurrenceDate.Date.Year) * 12 + day.Month - task.FirstOccurrenceDate.Date.Month) % task.EveryXMonths == 0 &&
                           //If the day is the same or this is the last day of the month and the day is more than the number of days in this month...
                           (day.Day == task.FirstOccurrenceDate.Date.Day || (IsLastDayOfMonth(day) && DateTime.DaysInMonth(day.Year, day.Month) < task.FirstOccurrenceDate.Day))))
                    {
                        AddActivityItemForDay(resultList, task.Id, day);
                    }
                }
                else
                {
                    if (day.Date == task.FirstOccurrenceDate.Date) AddActivityItemForDay(resultList, task.Id, day);
                }
            }

            await deleteTask;
            await asyncDb.InsertAllAsync(resultList);
            await RecreateDailyReminders();
        }

        /// <summary>
        /// Returns a List with all the Plants this PlantTask should be performed on.
        /// </summary>
        /// <param name="plantTask">The PlantTask to perform</param>
        /// <returns>The List of Plants</returns>
        public static List<Plant> GetPlantsOfTask(PlantTask plantTask)
        {
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

        public static PlantActivityItem GetNextIncompleteActivityOfTask(PlantTask plantTask)
        {
            var activitiesOfTask = Db.Table<PlantActivityItem>()
                .ToList()
                .Where(pai => pai.PlantTaskFk == plantTask.Id &&
                              pai.Time.Date >= DateTime.Today &&
                              !pai.IsCompleted)
                .FirstOrDefault();

            return activitiesOfTask;
        }

        /// <summary>
        /// Carries over all forgotten activities to today, given that today has no acitivity of the same task.
        /// </summary>
        /// <returns>A Task object representing the work to be done.</returns>
        public static async Task MoveForgottenTasksForward()
        {
            var allForgottenPastActivities = (await asyncDb.Table<PlantActivityItem>()
                .ToListAsync())
                .Where(pai => pai.Time.Date < DateTime.Now.Date && !pai.IsCompleted);

            await Task.WhenAll(allForgottenPastActivities
                .Select(async (activity) =>
                {
                    var sameActivityToday = await asyncDb.Table<PlantActivityItem>()
                        .Where(pai => pai.Time == DateTime.Now && pai.PlantTaskFk == activity.PlantTaskFk)
                        .FirstOrDefaultAsync();

                    if (sameActivityToday == null)
                    {
                        activity.Time = DateTime.Now;
                        await asyncDb.UpdateAsync(activity);
                    }
                }));
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
        /// Removes a given PlantActivityItem. Then recreates all daily reminders if the activity is not completed.
        /// </summary>
        /// <param name="activity">The acitivity to remove.</param>
        /// <returns></returns>
        public static async Task RemoveActivityAsync(PlantActivityItem activity)
        {
            await asyncDb.DeleteAsync(activity);

            if (!activity.IsCompleted)
                await RecreateDailyReminders();
        }

        /// <summary>
        /// Removes all activities for the specified task from the local storage.
        /// </summary>
        /// <param name="task">The PlantTask which's activities should be removed.</param>
        public static async Task RemoveActivitiesOfTaskAsync(PlantTask task)
        {
            await asyncDb.Table<PlantActivityItem>()
                .Where(act => act.PlantTaskFk == task.Id && act.Time >= DateTime.Now.Date)
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

        /// <summary>
        /// Modifies a single activity, asynchronously.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="shouldRecreateDailyReminders"></param>
        /// <returns></returns>
        public static async Task ModifyActivityAsync(PlantActivityItem activity, bool shouldRecreateDailyReminders = true)
        {
            var dbSavedInsanceOfActivity = await asyncDb.Table<PlantActivityItem>()
                .FirstAsync(pai => pai.Id == activity.Id);

            if (dbSavedInsanceOfActivity.Time.Date != activity.Time.Date) //Time changed
            {
                //Check if there is a similar activity on new day
                var identicalActivityOnNewDay = (await asyncDb.Table<PlantActivityItem>()
                    .ToListAsync())
                    .FirstOrDefault(pai => pai.Time.Date == activity.Time.Date && pai.PlantTaskFk == activity.PlantTaskFk);

                bool hasSameActivityOnNewDay = identicalActivityOnNewDay != null;

                if (hasSameActivityOnNewDay)
                    await asyncDb.DeleteAsync(activity);
                else
                    await asyncDb.UpdateAsync(activity);
            }
            else
            {
                await asyncDb.UpdateAsync(activity);
            }

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
                result[i] = activities.Where(act => act.Time.Date == from.AddDays(i).Date).ToList();
            }

            return result;
        }

        /// <summary>
        /// Gets (up to) 5 upcoming tasks for the plant, ordering earlier tasks higher.
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
                .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                              pai.Time.Date >= DateTime.Today &&
                              !pai.IsCompleted)
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

        public static PlantActivityItem GetLatestMissedActivityOfPlant(Plant p)
        {
            var taskConnectionsOfPlant = Db.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantFk == p.Id)
                .ToList();

            var tasksOfPlant = Db.Table<PlantTask>()
                .ToList()
                .Where(pt => taskConnectionsOfPlant.Any(tc => tc.PlantTaskFk == pt.Id))
                .ToList();

            var incompleteActivities = Db.Table<PlantActivityItem>()
                .ToList()
                .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                              pai.Time.Date < DateTime.Today &&
                              !pai.IsCompleted)
                .OrderByDescending(pai => pai.Time);

            return incompleteActivities.FirstOrDefault();
        }

        public static PlantActivityItem GetLatestCompletedActivityOfPlant(Plant p)
        {
            var taskConnectionsOfPlant = Db.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantFk == p.Id)
                .ToList();

            var tasksOfPlant = Db.Table<PlantTask>()
                .ToList()
                .Where(pt => taskConnectionsOfPlant.Any(tc => tc.PlantTaskFk == pt.Id))
                .ToList();

            var completedActivities = Db.Table<PlantActivityItem>()
                .ToList()
                .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                              pai.Time.Date < DateTime.Today &&
                              pai.IsCompleted)
                .OrderByDescending(pai => pai.Time)
                .ToList();

            return completedActivities.FirstOrDefault();
        }

        public static PlantActivityItem GetFirstCompletedActivityAfterLastMissedActivity(Plant p)
        {
            var taskConnectionsOfPlant = Db.Table<PlantTaskPlantConnection>()
                .Where(ptpc => ptpc.PlantFk == p.Id)
                .ToList();

            var tasksOfPlant = Db.Table<PlantTask>()
                .ToList()
                .Where(pt => taskConnectionsOfPlant.Any(tc => tc.PlantTaskFk == pt.Id))
                .ToList();

            var latestIncompleteActivityTime = Db.Table<PlantActivityItem>()
                .ToList()
                .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                              pai.Time.Date < DateTime.Today &&
                              !pai.IsCompleted)
                .OrderByDescending(pai => pai.Time)
                .FirstOrDefault()
                ?.Time.Date;

            if (latestIncompleteActivityTime == null)
            {
                var completedActivities = Db.Table<PlantActivityItem>()
                    .ToList()
                    .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                                  pai.Time.Date < DateTime.Today &&
                                  pai.IsCompleted)
                    .OrderBy(pai => pai.Time);

                return completedActivities.FirstOrDefault();
            }
            else
            {
                var completedActivitiesAfter = Db.Table<PlantActivityItem>()
                    .ToList()
                    .Where(pai => tasksOfPlant.Any(t => pai.PlantTaskFk == t.Id) &&
                                  pai.Time.Date < DateTime.Today &&
                                  pai.Time.Date > latestIncompleteActivityTime &&
                                  pai.IsCompleted)
                    .OrderBy(pai => pai.Time);

                return completedActivitiesAfter.FirstOrDefault();
            }
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
            PlantTask plantTask = await asyncDb
                .Table<PlantTask>()
                .FirstAsync(task => task.Id == activity.PlantTaskFk);

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

        #region HELPERS
        private static bool IsLastDayOfMonth(DateTime day)
        {
            return day.Date == new DateTime(day.Year, day.Month, 1).AddMonths(1).AddDays(-1);
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
