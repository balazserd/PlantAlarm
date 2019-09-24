using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using PlantAlarm.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class NewPlantViewModel : INotifyPropertyChanged
    {
        private INavigation Navigation { get; set; }
        private Page View { get; set; }

        private const string DefaultCategoriesMessage = "Tap to select categories";
        public string SelectedCategoriesMessage
        {
            get
            {
                return categories == null || categories.Count == 0 ?
                    DefaultCategoriesMessage :
                    string.Join(", ", categories.Select(c => c.Name));
            }
        }

        public string PlantName { get; set; }

        private List<PlantCategory> categories { get; set; }
        public List<PlantCategory> Categories
        {
            get => categories;
            set
            {
                categories = value;

                OnPropertyChanged(nameof(SelectedCategoriesMessage));
                OnPropertyChanged();
            }
        }

        public ICommand ShowCategorySelectorPageCommand { get; private set; }
        public ICommand AddPhotoCommand { get; set; }
        public ICommand ShowPhotoOptionsCommand { get; set; }
        public ICommand DeletePhotoCommand { get; set; }
        public ICommand ChangePhotoCommand { get; set; }
        public ICommand AddPlantCommand { get; set; }

        //This is a backing store, without absolute path to the photos.
        private List<PlantPhoto> photosToAdd { get; set; }
        public ObservableCollection<PlantPhotoItem> Photos { get; set; }

        public NewPlantViewModel(INavigation navigation, Page viewForViewModel)
        {
            View = viewForViewModel;
            Navigation = navigation;

            Categories = new List<PlantCategory>();
            Photos = new ObservableCollection<PlantPhotoItem>();
            photosToAdd = new List<PlantPhoto>();

            ShowCategorySelectorPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(await CategorySelectorPage.CreateAsync(Categories));
            });
            AddPhotoCommand = new Command(async () =>
            {
                var image = await GetNewPhoto();
                if (image == null) return;

                //Save the created image to the local folder. Must be done, or it can be removed by the user from their public picture folder.
                var photoName = Guid.NewGuid().ToString().Replace("-", "");

                await image
                .GetStreamWithImageRotatedForExternalStorage()
                .CopyToAsync(
                    File.Create(
                        PlantService.AppendLocalAppDataFolderToPhotoName(photoName)));

                //Goes to backing store.
                var plantPhoto_noFullUrl = new PlantPhoto
                {
                    IsPrimary = false,
                    TakenAt = DateTime.Now,
                    Url = photoName
                };
                photosToAdd.Add(plantPhoto_noFullUrl);

                //Goes to ObservableCollection.
                var plantPhoto_fullUrl = new PlantPhoto
                {
                    IsPrimary = false,
                    TakenAt = DateTime.Now,
                    Url = PlantService.AppendLocalAppDataFolderToPhotoName(photoName)
                };

                var photoItem = new PlantPhotoItem(
                    plantPhoto_fullUrl,
                    this.ShowPhotoOptionsCommand);

                //Add the created object to the collection of photos.
                Device.BeginInvokeOnMainThread(() =>
                {
                    Photos.Add(photoItem);
                });
            });
            ShowPhotoOptionsCommand = new Command(async (ppi) =>
            {
                string action = await View.DisplayActionSheet("Select an option", "Cancel", "Delete photo", "Change photo");

                var plantPhotoItem = ppi as PlantPhotoItem;
                switch (action)
                {
                    case "Delete photo":
                        DeletePhotoCommand.Execute(plantPhotoItem);
                        break;
                    case "Change photo":
                        ChangePhotoCommand.Execute(plantPhotoItem);
                        break;
                    case "Cancel":
                        break;
                    default: throw new InvalidOperationException("Unknown action chosen in ActionSheet.");
                }
            });
            DeletePhotoCommand = new Command((ppi) =>
            {
                var plantPhotoItem = ppi as PlantPhotoItem;

                File.Delete(plantPhotoItem.Photo.Url);

                //Remove from BOTH backing store and observed store.
                Photos.Remove(plantPhotoItem);
                photosToAdd.Remove(
                    photosToAdd.First(
                        ph => plantPhotoItem.Photo.Url.Contains(ph.Url)));
            });
            ChangePhotoCommand = new Command(async (ppi) =>
            {
                var image = await GetNewPhoto();
                if (image == null) return;

                var plantPhotoItem = ppi as PlantPhotoItem;
                var index = Photos.IndexOf(plantPhotoItem);

                File.Delete(plantPhotoItem.Photo.Url);
                await image.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(File.Create(plantPhotoItem.Photo.Url));

                //No need to re-add for backing store, as this is a step only to visually represent the change.
                plantPhotoItem.Photo.TakenAt = DateTime.Now;
                Photos.Remove(plantPhotoItem);
                Photos.Insert(index, plantPhotoItem);
            });
            AddPlantCommand = new Command(async() =>
            {
                Plant plantToAdd = new Plant
                {
                    CreatedAt = DateTime.Now,
                    Name = PlantName
                };
                await PlantService.AddPlantAsync(plantToAdd);

                if (Photos.Count > 0) photosToAdd[0].IsPrimary = true;
                var photosOfPlant = photosToAdd.Select(plantPhoto =>
                {
                    plantPhoto.PlantFk = plantToAdd.Id;
                    return plantPhoto;
                });

                await PlantService.AddPlantPhotosAsync(photosOfPlant);

                MessagingCenter.Send(this as object, "PlantAdded");
                await Navigation.PopAsync();
            });

            MessagingCenter.Subscribe<object, List<PlantCategory>>(this, "CategoriesSelected", (viewModel, categoryList) =>
            {
                Categories = categoryList;
            });
        }

        private async Task<MediaFile> GetNewPhoto()
        {
            //Create the actions for the add photo dialog.
            var actions = new List<string>();

            if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) actions.Add("Take photo");
            if (CrossMedia.Current.IsPickPhotoSupported) actions.Add("Pick from device");

            string actionToTake = await View.DisplayActionSheet("Select an option", "Back", null, actions.ToArray());

            MediaFile image;
            switch (actionToTake)
            {
                case "Take photo":
                    {
                        image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                        {
                            PhotoSize = PhotoSize.Medium,
                            CompressionQuality = 75,
                            SaveToAlbum = true
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PlantPhotoItem
    {
        public PlantPhoto Photo { get; set; }
        public ICommand ShowOptions { get; set; }

        public PlantPhotoItem(PlantPhoto plantPhoto, ICommand ShowOptionsCommand)
        {
            Photo = plantPhoto;
            ShowOptions = new Command(() =>
            {
                ShowOptionsCommand.Execute(this);
            });
        }
    }
}
