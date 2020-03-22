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

        public static async Task RemovePlantCategoryAsync(PlantCategory categoryToRemove)
        {
            await asyncDb.DeleteAsync(categoryToRemove);
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
                    photo.Url = MediaService.AppendLocalAppDataFolderToPhotoName(photo.Url);
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

        public static PlantPhoto GetPrimaryPhotoOfPlant(Plant plant)
        {
            var primaryPhoto = Db.Table<PlantPhoto>()
                .Where(photo => photo.PlantFk == plant.Id)
                .CorrectUrlForAll();

            return primaryPhoto.FirstOrDefault();
        }

        public static async Task<PlantPhoto> GetPrimaryPhotoOfPlantAsync(Plant plant)
        {
            var primaryPhoto = (await asyncDb.Table<PlantPhoto>()
                .ToListAsync())
                .Where(photo => photo.PlantFk == plant.Id)
                .CorrectUrlForAll();

            return primaryPhoto.FirstOrDefault();
        }

        public static List<PlantPhoto> GetPhotosOfPlant(Plant plant)
        {
            var photos = Db.Table<PlantPhoto>()
                .ToList();

            var photosOfPlant = photos
                .Where(photo => photo.PlantFk == plant.Id)
                .CorrectUrlForAll(); //We do not store the constant part of the url.

            return photosOfPlant.ToList();
        }

        public static async Task<List<PlantPhoto>> GetPhotosOfPlantAsync(Plant plant, bool onlyPrimaryPhoto = false)
        {
            var photos = await asyncDb.Table<PlantPhoto>()
                .ToListAsync();

            var photosOfPlant = photos
                .Where(photo => onlyPrimaryPhoto ?
                    photo.PlantFk == plant.Id && photo.IsPrimary :
                    photo.PlantFk == plant.Id)
                .CorrectUrlForAll(); //We do not store the constant part of the url.

            return photosOfPlant.ToList();
        }

        public static List<PlantPhoto> GetPrimaryPhotosOfPlants(List<Plant> plants)
        {
            var photos = Db.Table<PlantPhoto>()
                .Where(pp => pp.IsPrimary)
                .ToList();

            var plantIds = plants
                .Select(p => p.Id)
                .ToList();

            var primaryPhotosOfPlants = photos
                .Where(photo => plantIds.Any(pId => pId == photo.PlantFk))
                .CorrectUrlForAll()
                .ToList();

            return primaryPhotosOfPlants;
        }

        private static IEnumerable<PlantPhoto> CorrectUrlForAll<T>(this T plantPhotoList) where T : IEnumerable<PlantPhoto>
        {
            var urlCorrectedPhotos = plantPhotoList
                .Select(photo =>
                {
                    photo.Url = MediaService.AppendLocalAppDataFolderToPhotoName(photo.Url);
                    return photo;
                });

            return urlCorrectedPhotos;
        }
    }

    public class PlantServiceException : Exception
    {
        public PlantServiceException(string message) : base(message) { }
    }


}
