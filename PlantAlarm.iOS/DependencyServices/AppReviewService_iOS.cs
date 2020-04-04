using System;
using Foundation;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using StoreKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppReviewService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class AppReviewService_iOS : IAppReviewServiceProvider
    {
        public void RequestReview()
        {
            var writeReviewUrl = new NSUrl("https://itunes.apple.com/app/idXXXXXXXXXX?action=write-review")
        }
    }
}
