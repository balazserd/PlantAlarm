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
        private static readonly SQLiteAsyncConnection asyncDb = App.LocalDbConnection.AsyncDb;
        private static readonly SQLiteConnection Db = App.LocalDbConnection.Db;

        public readonly static string LocalPhotoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");
        public static string AppendLocalAppDataFolderToPhotoName(string photoName) => Path.Combine(LocalPhotoFolder, photoName);

        public static async Task AddPlantAsync(Plant newPlant)
        {
            await asyncDb.InsertAsync(newPlant);
        }

        public static async Task AddPlantCategoryAsync(PlantCategory newPlantCategory)
        {
            try
            {
                await asyncDb.InsertAsync(newPlantCategory);
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

            await asyncDb.InsertAllAsync(itemsToAdd);
        }

        public static async Task AddPlantPhotoAsync(PlantPhoto photo)
        {
            await asyncDb.InsertAsync(photo);
        }

        public static async Task AddPlantPhotosAsync(IEnumerable<PlantPhoto> photos)
        {
            int inserted = await asyncDb.InsertAllAsync(photos);
            if (inserted != photos.Count()) throw new PlantServiceException("Not all photos could be added.");
        }

        public static async Task<List<Plant>> GetPlantsAsync()
        {
            var plantList = await asyncDb.Table<Plant>().ToListAsync();
            return plantList;
        }

        public static List<Plant> GetPlants()
        {
            var plantList = Db.Table<Plant>().ToList();
            return plantList;
        }

        public static List<PlantPhoto> GetAllPhotos()
        {
            var photoList = Db.Table<PlantPhoto>().ToList();

            //correcting the url (only the image name is stored in the db).
            photoList = photoList
                .Select(photo =>
                {
                    photo.Url = AppendLocalAppDataFolderToPhotoName(photo.Url);
                    return photo;
                })
                .ToList();

            return photoList;
        }

        public static async Task<List<PlantCategory>> GetPlantCategoriesAsync()
        {
            var plantCategoryList = await asyncDb.Table<PlantCategory>().ToListAsync();
            return plantCategoryList;
        }

        public static async Task<List<PlantPhoto>> GetPhotosOfPlantAsync(Plant p, bool onlyPrimaryPhoto = false)
        {
            var photos = await asyncDb.Table<PlantPhoto>()
                .ToListAsync();

            var photosOfPlant = photos
                .Where(photo => onlyPrimaryPhoto ?
                    photo.PlantFk == p.Id && photo.IsPrimary :
                    photo.PlantFk == p.Id);

            //We do not store the constant part of the url.
            var urlCorrectedPhotos = photosOfPlant
                .Select(photo =>
                {
                    photo.Url = AppendLocalAppDataFolderToPhotoName(photo.Url);
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
