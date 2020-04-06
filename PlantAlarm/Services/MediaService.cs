using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PlantAlarm.DependencyServices;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    public enum PhotoSourceOptions
    {
        OnlyCamera = 1,
        OnlySavedPhotos,
        Both
    }

    public static class MediaService
    {
        public const string kIsSavingPhotosToPhotoLibrary = "kIsSavingPhotosToPhotoLibrary";

        public static string LocalPhotoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantPhotos");
        public static string AppendLocalAppDataFolderToPhotoName(string photoName) => Path.Combine(LocalPhotoFolder, photoName);
        public static IMediaManagementService MediaManagementService = DependencyService.Get<IMediaManagementService>();

        /// <summary>
        /// Saves a photo to the local folder, then returns its name. You can use AppendLocalAppDataFolderToPhotoName to create a full path.
        /// </summary>
        /// <param name="photo">The photo to save.</param>
        /// <returns>The name of the saved photo.</returns>
        public static async Task<string> SavePhotoToLocalFolder(MediaFile photo)
        {
            //Save the created image to the local folder. Must be done, or it can be removed by the user from their public picture folder.
            var photoName = Guid.NewGuid().ToString().Replace("-", "");

            await photo
            .GetStreamWithImageRotatedForExternalStorage()
            .CopyToAsync(
                File.Create(
                    AppendLocalAppDataFolderToPhotoName(photoName)));

            return photoName;
        }

        public static void RemovePhotoFromLocalFolder(string url)
        {
            if (!File.Exists(url)) return;

            File.Delete(url);
        }

        public static void SavePhotoToCameraRoll(MediaFile file) => MediaManagementService.SavePhotoToCameraRoll(file);

        public static void RequestPermissionToPhotoLibraries() => MediaManagementService.RequestAccessToCameraRoll();

        public static bool IsPermittedToAccessPhotoLibraries() => MediaManagementService.IsSavingPhotosToCameraRollPermitted();

        public static void ShowExplanatoryTextAfterDenyingPhotoLibraryRequest() => MediaManagementService.ShowExplanatoryTextForDenyingPhotoAlbumRequest();

        public static bool IsSavingPhotosToPhotoLibrary() {
            if (Application.Current.Properties.TryGetValue(kIsSavingPhotosToPhotoLibrary, out object _isSavingPhotos))
            {
                return (bool)_isSavingPhotos;
            }
            return false;
        }

        /// <summary>
        /// Presents a flow that will result in getting an image.
        /// </summary>
        /// <typeparam name="P">A type that is or the subclass of the Page class.</typeparam>
        /// <param name="ViewToPresentActionSheetOn">The page to present the flow on top of.</param>
        /// <param name="photoSourceOptions">The options to allow. See PhotoSourceOptions Enum for more info.</param>
        /// <returns></returns>
        public static async Task<MediaFile> GetNewPhoto<P>(this P ViewToPresentActionSheetOn, PhotoSourceOptions photoSourceOptions = PhotoSourceOptions.Both)
            where P : Page
        {
            //Create the actions for the add photo dialog.
            var actions = new List<string>();

            if (photoSourceOptions != PhotoSourceOptions.OnlySavedPhotos &&
                CrossMedia.Current.IsCameraAvailable &&
                CrossMedia.Current.IsTakePhotoSupported)
            {
                actions.Add("Take photo");
            }

            if (photoSourceOptions != PhotoSourceOptions.OnlyCamera &&
                CrossMedia.Current.IsPickPhotoSupported)
            {
                actions.Add("Pick from device");
            } 

            string actionToTake = await ViewToPresentActionSheetOn.DisplayActionSheet("Select an option", "Back", null, actions.ToArray());

            MediaFile image;
            switch (actionToTake)
            {
                case "Take photo":
                    {
                        image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                        {
                            PhotoSize = PhotoSize.Medium,
                            CompressionQuality = 75,
                            SaveToAlbum = IsSavingPhotosToPhotoLibrary()
                        });
                        break;
                    }
                case "Pick from device":
                    {
                        image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                        {
                            PhotoSize = PhotoSize.Medium,
                            CompressionQuality = 75,
                        });
                        break;
                    }
                case "Back": return null;
                default: throw new InvalidOperationException("Unknown action chosen in ActionSheet.");
            }

            return image;
        }
    }
}
