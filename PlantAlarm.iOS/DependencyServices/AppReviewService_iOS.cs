using System;
using Foundation;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using StoreKit;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppReviewService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class AppReviewService_iOS : IAppReviewServiceProvider
    {
        public void RequestReview()
        {
            var writeReviewUrl = new NSUrl("https://itunes.apple.com/app/id1506366093?action=write-review");

            UIApplication.SharedApplication.OpenUrl(writeReviewUrl);
        }
    }
}
