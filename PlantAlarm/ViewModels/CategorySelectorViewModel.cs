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

        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;
        private readonly Page View;
        private readonly List<PlantCategory> plantCategoryList;

        private ObservableCollection<CategoryItem> selectedCategoryItems { get; set; }
        public ObservableCollection<CategoryItem> SelectedCategoryItems
        {
            get => selectedCategoryItems;
            set
            {
                selectedCategoryItems = value;
                OnPropertyChanged();
            }
        }

        private string filterString { get; set; }
        public string FilterString
        {
            get => filterString;
            set
            {
                filterString = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Categories));
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

                var filteredCategories = _categories
                    .Where(cat => cat.PlantCategory.Name.ToLower().Contains(FilterString?.ToLower() ?? string.Empty));

                return new ObservableCollection<CategoryItem>(filteredCategories);
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
        public ICommand DeleteCategoryCommand { get; private set; }
        public ICommand SelectionChangedCommand { get; private set; }
        public ICommand AddCategoriesCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        private CategorySelectorViewModel(Page view, List<PlantCategory> alreadySelectedCategories, List<PlantCategory> allCategories) 
        {
            AddCategoriesCommand = new Command(async () =>
            {
                var selectedCategories = categoryList
                    .Where(ci => ci.IsSelected)
                    .Select(ci => ci.PlantCategory)
                    .ToList();

                MessagingCenter.Send(this as object, "CategoriesSelected", selectedCategories);
                await Application.Current.MainPage.Navigation.PopAsync();
            });
            DeleteCategoryCommand = new Command<CategoryItem>(async (item) =>
            {
                await PlantService.RemovePlantCategoryAsync(item.PlantCategory);
            });
            SelectionChangedCommand = new Command((itemTapped) =>
            {
                var catItemTapped = itemTapped as CategoryItem;

                if (SelectedCategoryItems.IndexOf(catItemTapped) > -1)
                {
                    SelectedCategoryItems.Remove(catItemTapped);
                    catItemTapped.IsSelected = false;
                }
                else
                {
                    SelectedCategoryItems.Add(catItemTapped);
                    catItemTapped.IsSelected = true;
                }
            });
            AddCategoryModalCommand = new Command(async () =>
            {
                var categoryName = await DependencyService.Get<ITextInputModalProvider>().ShowTextModalAsync();
                if (string.IsNullOrEmpty(categoryName)) return;

                var plantCategory = new PlantCategory { Name = categoryName };
                try
                {
                    await PlantService.AddPlantCategoryAsync(plantCategory);

                    var categoryItem = new CategoryItem(this.SelectionChangedCommand, this.DeleteCategoryCommand)
                    {
                        PlantCategory = plantCategory
                    };

                    //Please check the unique getter and setter for this property and its private counterpart for the reason of this logic.
                    _categories.Add(categoryItem);
                    OnPropertyChanged(nameof(Categories)); 
                    categoryList.Add(categoryItem);
                }
                catch (PlantServiceException pse)
                {
                    await View.DisplayAlert("Error", pse.Message, "OK");
                    return;
                }
            });
            BackCommand = new Command(async () => await NavigationStack.PopAsync());

            View = view;
            plantCategoryList = allCategories;
            var categoryItemList = plantCategoryList
                .Select(pc => new CategoryItem(this.SelectionChangedCommand, this.DeleteCategoryCommand)
                {
                    PlantCategory = pc
                })
                .ToList();

            categoryList = categoryItemList;
            Categories = new ObservableCollection<CategoryItem>(categoryItemList);

            foreach (var selCat in alreadySelectedCategories)
            {
                var categoryItem = categoryList.First(ci => ci.PlantCategory.Id == selCat.Id);
                categoryItem.IsSelected = true;
            }

            var alreadySelectedCategoryItems = Categories.Where(ci => alreadySelectedCategories.FirstOrDefault(pc => pc.Id == ci.PlantCategory.Id) != null);

            SelectedCategoryItems = new ObservableCollection<CategoryItem>(alreadySelectedCategoryItems);
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

        public ICommand ItemTappedCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }

        public CategoryItem(ICommand itemTappedCommand, ICommand deleteCategoryCommand)
        {
            ItemTappedCommand = itemTappedCommand;
            RemoveCommand = deleteCategoryCommand;
        }
    }
}
