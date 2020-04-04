using System;
using PlantAlarm.DependencyServices;
using Xamarin.Forms;

namespace PlantAlarm.Services
{
    public static class AppReviewService
    {
        public static void InitiateReviewRequest()
        {
            DependencyService.Get<IAppReviewServiceProvider>().RequestReview();
        }
    }
}
