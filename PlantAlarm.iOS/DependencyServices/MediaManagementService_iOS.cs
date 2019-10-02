using System;
using Foundation;
using Photos;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using Plugin.Media.Abstractions;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(MediaManagementService_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class MediaManagementService_iOS : IMediaManagementService
    {
        public bool IsSavingPhotosToCameraRollPermitted()
        {
            return PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized;
        }

        public void RequestAccessToCameraRoll()
        {
            PHPhotoLibrary.RequestAuthorization(status => { });
        }

        public void SavePhotoToCameraRoll(MediaFile photo)
        {
            var uiImage = UIImage.FromFile(photo.Path);
            Device.BeginInvokeOnMainThread(() => uiImage.SaveToPhotosAlbum(null));
        }

        public void ShowExplanatoryTextForDenyingPhotoAlbumRequest()
        {
            var alert = UIAlertController.Create(
                "Warning",
                "You either decided not to allow access to your Camera Roll or you are forbidden from changing this setting. " +
                "PlantAlarm is unable to save the photos you take to your Photos App. To change this," +
                "you need to allow access manually under Settings > PlantAlarm > Photos or ask your supervisor to allow access.",
                UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create(
                "Not now",
                UIAlertActionStyle.Cancel,
                null));
            alert.AddAction(UIAlertAction.Create(
                "Open Settings",
                UIAlertActionStyle.Default,
                (uiAlertAction) => {
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
                }));

            var window = new UIWindow(UIScreen.MainScreen.Bounds);
            var viewController = new UIViewController();
            viewController.View.BackgroundColor = UIColor.Clear;
            window.RootViewController = viewController;
            window.WindowLevel = UIWindowLevel.Alert + 1;
            window.MakeKeyAndVisible();

            viewController.PresentViewController(alert, true, () =>
            {
                window.DangerousAutorelease();
            });
        }
    }
}
