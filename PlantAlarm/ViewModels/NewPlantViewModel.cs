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

        public Plant PlantToAdd { get; set; }

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

        public ObservableCollection<PlantPhotoItem> Photos { get; private set; }

        public NewPlantViewModel(INavigation navigation, Page viewForViewModel)
        {
            View = viewForViewModel;
            Navigation = navigation;

            ShowCategorySelectorPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(await CategorySelectorPage.CreateAsync(Categories));
            });
            AddPhotoCommand = new Command(async () =>
            {
                await InitPlantToAddIfNeeded();

                var image = await GetNewPhoto();
                if (image == null) return;

                //Save the created image to the local folder. Must be done, or it can be removed by the user from their public picture folder.
                var guidString = Guid.NewGuid().ToString().Replace("-", "");
                var fullUrl = Path.Combine(PlantService.LocalPhotoFolder, guidString);

                await image.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(File.Create(fullUrl));

                var photoItem = new PlantPhotoItem(
                    new PlantPhoto
                    {
                        PlantFk = PlantToAdd.Id,
                        IsPrimary = false,
                        TakenAt = DateTime.Now,
                        Url = fullUrl
                    },
                    this.ShowPhotoOptionsCommand);
                
                //Add the created object to the collection of photos.
                Photos.Add(photoItem);
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
            DeletePhotoCommand = new Command(async (ppi) =>
            {
                var plantPhotoItem = ppi as PlantPhotoItem;

                File.Delete(plantPhotoItem.Photo.Url);
                Photos.Remove(plantPhotoItem);
            });
            ChangePhotoCommand = new Command(async (ppi) =>
            {
                var image = await GetNewPhoto();
                if (image == null) return;

                var plantPhotoItem = ppi as PlantPhotoItem;
                var index = Photos.IndexOf(plantPhotoItem);

                File.Delete(plantPhotoItem.Photo.Url);
                await image.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(File.Create(plantPhotoItem.Photo.Url));

                plantPhotoItem.Photo.TakenAt = DateTime.Now;
                Photos.Remove(plantPhotoItem);
                Photos.Insert(index, plantPhotoItem);
            });

            MessagingCenter.Subscribe<object, List<PlantCategory>>(this, "CategoriesSelected", (viewModel, categoryList) =>
            {
                Categories = categoryList;
            });

            Categories = new List<PlantCategory>();
            Photos = new ObservableCollection<PlantPhotoItem>();
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

        private async Task InitPlantToAddIfNeeded()
        {
            if (PlantToAdd == null)
            {
                //Add the plant instantly, so that we will have an id for the plant photos.
                PlantToAdd = new Plant { Name = "Placeholder Name" };
                await PlantService.AddPlantAsync(PlantToAdd);
            }
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
