using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.Helpers;
using PlantAlarm.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace PlantAlarm.ViewModels
{
    public class CategorySelectorViewModel : INotifyPropertyChanged
    {
        private List<CategoryItem> categoryList { get; set; }
        private readonly Page View;
        private readonly List<PlantCategory> plantCategoryList;

        private ObservableCollection<object> selectedCategoryItems { get; set; }
        public ObservableCollection<object> SelectedCategoryItems
        {
            get => selectedCategoryItems;
            set
            {
                selectedCategoryItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CategoryItem> _categories { get; set; }
        public ObservableCollection<CategoryItem> Categories
        {
            get
            {
                if (_categories == null) return null;

                _categories.Sort(new Comparison<CategoryItem>((a, b) =>
                {
                    return string.Compare(a.PlantCategory.Name, b.PlantCategory.Name, StringComparison.Ordinal);
                }));

                return _categories;
            }  
            private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCategoriesCommand { get; private set; }
        public ICommand AddCategoryModalCommand { get; private set; }
        public ICommand AppearingCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand AddCategoriesCommand { get; set; }

        private CategorySelectorViewModel(Page view, List<PlantCategory> alreadySelectedCategories, List<PlantCategory> allCategories) 
        {
            View = view;
            plantCategoryList = allCategories;
            var categoryItemList = plantCategoryList.Select(pc => new CategoryItem { PlantCategory = pc }).ToList();

            categoryList = categoryItemList;
            Categories = new ObservableCollection<CategoryItem>(categoryItemList);

            foreach (var selCat in alreadySelectedCategories)
            {
                var categoryItem = categoryList.First(ci => ci.PlantCategory.Id == selCat.Id);
                categoryItem.IsSelected = true;
            }

            var alreadySelectedCategoryItems = Categories.Where(ci => alreadySelectedCategories.FirstOrDefault(pc => pc.Id == ci.PlantCategory.Id) != null).Cast<object>();

            SelectedCategoryItems = new ObservableCollection<object>(alreadySelectedCategoryItems);

            AddCategoriesCommand = new Command(async () =>
            {
                var selectedCategories = categoryList
                    .Where(ci => ci.IsSelected)
                    .Select(ci => ci.PlantCategory)
                    .ToList();

                MessagingCenter.Send(this as object, "CategoriesSelected", selectedCategories);
                await Application.Current.MainPage.Navigation.PopAsync();
            });
            AddCategoryModalCommand = new Command(async () =>
            {
                var categoryName = await DependencyService.Get<ITextInputModalProvider>().ShowTextModalAsync();
                if (string.IsNullOrEmpty(categoryName)) return;

                var plantCategory = new PlantCategory { Name = categoryName };
                try
                {
                    await PlantService.AddPlantCategoryAsync(plantCategory);

                    var categoryItem = new CategoryItem { PlantCategory = plantCategory };

                    Categories.Add(categoryItem);
                    categoryList.Add(categoryItem);
                }
                catch (PlantServiceException pse)
                {
                    await View.DisplayAlert("Error", pse.Message, "OK");
                    return;
                }
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
            });
        }

        public static async Task<CategorySelectorViewModel> CreateAsync(Page view, List<PlantCategory> alreadySelected)
        {
            var allCategories = await PlantService.GetPlantCategoriesAsync();
            var vm = new CategorySelectorViewModel(view, alreadySelected, allCategories);

            return vm;
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
    }
}
