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

namespace PlantAlarm.ViewModels
{
    public class CategorySelectorViewModel : INotifyPropertyChanged
    {
        private List<CategoryItem> categories { get; set; }
        public ObservableCollection<CategoryItem> Categories { get; private set; }

        //public Action<CategoryItem> ExpandCategoryItemCommand { get; private set; }
        public ICommand SearchCategoriesCommand { get; private set; }
        public ICommand AddCategoryCommand { get; private set; }
        public ICommand AppearingCommand { get; private set; }

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
            SearchCategoriesCommand = new Command((s) =>
            {
                string searchString = s as string;

                if (string.IsNullOrEmpty(searchString))
                {
                    Categories = new ObservableCollection<CategoryItem>(categories);
                }
                else
                {
                    var list = categories
                        .Where(c => c.PlantCategory.Name.Contains(searchString))
                        .ToList();

                    Categories = new ObservableCollection<CategoryItem>(categories);
                }
            });
            AddCategoryCommand = new Command(() =>
            {
                Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(this, "ShowCategoryAdderModal"));
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

                Categories = new ObservableCollection<CategoryItem>(categoryItemList);
            });

            //Subscribe to messages from view.
            MessagingCenter.Subscribe<object, string>(this, "AddCategoryFromModal", async(sender, categoryName) =>
            {
                var plantCategory = new PlantCategory { Name = categoryName };
                await PlantService.AddPlantCategoryAsync(plantCategory);

                var categoryItem = new CategoryItem { PlantCategory = plantCategory };
                Categories.Add(categoryItem);
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
    public class CategoryItem
    {
        public PlantCategory PlantCategory { get; set; }
        //public ObservableCollection<CategoryPlantItem> PlantsOfCategory { get; set; }
        //public bool IsExpanded { get; set; }
    }

    public class CategoryPlantItem
    {
        public Plant Plant { get; set; }
        public PlantPhoto PhotoOfPlant { get; set; }
    }
}
