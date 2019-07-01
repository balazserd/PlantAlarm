using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using SQLite;

namespace PlantAlarm.Services
{
    public static class PlantService
    {
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        public static async Task<List<Plant>> GetPlantsAsync()
        {
            var plantList = await db.Table<Plant>().ToListAsync();
            return plantList;
        }
    }
}
