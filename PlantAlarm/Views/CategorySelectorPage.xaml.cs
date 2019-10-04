using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.ViewModels;
using PlantAlarm.Views.RootPages;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class CategorySelectorPage : SafeAreaRespectingPage
    {
        private CategorySelectorViewModel vm;
        private CategorySelectorPage()
        {
            InitializeComponent();
        }

        public static async Task<CategorySelectorPage> CreateAsync(List<PlantCategory> selectedPlantCategories)
        {
            var page = new CategorySelectorPage();
            page.BindingContext = await CategorySelectorViewModel.CreateAsync(page, selectedPlantCategories);
            page.vm = page.BindingContext as CategorySelectorViewModel;

            return page;
        }
    }
}
