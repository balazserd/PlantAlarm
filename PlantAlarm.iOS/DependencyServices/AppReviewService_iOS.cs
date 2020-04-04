using System;
using PlantAlarm.DependencyServices;
using StoreKit;

namespace PlantAlarm.iOS.DependencyServices
{
    public class AppReviewService_iOS : IAppReviewServiceProvider
    {
        public void RequestReview()
        {
            SKStoreReviewController.RequestReview();
        }
    }
}
