using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace PlantAlarm.ViewModels
{
    public class CategorySelectorViewModel : INotifyPropertyChanged
    {
        private List<CategoryItem> categoryList { get; set; }

        private ObservableCollection<CategoryItem> _categories { get; set; }
        public ObservableCollection<CategoryItem> Categories
        {
            get => _categories;
            private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        //public Action<CategoryItem> ExpandCategoryItemCommand { get; private set; }
        public ICommand SearchCategoriesCommand { get; private set; }
        public ICommand AddCategoryCommand { get; private set; }
        public ICommand AppearingCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }

        public CategorySelectorViewModel() 
        {
            //Initing the Commands.

            //IF WE WANT TO IMPLEMENT IN THE FUTURE THAT TAPPING ON CATEGORY SHOWS ITS LIST OF PLANTS
            //ExpandCategoryItemCommand = new Action<CategoryItem>(ci =>
            //{
            //    var index = Categories.IndexOf(ci);
            //    Categories.Remove(ci);

            //    ci.IsExpanded = !ci.IsExpanded;
            //    Categories.Insert(index, ci);
            //});
            //SEARCH FUNCTIONALITY
            //SearchCategoriesCommand = new Command((s) =>
            //{
            //    string searchString = s as string;

            //    if (string.IsNullOrEmpty(searchString))
            //    {
            //        Categories = new ObservableCollection<CategoryItem>(categoryList);
            //    }
            //    else
            //    {
            //        var list = categoryList
            //            .Where(c => c.PlantCategory.Name.Contains(searchString))
            //            .ToList();

            //        Categories = new ObservableCollection<CategoryItem>(categoryList);
            //    }
            //});
            AddCategoryCommand = new Command(() =>
            {
                MessagingCenter.Send((object)this, "ShowCategoryAdderModal");
            });
            AppearingCommand = new Command(async () =>
            {
                //Building the Item Sets.
                var plantCategoryList = await PlantService.GetPlantCategoriesAsync();

                //IF WE WANT TO IMPLEMENT IN THE FUTURE THAT TAPPING ON CATEGORY SHOWS ITS LIST OF PLANTS
                //var plantList = PlantService.GetPlantsAsync().Result;

                //categories = plantCategoryList
                //    .Select(pc => {
                //        var plantsOfThisCategory = plantList
                //            .Where(p => p.PlantCategoryFk == pc.Id)
                //            .Select(p => new CategoryPlantItem
                //            {
                //                Plant = p,
                //                PhotoOfPlant = PlantService.GetPhotosOfPlantAsync(p)
                //                    .Result
                //                    .FirstOrDefault(photo => photo.IsPrimary)
                //            });
                //        return new CategoryItem
                //        {
                //            PlantCategory = pc,
                //            PlantsOfCategory = new ObservableCollection<CategoryPlantItem>(plantsOfThisCategory.ToList())
                //        };
                //    })
                //    .ToList();

                var categoryItemList = plantCategoryList.Select(pc => new CategoryItem { PlantCategory = pc }).ToList();

                categoryList = categoryItemList;
                Categories = new ObservableCollection<CategoryItem>(categoryItemList);
            });
            SelectionChangedCommand = new Command((itemsSelected) =>
            {
                var selectedCategories = (itemsSelected as IList<object>).ToList().Cast<CategoryItem>();
                var selectedIdsList = selectedCategories.Select(ci => ci.PlantCategory.Id).ToList();

                foreach (var categoryItem in categoryList)
                {
                    //If we can find the current categoryItem's Id in the list of selected Id-s, then it is currently selected.
                    if (selectedIdsList.FirstOrDefault(selId => selId == categoryItem.PlantCategory.Id) != default(int))
                    {
                        categoryItem.IsSelected = true;
                    }
                    else
                    {
                        categoryItem.IsSelected = false;
                    }
                }

                OnPropertyChanged(nameof(Categories));
            });

            //Subscribe to messages from view.
            MessagingCenter.Subscribe<object, string>(this, "AddCategoryFromModal", async(sender, categoryName) =>
            {
                var plantCategory = new PlantCategory { Name = categoryName };
                try
                {
                    int Id = await PlantService.AddPlantCategoryAsync(plantCategory);
                    plantCategory.Id = Id;

                    var categoryItem = new CategoryItem { PlantCategory = plantCategory };

                    Categories.Add(categoryItem);
                    categoryList.Add(categoryItem);
                }
                catch (PlantServiceException pse)
                {
                    MessagingCenter.Send(this as object, "AddCategoryFailed", pse.Message);
                    return;
                } 
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    //WARNING
    //The classes here only exist to serve this viewmodel. They should never be reused.
    public class CategoryItem : BindableObject
    {
        public PlantCategory PlantCategory { get; set; }

        private bool isSelected { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
        //public ObservableCollection<CategoryPlantItem> PlantsOfCategory { get; set; }
        //public bool IsExpanded { get; set; }
    }

    public class CategoryPlantItem
    {
        public Plant Plant { get; set; }
        public PlantPhoto PhotoOfPlant { get; set; }
    }
}
