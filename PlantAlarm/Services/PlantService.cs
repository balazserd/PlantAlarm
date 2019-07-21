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
        private static readonly SQLiteAsyncConnection db = App.LocalDbConnection.Db;

        public readonly static string LocalPhotoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");

        public static async Task AddPlantAsync(Plant newPlant)
        {
            await db.InsertAsync(newPlant);
        }

        public static async Task AddPlantCategoryAsync(PlantCategory newPlantCategory)
        {
            try
            {
                await db.InsertAsync(newPlantCategory);
            }
            catch (SQLiteException e)
            {
                if (e.Message == "Constraint")
                {
                    throw new PlantServiceException("A category with the given name already exists.");
                }
            }
        }

        public static async Task AddPlantCategoryConnectionsAsync(List<PlantCategory> categoryList, Plant plant)
        {
            List<PlantCategorization> itemsToAdd = categoryList
                .Select(category => new PlantCategorization
                {
                    PlantFk = plant.Id,
                    PlantCategoryFk = category.Id
                })
                .ToList();

            await db.InsertAllAsync(itemsToAdd);
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

    public class PlantServiceException : Exception
    {
        public PlantServiceException(string message) : base(message) { }
    }
}
