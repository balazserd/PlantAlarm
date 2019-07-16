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
        public Task<string> ShowTextModal()
        {
            var completionSource = new TaskCompletionSource<string>();
            completionSource.SetResult(null);

            var modal = UIAlertController.Create("New Category", "Please enter the name for the new category you wish to add.", UIAlertControllerStyle.Alert);

            modal.AddTextField((textField) =>
            {
                textField.Placeholder = "ExampleCategory";
            });

            modal.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            modal.AddAction(UIAlertAction.Create("Add", UIAlertActionStyle.Default, (action) => { completionSource.SetResult(modal.TextFields[0].Text); }));

            var window = new UIWindow(UIScreen.MainScreen.Bounds);
            var viewController = new UIViewController();
            viewController.View.BackgroundColor = UIColor.Clear;
            window.RootViewController = viewController;
            window.WindowLevel = UIWindowLevel.Alert + 1;
            window.MakeKeyAndVisible();

            viewController.PresentViewController(modal, true, null);

            return completionSource.Task;
        }
    }
}
