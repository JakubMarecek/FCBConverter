using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace FCBConverterGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += OnStartup;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnStartup(object? s, ControlledApplicationLifetimeStartupEventArgs e)
        {
            var wnd = new MainWindow();
#if LINUXBUILD
            wnd.SystemDecorations = SystemDecorations.BorderOnly;
#endif
            wnd.Show();
        }
    }
}
