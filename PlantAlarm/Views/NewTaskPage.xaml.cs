using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewTaskPage : ContentPage
    {
        private const int CellHeight = 44;
        private readonly NewTaskViewModel vm;

        public NewTaskPage()
        {
            InitializeComponent();

            this.DatePicker.MinimumDate = DateTime.Now.Date;
            this.TimePicker.Time = TimeSpan.FromHours(8);

            this.BindingContext = new NewTaskViewModel();
            vm = this.BindingContext as NewTaskViewModel;
        }

        void CategorySelectorTapped(object sender, EventArgs e)
        {
            //Show category selector window.
        }

        void AddPlantsTapped(object sender, EventArgs e)
        {
            vm.AddPlantsCommand.Execute(vm.PlantList);
        }
    }
}
