﻿using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class TodoPage : SafeAreaRespectingPage
    {
        public TodoPage()
        {
            InitializeComponent();
            this.BindingContext = new TodayViewModel(this);
        }
    }
}