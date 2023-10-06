using Avalonia;
using System;
using System.IO;

namespace FCBConverterGUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            try
            {
            }
            catch (Exception ex)
            {
                File.WriteAllText(AppContext.BaseDirectory + Path.DirectorySeparatorChar + "Error.txt", ex.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect().UseSkia()
                .LogToTrace();
    }
}
