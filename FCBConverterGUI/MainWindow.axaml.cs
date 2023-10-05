using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace FCBConverterGUI
{
    public partial class MainWindow : Window
	{
		const string appVer = "v0.01-DEV";
		public const string appName = "FCBConverter";

        public static Window MainWnd;

        public MainWindow()
		{
			InitializeComponent();

            MainWnd = this;
			
			wndTitle.Content = Title = appName;
            verT.Content = appVer;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
            DiscordOwnRPC.Connect();
		}

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            DiscordOwnRPC.Disconnect();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Keyboard.Keys.Add(e.Key);
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Keyboard.Keys.Remove(e.Key);
            base.OnKeyUp(e);
        }

        public void MainWindow_Deactivated(object sender, EventArgs e)
        {
            Keyboard.Clear();
        }

        private async void W_KeyDown(object sender, KeyEventArgs e)
		{
			/*if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				if (loaded)
					Animation(true, gridSearch);
			}
			if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				if (loaded)
                    parser.CopyingMakeCopy();
			}
			if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				if (loaded)
                    await parser.CopyingPaste();
			}
			if (e.Key == Key.Delete)
			{
				if (loaded)
                    parser.SelectedDelete();
			}*/
		}

		private void Animation(bool fadeInOut, Grid grid)
		{
            foreach (var ch in grid.GetVisualDescendants().OfType<Button>())
            {
                if (fadeInOut)
                    (ch as Button).IsEnabled = true;
                else
                    (ch as Button).IsEnabled = false;
            }

			if (fadeInOut)
			{
				grid.IsVisible = true;
			}
            
            grid.Opacity = fadeInOut ? 1 : 0;

            if (!fadeInOut)
            {
                Timer aTimer = new Timer(300);
                aTimer.Enabled = true;
                aTimer.Elapsed += (object source, ElapsedEventArgs e) =>
                {
                    aTimer.Stop();
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        grid.IsVisible = false;
                    });
                };
            }
        }

        private void OpenAskDialog(string name, string val)
        {
            dialogAskName.Content = name;
            dialogAskDesc.Text = val;
            Animation(true, gridDialogAsk);
        }

        private void ButtonDialogAskClose_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
			{}

            if (tag == "1")
			{}

            Animation(false, gridDialogAsk);
        }

        private void OpenInfoDialog(string name, string val)
        {
            dialogInfoName.Content = name;
            dialogInfoLabel.Text = val;
            Animation(true, gridDialogInfo);
        }

        private void ButtonDialogInfoClose_Click(object sender, RoutedEventArgs e)
        {
            Animation(false, gridDialogInfo);
        }

		private void ShowNotice(string text)
		{
            noticeNote.Content = text;

            gridNoticeNote.Opacity = 1;

            Timer aTimer = new Timer(5000);
            aTimer.Enabled = true;
            aTimer.Elapsed += (object source, ElapsedEventArgs e) =>
            {
                aTimer.Stop();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    gridNoticeNote.Opacity = 0;
                });
            };
        }

        private void ButtonChrome_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
            {
                Close();
            }
            if (tag == "1")
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            if (tag == "2")
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void MoveWnd(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsLeftButtonPressed && e.ClickCount == 1)
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
                BeginMoveDrag(e);
                Cursor = new Cursor(StandardCursorType.Arrow);
            }

            if (props.IsLeftButtonPressed && e.ClickCount == 2)
                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
        }

        GameType SelectedGame = GameType.Invalid;

        private void GameSel_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((CheckBox)sender).Tag;
            if (tag == "gameFC2") SelectedGame = GameType.FarCry2;
            if (tag == "gameFC3") SelectedGame = GameType.FarCry3;
            if (tag == "gameFC3BD") SelectedGame = GameType.FarCry3BloodDragon;
            if (tag == "gameFC4") SelectedGame = GameType.FarCry4;
            if (tag == "gameFCP") SelectedGame = GameType.FarCryPrimal;
            if (tag == "gameFC5") SelectedGame = GameType.FarCry5;
            if (tag == "gameFCND") SelectedGame = GameType.FarCryNewDawn;
            if (tag == "gameFC6") SelectedGame = GameType.FarCry6;
        
            foreach (var ch in selGameGrid.GetVisualDescendants().OfType<CheckBox>())
            {
                if ((CheckBox)sender != ch)
                    ch.IsChecked = false;
            }

            ((CheckBox)sender).IsChecked = true;
        }

        readonly string[][] files = new string[][]
        {
            new string[] { "Far Cry Binary file", "*.fcb" },
            new string[] { "Database file", "*.ndb" },
            new string[] { "Dependency loader file", "*_depload.dat" },
            new string[] { "Sound info file", "soundinfo.bin" },
            new string[] { "Animation markup file", "*.markup.bin" },
            new string[] { "Far Cry 5 / ND / 6 Strings file", "oasisstrings.oasis.bin" },
            new string[] { "Far Cry 3 / 4 Strings file", "oasisstrings_compressed.bin" },
            new string[] { "Lua file (adds LUAC header)", "*.lua" },
            new string[] { "Material file", "*.material.bin" },
            new string[] { "Texture file", "*.xbt" },
            new string[] { "Terrain texture file", "*.xbts" },
            new string[] { "Animation move file", "*.move.bin" },
            new string[] { "Combined Move File", "CombinedMoveFile.bin" },
            new string[] { "Sequence file", "*.cseq" },
            new string[] { "Flash UI file", "*.feu" },
            new string[] { "Bundle file", "*.bdl" },
            new string[] { "Binary WolfSkin file", "*.bwsk" },
            new string[] { "Wwise SoundBank file", "*.bnk" },
            new string[] { "Wwise Encoded Media", "*.wem" },
            new string[] { "File allocation table (DAT header file)", "*.fat" },
            new string[] { "Converted files", "*.converted.xml" },
        };

        private void SelectFileConvert_Click(object sender, RoutedEventArgs e)
        {
            FilePickerOpenOptions opts = new();
            opts.AllowMultiple = false;
            opts.Title = "Select a file";
            //opts.FileTypeFilter = new FilePickerFileType[] { new("Domino script") { Patterns = new[] { "*.lua" } } };

            List<FilePickerFileType> filter = new();
            foreach (var a in files)
                filter.Add(new(a[0]) { Patterns = new[] { a[1] } });

            opts.FileTypeFilter = filter.ToArray();

            var d = StorageProvider.OpenFilePickerAsync(opts).Result;
            if (d != null && d.Count > 0)
            {
                // d[0].Path.LocalPath
            }
        }
    }
}