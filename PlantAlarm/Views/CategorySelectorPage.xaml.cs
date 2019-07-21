using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;
using PlantAlarm.DependencyServices;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class CategorySelectorPage : ContentPage
    {
        private readonly CategorySelectorViewModel vm;
        public CategorySelectorPage(List<PlantCategory> selectedPlantCategories)
        {
            InitializeComponent();

            BindingContext = new CategorySelectorViewModel(this, selectedPlantCategories);
            vm = BindingContext as CategorySelectorViewModel;
        }

        void Handle_Appearing(object sender, EventArgs e)
        {
            vm.AppearingCommand.Execute(null);
        }
    }
}
