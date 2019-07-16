using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using SQLite;

namespace PlantAlarm.Services
{
    public static class PlantService
    {
        private static readonly SQLiteAsyncConnection db = Application.LocalDbConnection.Db;

        public readonly static string LocalPhotoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");

        public static async Task<int> AddPlantAsync(Plant newPlant)
        {
            return await db.InsertAsync(newPlant);
        }

        public static async Task AddPlantCategoryAsync(PlantCategory newPlantCategory)
        {
            await db.InsertAsync(newPlantCategory);
        }

        public static async Task AddPlantPhotoAsync(PlantPhoto photo)
        {
            await db.InsertAsync(photo);
        }

        public static async Task<List<Plant>> GetPlantsAsync()
        {
            var plantList = await db.Table<Plant>().ToListAsync();
            return plantList;
        }

        public static async Task<List<PlantCategory>> GetPlantCategoriesAsync()
        {
            var plantCategoryList = await db.Table<PlantCategory>().ToListAsync();
            return plantCategoryList;
        }

        public static async Task<List<PlantPhoto>> GetPhotosOfPlantAsync(Plant p)
        {
            var photos = await db.Table<PlantPhoto>()
                .Where(photo => photo.PlantFk == p.Id)
                .ToListAsync();

            //We do not store the constant part of the url.
            var urlCorrectedPhotos = photos
                .Select(photo =>
                {
                    photo.Url = Path.Combine(LocalPhotoFolder, photo.Url);
                    return photo;
                })
                .ToList();

            return urlCorrectedPhotos;
        }
    }
}
