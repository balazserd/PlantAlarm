using System;
using System.Collections.Generic;
using PlantAlarm.Models;
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

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            DaysListView.ScrollTo(DaysListView.SelectedItem, ScrollToPosition.Center, true);
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            ScrollToToday();
        }

        private void ScrollToToday()
        {
            DaysListView.SelectedItem = vm.CalendarDays[121];
            DaysListView.ScrollTo(vm.CalendarDays[121], ScrollToPosition.Center, true);
        }
    }
}
