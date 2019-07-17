using System;
using System.Threading.Tasks;
using PlantAlarm.DependencyServices;
using PlantAlarm.iOS.DependencyServices;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(TextInputModal_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class TextInputModal_iOS : ITextInputModalProvider
    {
        public async Task<string> ShowTextModalAsync()
        {
            var completionSource = new TaskCompletionSource<string>();
            var modalTask = completionSource.Task;

            var window = new UIWindow(UIScreen.MainScreen.Bounds);
            var viewController = new UIViewController();
            viewController.View.BackgroundColor = UIColor.Clear;
            window.RootViewController = viewController;
            window.WindowLevel = UIWindowLevel.Alert + 1;
            window.MakeKeyAndVisible();

            var modal = UIAlertController.Create("New Category", "Please enter the name for the new category you wish to add.", UIAlertControllerStyle.Alert);

            modal.AddTextField((textField) =>
            {
                textField.Placeholder = "ExampleCategory";
            });
            modal.AddAction(UIAlertAction.Create("Add", UIAlertActionStyle.Default, (action) => {
                completionSource.SetResult(modal.TextFields[0].Text);
                window.DangerousAutorelease();
            }));
            modal.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => {
                completionSource.SetResult(null);
                window.DangerousAutorelease();
            }));
            //This release should not be dangerous.
            viewController.PresentViewController(modal, true, null); 

            return await modalTask;
        }
    }
}
