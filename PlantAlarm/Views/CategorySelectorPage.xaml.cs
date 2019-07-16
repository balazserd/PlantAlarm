using System;
using System.Collections.Generic;
using PlantAlarm.DependencyServices;
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

            MessagingCenter.Subscribe<CategorySelectorViewModel>(this, "ShowCategoryAdderModal", async(viewModel) =>
            {
                string categoryName = await DependencyService.Get<ITextInputModalProvider>().ShowTextModal();
                if (!string.IsNullOrEmpty(categoryName))
                {
                    MessagingCenter.Send(this, "AddCategoryFromModal", categoryName);
                }
            });

            BindingContext = new CategorySelectorViewModel();
            vm = BindingContext as CategorySelectorViewModel;
        }

        void Handle_Appearing(object sender, EventArgs e)
        {
            vm.AppearingCommand.Execute(null);
        }

        //POSSIBLE UPGRADE TO SHOW PLANTS OF CATEGORY TAPPED
        //void CategoryTapped(object sender, ItemTappedEventArgs e)
        //{
        //    var categoryItem = e.Item as CategoryItem;
        //    vm.ExpandCategoryItemCommand(categoryItem);
        //}
    }
}
