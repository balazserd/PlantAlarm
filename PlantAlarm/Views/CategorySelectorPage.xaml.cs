using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class CategorySelectorPage : ContentPage
    {
        private readonly CategorySelectorViewModel vm;
        public CategorySelectorPage()
        {
            InitializeComponent();

            BindingContext = new CategorySelectorViewModel();
            vm = BindingContext as CategorySelectorViewModel;
        }

        void CategoryTapped(object sender, ItemTappedEventArgs e)
        {
            var categoryItem = e.Item as CategoryItem;
            vm.ExpandCategoryItemCommand(categoryItem);
        }
    }
}
