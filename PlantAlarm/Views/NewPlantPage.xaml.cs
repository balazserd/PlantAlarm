using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewPlantPage : ContentPage
    {
        public NewPlantPage()
        {
            InitializeComponent();

            this.OneTimeDatePicker.MinimumDate = DateTime.Now.Date;
            this.OneTimeTimePicker.Time = TimeSpan.FromHours(8);
        }

        void IsRepeatingChanged(object sender, ToggledEventArgs e)
        {
            //Change which form part is showing.
        }
    }
}
