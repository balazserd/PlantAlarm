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
        private readonly INavigation Navigation = Application.Current.MainPage.Navigation;
        private Page View { get; set; }

        public string PlantName { get; set; }

        private const string DefaultCategoriesMessage = "Tap to select categories";
        public string SelectedCategoriesMessage
        {
            get
            {
                OnPropertyChanged(nameof(CategoriesTextColor));
                return categories == null || categories.Count == 0 ?
                    DefaultCategoriesMessage :
                    string.Join(", ", categories.Select(c => c.Name));
            }
        }

        public Color CategoriesTextColor
        {
            get => categories == null || categories.Count == 0 ?
                Color.Gray :
                Color.Default;
        }

        public bool HasPhoto
        {
            get => PhotoToAdd != null;
        }

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

        //This is a backing store, without absolute path to the photos.
        private PlantPhoto _truncatedUrlPlantPhoto { get; set; }
        private PlantPhoto photoToAdd { get; set; }
        public PlantPhoto PhotoToAdd
        {
            get => photoToAdd;
            set
            {
                photoToAdd = value;
                OnPropertyChanged(nameof(HasPhoto));
                OnPropertyChanged();
            }
        }

        public ICommand ShowCategorySelectorPageCommand { get; private set; }
        public ICommand AddPhotoCommand { get; set; }
        public ICommand ShowPhotoOptionsCommand { get; set; }
        public ICommand DeletePhotoCommand { get; set; }
        public ICommand ChangePhotoCommand { get; set; }
        public ICommand AddPlantCommand { get; set; }

        public NewPlantViewModel(Page viewForViewModel)
        {
            View = viewForViewModel;

            Categories = new List<PlantCategory>();

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
                _truncatedUrlPlantPhoto = plantPhoto_noFullUrl;

                //Goes to full item.
                var plantPhoto_fullUrl = new PlantPhoto
                {
                    IsPrimary = false,
                    TakenAt = DateTime.Now,
                    Url = PlantService.AppendLocalAppDataFolderToPhotoName(photoName)
                };
                PhotoToAdd = plantPhoto_fullUrl;
            });
            ShowPhotoOptionsCommand = new Command(async () =>
            {
                string action = await View.DisplayActionSheet("Select an option", "Cancel", "Delete photo", "Change photo");

                switch (action)
                {
                    case "Delete photo":
                        DeletePhotoCommand.Execute(PhotoToAdd);
                        break;
                    case "Change photo":
                        ChangePhotoCommand.Execute(PhotoToAdd);
                        break;
                    case "Cancel":
                        break;
                    default: throw new InvalidOperationException("Unknown action chosen in ActionSheet.");
                }
            });
            DeletePhotoCommand = new Command((pp) =>
            {
                var plantPhoto = pp as PlantPhoto;

                File.Delete(plantPhoto.Url);

                //Remove from BOTH backing store and observed store.
                _truncatedUrlPlantPhoto = null;
                PhotoToAdd = null;
            });
            ChangePhotoCommand = new Command(async (pp) =>
            {
                var image = await GetNewPhoto();
                if (image == null) return;

                var plantPhoto = pp as PlantPhoto;

                File.Delete(plantPhoto.Url);
                await image.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(File.Create(plantPhoto.Url));

                //No need to re-add for backing store, as this is a step only to visually represent the change.
                plantPhoto.TakenAt = DateTime.Now;
                PhotoToAdd = plantPhoto;
            });
            AddPlantCommand = new Command(async() =>
            {
                Plant plantToAdd = new Plant
                {
                    CreatedAt = DateTime.Now,
                    Name = PlantName
                };
                await PlantService.AddPlantAsync(plantToAdd);

                _truncatedUrlPlantPhoto.IsPrimary = true;
                _truncatedUrlPlantPhoto.PlantFk = plantToAdd.Id;

                await PlantService.AddPlantPhotosAsync(new List<PlantPhoto>{ _truncatedUrlPlantPhoto });

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
}
