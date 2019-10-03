using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace PlantAlarm.Views.RootPages
{
    public class SafeAreaRespectingPage : ContentPage
    {
        public SafeAreaRespectingPage()
        {
            On<iOS>().SetUseSafeArea(true);
        }
    }
}
