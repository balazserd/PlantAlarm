using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace PlantAlarm.Views.RootPages
{
    public class RootPage : ContentPage
    {
        public RootPage()
        {
            On<iOS>().SetUseSafeArea(true);
        }
    }
}
