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
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;
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
                Color.FromHex("#D3E5B7") :
                Color.White;
        }

        public bool HasPhoto
        {
            get => PhotoToAdd != null;
        }

        public string PhotoOptionsText
        {
            get => PhotoToAdd == null ?
                "Upload / Take" :
                "Change / Delete";
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
                OnPropertyChanged(nameof(PhotoOptionsText));
                OnPropertyChanged();
            }
        }

        public ICommand ShowCategorySelectorPageCommand { get; private set; }
        public ICommand AddPhotoCommand { get; set; }
        public ICommand ShowPhotoOptionsCommand { get; set; }
        public ICommand DeletePhotoCommand { get; set; }
        public ICommand ChangePhotoCommand { get; set; }
        public ICommand AddPlantCommand { get; set; }
        public ICommand BackCommand { get; set; }

        public NewPlantViewModel(Page viewForViewModel)
        {
            View = viewForViewModel;

            Categories = new List<PlantCategory>();

            ShowCategorySelectorPageCommand = new Command(async () =>
            {
                await NavigationStack.PushAsync(await CategorySelectorPage.CreateAsync(Categories));
            });
            AddPhotoCommand = new Command(async () =>
            {
                if (PhotoToAdd != null)
                {
                    ShowPhotoOptionsCommand.Execute(null);
                }
                else
                {
                    var image = await viewForViewModel.GetNewPhoto();
                    if (image == null) return;

                    var photoName = await MediaService.SavePhotoToLocalFolder(image);

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
                        Url = MediaService.AppendLocalAppDataFolderToPhotoName(photoName)
                    };
                    PhotoToAdd = plantPhoto_fullUrl;
                }
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
                var image = await View.GetNewPhoto();
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
                await NavigationStack.PopAsync();
            });
            BackCommand = new Command(async () => await NavigationStack.PopAsync());

            MessagingCenter.Subscribe<object, List<PlantCategory>>(this, "CategoriesSelected", (viewModel, categoryList) =>
            {
                Categories = categoryList;
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
