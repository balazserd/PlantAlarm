using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewPlantPage : ContentPage
    {
        private NewPlantViewModel vm;
        public NewPlantPage()
        {
            InitializeComponent();

            this.BindingContext = new NewPlantViewModel(Application.Current.MainPage.Navigation, this);
            vm = this.BindingContext as NewPlantViewModel;
        }

        void ShowAddCategoryPage(object sender, EventArgs e)
        {
            vm.ShowCategorySelectorPageCommand.Execute(null);
        }
    }
}
