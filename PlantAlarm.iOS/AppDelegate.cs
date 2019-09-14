using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using PlantAlarm.iOS.DependencyServices;
using PlantAlarm.Services;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using VideoToolbox;

namespace PlantAlarm.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            //TouchTracking bug requires this line: https://github.com/OndrejKunc/SkiaScene#touchtrackingforms
            var _ = new TouchTracking.Forms.iOS.TouchEffect();

            //Ask for permission to Notifications.
            NotificationService.AskForNotificationPermission();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}
