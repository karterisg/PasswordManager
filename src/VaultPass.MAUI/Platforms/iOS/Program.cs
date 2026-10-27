// The static Main method is the true entry point on iOS.
// UIApplication.Main starts the UIKit run loop and tells it to use AppDelegate
// as the application delegate that handles lifecycle events.
using ObjCRuntime;
using UIKit;

namespace VaultPass.MAUI;

public class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
