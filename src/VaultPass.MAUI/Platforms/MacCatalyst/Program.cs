// Mac Catalyst entry point — identical to iOS/Program.cs.
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
