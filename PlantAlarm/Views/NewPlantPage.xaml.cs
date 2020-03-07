﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using PlantAlarm.ViewModels;
using PlantAlarm.Views.RootPages;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewPlantPage : SafeAreaRespectingPage
    {
        private NewPlantViewModel vm;
        public NewPlantPage()
        {
            InitializeComponent();

            this.BindingContext = new NewPlantViewModel(this);
            vm = this.BindingContext as NewPlantViewModel;
        }

        void ShowAddCategoryPage(object sender, EventArgs e)
        {
            vm.ShowCategorySelectorPageCommand.Execute(null);
        }

        void CheckIfImageSourceChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Image.Source))
            {
                this.ImageContainerStack.ForceLayout();
            }
        }
    }
}
