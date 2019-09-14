using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class TodayPage : ContentPage
    {
        private readonly TodayViewModel vm;

        public TodayPage()
        {
            InitializeComponent();

            BindingContext = new TodayViewModel();
            vm = BindingContext as TodayViewModel;

            ScrollToToday();
        }

        void NewDaySelected(object sender, SelectedItemChangedEventArgs e)
        {
            DaysListView.ScrollTo(DaysListView.SelectedItem, ScrollToPosition.Center, true);
            vm.SelectedDayChangedCommand.Execute(e.SelectedItem);
        }

        void BackToTodayTapped(object sender, EventArgs e)
        {
            ScrollToToday();
        }

        private void ScrollToToday()
        {
            DaysListView.SelectedItem = vm.CalendarDays[121];
            DaysListView.ScrollTo(vm.CalendarDays[121], ScrollToPosition.Center, true);
        }

        void ActivityTapped(object sender, SelectedItemChangedEventArgs e)
        {
            vm.ActivitySelectedCommand.Execute(e.SelectedItem);
        }
    }
}
