using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using PlantAlarm.Views.RootPages;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewTaskPage : SafeAreaRespectingPage
    {
        private const int CellHeight = 44;
        private readonly NewTaskViewModel vm;

        public NewTaskPage(bool isEditingMode, PlantTask taskToEdit = null)
        {
            InitializeComponent();

            this.DatePicker.MinimumDate = DateTime.Now.Date;
            this.TimePicker.Time = TimeSpan.FromHours(8);

            this.BindingContext = new NewTaskViewModel(this, isEditingMode, taskToEdit);
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
