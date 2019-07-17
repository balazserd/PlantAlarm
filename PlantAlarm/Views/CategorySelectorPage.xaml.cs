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

            MessagingCenter.Subscribe<object>(this, "ShowCategoryAdderModal", async (viewmodel) => {
                var categoryName = await DependencyService.Get<ITextInputModalProvider>().ShowTextModalAsync();
                if (!string.IsNullOrEmpty(categoryName))
                {
                    MessagingCenter.Send((object)this, "AddCategoryFromModal", categoryName);
                }
            });

            BindingContext = new CategorySelectorViewModel();
            vm = BindingContext as CategorySelectorViewModel;
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<object>(this, "ShowCategoryAdderModal");
            base.OnDisappearing();
        }

        void Handle_Appearing(object sender, EventArgs e)
        {
            vm.AppearingCommand.Execute(null);
        }
    }
}
