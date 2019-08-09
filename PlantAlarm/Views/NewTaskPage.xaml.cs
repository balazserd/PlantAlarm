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

        void CategorySelectorTapped(object sender, System.EventArgs e)
        {
            //Show category selector window.
        }

        private bool IsRecurringTask()
        {
            bool EveryXDaysEntryMakesSense = int.TryParse(EveryXDaysEntry.Text, out int dayInterval) && dayInterval > 0; //Can be parsed into a valid int.
            bool EveryXMonthsEntryMakesSense = int.TryParse(EveryXMonthsEntry.Text, out int monthInterval) && monthInterval > 0; //Can be parsed into a valid int.

            return MondayCheckBox.IsChecked || TuesdayCheckBox.IsChecked || WednesdayCheckBox.IsChecked || ThursdayCheckBox.IsChecked || FridayCheckBox.IsChecked || SaturdayCheckBox.IsChecked || SundayCheckBox.IsChecked || (EveryXDaysEntry != null && EveryXDaysEntryMakesSense) || (EveryXMonthsEntry != null && EveryXMonthsEntryMakesSense);
        }

        void AddPlantsTapped(object sender, EventArgs e)
        {
            vm.AddPlantsCommand.Execute(vm.PlantList);
        }
    }
}
