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
        private readonly Plant PlantToEdit;

        private Page View { get; set; }

        public string PlantName { get; set; }

        private bool isEditingMode { get; set; }
        public bool IsEditingMode
        {
            get => isEditingMode;
            set
            {
                isEditingMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CommitButtonText));
            }
        }

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

        //public ScrollOrientation AllowedScrollOrientations
        //{
        //    get => HasPhoto ? ScrollOrientation.Vertical : ScrollOrientation.Neither;
        //}

        public bool HasPhoto { get => PhotoToAdd != null; }
        public bool HasNoPhoto { get => !HasPhoto; }

        public string CommitButtonText { get => IsEditingMode ? "Save" : "Add"; }

        public string Title { get => IsEditingMode ? PlantToEdit.Name : "New Plant"; }

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
                OnPropertyChanged(nameof(HasNoPhoto));
                //OnPropertyChanged(nameof(AllowedScrollOrientations));
                OnPropertyChanged(nameof(PhotoOptionsText));
                OnPropertyChanged();
            }
        }

        public ICommand ShowCategorySelectorPageCommand { get; private set; }
        public ICommand AddPhotoCommand { get; private set; }
        public ICommand ShowPhotoOptionsCommand { get; private set; }
        public ICommand DeletePhotoCommand { get; private set; }
        public ICommand ChangePhotoCommand { get; private set; }
        public ICommand CommitPlantCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand DeletePlantCommand { get; private set; }

        public NewPlantViewModel(Page viewForViewModel, bool isEditing = false, Plant plantToEdit = null)
        {
            this.View = viewForViewModel;
            this.IsEditingMode = isEditing;
            this.PlantToEdit = plantToEdit;

            if (this.IsEditingMode)
            {
                this.FillFormWithExistingPlant();
            }

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
            CommitPlantCommand = new Command(async() =>
            {
                if (this.IsEditingMode)
                {
                    this.PlantToEdit.Name = this.PlantName;
                    await PlantService.ModifyPlantAsync(this.PlantToEdit);

                    MessagingCenter.Send(this as object, "PlantChanged");
                }
                else
                {
                    Plant plantToAdd = new Plant
                    {
                        CreatedAt = DateTime.Now,
                        Name = PlantName ?? ""
                    };
                    await PlantService.AddPlantAsync(plantToAdd);

                    if (_truncatedUrlPlantPhoto != null)
                    {
                        _truncatedUrlPlantPhoto.IsPrimary = true;
                        _truncatedUrlPlantPhoto.PlantFk = plantToAdd.Id;

                        await PlantService.AddPlantPhotosAsync(new List<PlantPhoto> { _truncatedUrlPlantPhoto });
                    }

                    MessagingCenter.Send(this as object, "PlantAdded");
                }

                await NavigationStack.PopAsync();
            });
            BackCommand = new Command(async () => await NavigationStack.PopAsync());
            DeletePlantCommand = new Command(async () =>
            {
                switch (await this.View.DisplayActionSheet($"Deleting a plant will remove all photos related to it and also modify all tasks that were to be performed on it.\n\nThis action cannot be undone. Are you sure you want to delete {this.PlantToEdit.Name}?", "Cancel", "Delete"))
                {
                    case "Delete": break;
                    default: return;
                }

                switch (await this.View.DisplayActionSheet($"Please confirm again that you want to delete the plant \"{this.PlantToEdit.Name}\" and all photos related to it.", "Cancel", "Delete permanently"))
                {
                    case "Delete permanently": break;
                    default: return;
                }

                await PlantService.DeletePlantAsync(this.PlantToEdit);
                MessagingCenter.Send(this as object, "PlantDeleted");

                await this.NavigationStack.PopToRootAsync();
            });

            MessagingCenter.Subscribe<object, List<PlantCategory>>(this, "CategoriesSelected", (viewModel, categoryList) =>
            {
                Categories = categoryList;
            });
        }

        private void FillFormWithExistingPlant()
        {
            this.PlantName = this.PlantToEdit.Name;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
