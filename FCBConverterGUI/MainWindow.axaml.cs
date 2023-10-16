using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string baseDir;

        public static Window MainWnd;

        public MainWindow()
		{
			InitializeComponent();

            MainWnd = this;
			
			wndTitle.Content = Title = appName;
            verT.Content = appVer;
            
            using var processModule = Process.GetCurrentProcess().MainModule;
            baseDir = Path.GetDirectoryName(processModule?.FileName);

            ueDesc.Text = 
                "The UAsset file should have also UExp file and TXT file with names of materials, for example:" + Environment.NewLine +
                "handw_avatar_mygloves_aver_mf.txt" + Environment.NewLine +
                "handw_avatar_mygloves_aver_mf.uasset" + Environment.NewLine +
                "handw_avatar_mygloves_aver_mf.uexp" + Environment.NewLine + Environment.NewLine +
                "The TXT must contains material name, each material on new line, so for example:" + Environment.NewLine +
                "graymond-M-20180904151809" + Environment.NewLine + Environment.NewLine +
                "The reference XBG is required for some binary data for new XBG file." + Environment.NewLine +
                "The type of XBG must be same, so for example hand cloth must have same reference hand cloth XBG.";
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
            DiscordOwnRPC.Connect();

            List<HiddenMeshListEntry> a = new();
            for (int i = 0; i < 100; i++)
                a.Add(new() { Name = "aa " + i.ToString(), Enabled = false });
            hiddenMeshList.ItemsSource = a;

            List<HiddenFacesListEntry> b = new();
            for (int i = 0; i < 100; i++)
                b.Add(new() { Name = "aa " + i.ToString(), FaceStartIndex = "100", CountOfFaces = "1000" });
            hiddenFacesList.ItemsSource = b;
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

        private int CallFCBConverter(string launchParams)
        {
            Process process = new Process();
            process.StartInfo.FileName = baseDir + "FCBConverter";
            process.StartInfo.Arguments = launchParams + " -keep";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        private bool CheckSelectedGame()
        {
            if (SelectedGame == GameType.Invalid)
            {
                OpenInfoDialog("Game", "Game is not selected. Please select game on first tab.");
                return false;
            }

            return true;
        }

        private GameType SelectedGame = GameType.Invalid;

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

        private void ConvertFile_Click(object sender, RoutedEventArgs e)
        {
            CallFCBConverter(fileToConvert.Text);
        }

        private async Task<string> OpenFileDialog(string title, FilePickerFileType[] files)
        {
            FilePickerOpenOptions opts = new();
            opts.AllowMultiple = false;
            opts.Title = title;
            opts.FileTypeFilter = files;

            var d = await StorageProvider.OpenFilePickerAsync(opts);
            if (d != null && d.Count > 0)
            {
                return d[0].Path.LocalPath;
            }

            return "";
        }

        private async Task<string> OpenFolderDialog(string title)
        {
            FolderPickerOpenOptions opts = new();
            opts.AllowMultiple = false;
            opts.Title = title;

            var d = await StorageProvider.OpenFolderPickerAsync(opts);

            if (d != null && d.Count > 0)
            {
                return d[0].Path.LocalPath;
            }
            
            return "";
        }

        private async Task<string> SaveFileDialog(string title, FilePickerFileType[] files)
        {
            FilePickerSaveOptions opts = new();
            opts.FileTypeChoices = files;
            opts.Title = title;

            var d = await StorageProvider.SaveFilePickerAsync(opts);
			if (d != null)
			{
				return d.Path.LocalPath;
            }
            
            return "";
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
            new string[] { "Lua file (bytecode / Domino box to code)", "*.lua" },
            new string[] { "Lua code to Domino box", "*.lua.converted.xml" },
            new string[] { "Lua to bytecode", "*.decompiled.lua" },
            new string[] { "Material file", "*.material.bin" },
            new string[] { "Texture file", "*.xbt" },
            new string[] { "Terrain texture file", "*.xbts" },
            new string[] { "DirectDraw Surface texture (to XBT)", "*.dds" },
            new string[] { "Animation move file", "*.move.bin" },
            new string[] { "Combined Move File", "CombinedMoveFile.bin" },
            new string[] { "Compiled Sequence file", "*.cseq" },
            new string[] { "Flash UI file (changes header to SWF)", "*.feu" },
            new string[] { "Flash UI file (changes header to FEU)", "*.swf" },
            new string[] { "Bundle file", "*.bdl" },
            new string[] { "Binary WolfSkin file", "*.bwsk" },
            new string[] { "Wwise SoundBank file", "*.bnk" },
            new string[] { "Wwise Encoded Media", "*.wem" },
            new string[] { "File allocation table (DAT header file)", "*.fat" },
            new string[] { "Havok physics file (extracts only FCB data)", "*.hkx" },
            new string[] { "Phoenix UI file (extracts only FCB data)", "*.spx" },
            new string[] { "Particle file", "*.part" },
            new string[] { "Skeleton file", "*.skeleton" },
            new string[] { "Animation track file", "*.animtrackcol" },
            new string[] { "Binary file", "*.bin" },
            new string[] { "GOSM file", "*.gosm.xml" },
            new string[] { "Binary XML file", "*.rml" },
            new string[] { "Converted files", "*.converted.xml" },
        };

        private async void SelectFileConvert_Click(object sender, RoutedEventArgs e)
        {
            List<FilePickerFileType> filter = new();
            foreach (var a in files)
                filter.Add(new($"{a[0]} ({a[1]})") { Patterns = new[] { a[1] } });

            fileToConvert.Text = await OpenFileDialog("Select a file", filter.ToArray());
        }

        private async void SelectFATUnpack_Click(object sender, RoutedEventArgs e)
        {
            unpackFATFile.Text = await OpenFileDialog("Select a FAT file", new FilePickerFileType[] { new("FAT file (*.fat)") { Patterns = new[] { "*.fat" } } });
        }

        private async void SelectFATUnpackDest_Click(object sender, RoutedEventArgs e)
        {
            unpackFATFileDest.Text = await OpenFolderDialog("Select output folder");
        }

        private void FATUnpack_Click(object sender, RoutedEventArgs e)
        {
            CallFCBConverter($"-source=\"{unpackFATFile.Text}\" -out=\"{unpackFATFileDest.Text}\"");
        }

        private async void SelectFATPackDest_Click(object sender, RoutedEventArgs e)
        {
			packFATFileDest.Text = await SaveFileDialog("Save a FAT file", new FilePickerFileType[] { new("FAT file (*.fat)") { Patterns = new[] { "*.fat" } } });
        }

        private async void SelectFATPackSource_Click(object sender, RoutedEventArgs e)
        {
            packFATFileSource.Text = await OpenFolderDialog("Select source folder");
        }

        private void FATPack_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSelectedGame())
                return;

            string fatVer;
            if (SelectedGame == GameType.FarCry2) fatVer = "-v5";
            else if (SelectedGame == GameType.FarCry6) fatVer = "-v11";
            else if (SelectedGame == GameType.FarCry5 || SelectedGame == GameType.FarCryNewDawn) fatVer = "";
            else fatVer = "-v9";

            string compress = "-disablecompress";
            if (enableCompression.IsChecked == true) compress = "-enablecompress";

            string excludeCompress = "-excludeFilesFromCompress=" + extFilesCompress.Text;

            CallFCBConverter($"-source=\"{packFATFileSource.Text}\" -fat=\"{packFATFileDest.Text}\" {fatVer} {compress} {excludeCompress}");
        }
        
        private async void SelectSingleUnpack_Click(object sender, RoutedEventArgs e)
        {
            unpackSingleFile.Text = await OpenFileDialog("Select a FAT file", new FilePickerFileType[] { new("FAT file (*.fat)") { Patterns = new[] { "*.fat" } } });
        }
        
        private async void SelectSingleUnpackDest_Click(object sender, RoutedEventArgs e)
        {
            unpackSingleFileDest.Text = await OpenFolderDialog("Select output folder");
        }
        
        private void SingleUnpack_Click(object sender, RoutedEventArgs e)
        {
            CallFCBConverter($"-source=\"{unpackFATFile.Text}\" -out=\"{unpackFATFileDest.Text}\" -single=\"{unpackSingleFileName.Text}\"");
        }

        private async void SelectFolderConvert_Click(object sender, RoutedEventArgs e)
        {
            folderToConvert.Text = await OpenFolderDialog("Select source folder");
        }
        
        private async void ConvertFolder_Click(object sender, RoutedEventArgs e)
        {
            string subFld = "";
            if (allowSubfolders.IsChecked == true) subFld = "-subfolders";

            CallFCBConverter($"-source=\"{folderToConvert.Text}\" -filter=\"{folderFilter.Text}\" {subFld}");
        }
        
        private async void SelectFPXbgFile_Click(object sender, RoutedEventArgs e)
        {
            fpXbgFile.Text = await OpenFileDialog("Select XBG file for fix", new FilePickerFileType[] { new("XBG file (*.xbg)") { Patterns = new[] { "*.xbg" } } });
        }
        
        private void hiddenMeshList_MouseDoubleClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(hiddenMeshList).Properties;

            if (e.ClickCount == 2 && props.IsLeftButtonPressed)
            {
                if (hiddenMeshList.SelectedIndex != -1)
                {
                }
            }
        }
        
        private void hiddenFacesList_MouseDoubleClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(hiddenFacesList).Properties;

            if (e.ClickCount == 2 && props.IsLeftButtonPressed)
            {
                if (hiddenFacesList.SelectedIndex != -1)
                {
                }
            }
        }






        private async void SelectUEFile_Click(object sender, RoutedEventArgs e)
        {
            ueFile.Text = await OpenFileDialog("Select UAsset file", new FilePickerFileType[] { new("UAsset file (*.uasset)") { Patterns = new[] { "*.uasset" } } });
        }

        private async void SelectUERefFile_Click(object sender, RoutedEventArgs e)
        {
            ueRefFile.Text = await OpenFileDialog("Select reference XBG file", new FilePickerFileType[] { new("XBG file (*.xbg)") { Patterns = new[] { "*.xbg" } } });
        }

        private string selectedUEModelType = "0";

        private void UEModelTypeSel_Click(object sender, RoutedEventArgs e)
        {
            selectedUEModelType = (string)((CheckBox)sender).Tag;
        
            foreach (var ch in selUEModelTypeGrid.GetVisualDescendants().OfType<CheckBox>())
            {
                if ((CheckBox)sender != ch)
                    ch.IsChecked = false;
            }

            ((CheckBox)sender).IsChecked = true;
        }

        private void UEConvert_Click(object sender, RoutedEventArgs e)
        {
            CallFCBConverter($"-UE2XBG={selectedUEModelType} -ue=\"{ueFile.Text}\" -xbg=\"{ueRefFile.Text}\"");
        }
    }
}