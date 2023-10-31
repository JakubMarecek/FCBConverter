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
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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

            wlDesc.Text = 
                "This app is a GUI version for command line app FCBConverter and makes it esaily usable by even less skilled modders." + Environment.NewLine +
                "This app is divided into several tabs, categorized by specific usage of FCBConv." + Environment.NewLine +
                "" + Environment.NewLine +
                "Selected game" + Environment.NewLine +
                "Here you can specify what game you will modify. It's required because some converting processes are specific for each FC game." + Environment.NewLine +
                "" + Environment.NewLine +
                "Convert files" + Environment.NewLine +
                "Here you can convert single file. You can also use batch converting in case of many files." + Environment.NewLine +
                "" + Environment.NewLine +
                "Unpack / pack game files" + Environment.NewLine +
                "In this tab you can unpack and pack game main files. All FC games use same format DAT/FAT but each FC game uses a bit changed version of DAT/FAT." + Environment.NewLine +
                "You can even specify a single file name which you want to unpack from a DAT/FAT." + Environment.NewLine +
                "" + Environment.NewLine +
                "Fix XBG for FP" + Environment.NewLine +
                "Here you can fix FP issues with clothing. Clothing which is primary for NPCs are missing parts which are required for player's usage, so without them you can experience doubled legs or hands." + Environment.NewLine +
                "You can even define which part of body mesh will be hidden if a cloth will be equipped." + Environment.NewLine +
                "" + Environment.NewLine +
                "Convert UE to XBG" + Environment.NewLine +
                "Converting backed Unreal Engine Asset to XBG. This process is currently only know way how to get new models into the FC game." + Environment.NewLine +
                "" + Environment.NewLine +
                "";
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
            DiscordOwnRPC.Connect();

            /*List<HiddenMeshListEntry> a = new();
            for (int i = 0; i < 100; i++)
                a.Add(new() { Name = "aa " + i.ToString(), Enabled = false });
            hiddenMeshList.ItemsSource = a;

            List<HiddenFacesListEntry> b = new();
            for (int i = 0; i < 5; i++)
                b.Add(new() { Name = "aa " + i.ToString(), FaceStartIndex = 100, CountOfFaces = 1000 });
            hiddenFacesList.ItemsSource = b;*/

            hiddenFacesList.ItemsSource = new List<HiddenFacesListEntry>();
            hiddenMeshList.ItemsSource = new List<HiddenMeshListEntry>();
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

        private void TextBlock_MouseLeftButtonDown(object sender, PointerReleasedEventArgs e)
        {
            string url = "https://downloads.fcmodding.com/others/fcbconverter/";

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
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

        Action askDialogAccept;
        Action askDialogCancel;

        private void OpenAskDialog(string name, string val, Action accept, Action cancel)
        {
            dialogAskName.Content = name;
            dialogAskDesc.Text = val;
            askDialogAccept = accept;
            askDialogCancel = cancel;
            Animation(true, gridDialogAsk);
        }

        private void ButtonDialogAskClose_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
                askDialogCancel();

            if (tag == "1")
                askDialogAccept();

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

            LoadMeshParts();
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
            /*var props = e.GetCurrentPoint(hiddenMeshList).Properties;

            if (e.ClickCount == 2 && props.IsLeftButtonPressed)
            {
                if (hiddenMeshList.SelectedIndex != -1)
                {
                }
            }*/
        }
        
        private void hiddenFacesList_MouseDoubleClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(hiddenFacesList).Properties;

            if (e.ClickCount == 2 && props.IsLeftButtonPressed)
            {
                if (hiddenFacesList.SelectedIndex != -1)
                {
                    var selFL = (HiddenFacesListEntry)hiddenFacesList.SelectedItem;

                    addHFMeshPart.ItemsSource = GetMeshParts();
                    addHFMeshPart.SelectedIndex = GetMeshParts().FindIndex(a => a.ID == selFL.ID);
                    addHFFaceStartIndex.Value = selFL.FaceStartIndex;
                    addHFFaceCount.Value = selFL.CountOfFaces;

                    hiddenFacesAddEdit = true;
                    addHFAddBtn.Content = "Edit";

                    Animation(true, gridDialogAddFace);
                }
            }
        }
        
        bool hiddenFacesAddEdit = false;

        private async void ButtonAddFaceAdd_Click(object sender, RoutedEventArgs e)
        {
            var entry = (HiddenFacesListEntry)addHFMeshPart.SelectedItem;
            entry.CountOfFaces = (int)addHFFaceCount.Value;
            entry.FaceStartIndex = (int)addHFFaceStartIndex.Value;
            
            var a = hiddenFacesList.ItemsSource.OfType<HiddenFacesListEntry>().ToList();

            if (hiddenFacesAddEdit)
                a[hiddenFacesList.SelectedIndex] = entry;
            else
                a.Add(entry);

            hiddenFacesList.ItemsSource = a;

            Animation(false, gridDialogAddFace);
        }
        
        private async void ButtonAddFaceCancel_Click(object sender, RoutedEventArgs e)
        {
            Animation(false, gridDialogAddFace);
        }

        private List<HiddenFacesListEntry> GetMeshParts()
        {
            if (SelectedGame == GameType.FarCry5)
                return MeshParts.meshPartsFC5;

            else if (SelectedGame == GameType.FarCryNewDawn)
                return MeshParts.meshPartsNewDawn;

            else if (SelectedGame == GameType.FarCry6)
                return MeshParts.meshPartsFC6;

            else
            {
            }

            return null;
        }

        private async void FPFacesAdd_Click(object sender, RoutedEventArgs e)
        {
            hiddenFacesAddEdit = false;
            addHFAddBtn.Content = "Add";

            addHFMeshPart.ItemsSource = GetMeshParts();
            addHFMeshPart.SelectedIndex = 0;
            addHFFaceStartIndex.Value = 0;
            addHFFaceCount.Value = 0;
            Animation(true, gridDialogAddFace);
        }
        
        private async void FPFacesCalc_Click(object sender, RoutedEventArgs e)
        {
            OpenAskDialog("Fix XBG for FP", "Autocalculate will add all faces from selected material and submesh. This is used for clothes of type feet, bottom and handwear. Top parts can produce bugs like missing body when you look down.",
                () => {
                    var meshList = hiddenFacesList.ItemsSource.OfType<HiddenFacesListEntry>().ToList();

                    int clc = (int)Math.Floor((double)sourceXbgSkelFaces[(int)fpSkelSel.Value][(int)fpMatSel.Value] / meshList.Count);

                    for (int i = 0; i < meshList.Count; i++)
                    {
                        meshList[i].FaceStartIndex = (clc * i * 3);
                        meshList[i].CountOfFaces = (clc * 3);
                    }

                    hiddenFacesList.ItemsSource = meshList;
                },
                () => {}
            );
        }
        
        private async void FPFacesRemove_Click(object sender, RoutedEventArgs e)
        {
            var a = hiddenFacesList.ItemsSource.OfType<HiddenFacesListEntry>().ToList();
            a.RemoveAt(hiddenFacesList.SelectedIndex);
            hiddenFacesList.ItemsSource = a;
        }
        
        private async void ButtonXBGInfoClose_Click(object sender, RoutedEventArgs e)
        {
            Animation(false, gridDialogXBGInfo);
        }
        
        private async void FPConvert_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(fpXbgFile.Text))
                return;

            string outParamVal = "";

            outParamVal += "SKELHIDEFACESFP;";
            for (int i = 0; i < sourceXbgSkelHideFacesFP.Count; i++)
            {
                outParamVal += (i > 0 ? "," : "");
                for (int j = 0; j < sourceXbgSkelHideFacesFP[i].Count; j++)
                {
                    outParamVal += (j > 0 ? "+" : "");
                    for (int k = 0; k < sourceXbgSkelHideFacesFP[i][j].Count; k++)
                    {
                        var hideFace = sourceXbgSkelHideFacesFP[i][j][k];
                        outParamVal += (k > 0 ? "-" : "") + hideFace.ID + "*" + hideFace.FaceStartIndex + "*" + hideFace.CountOfFaces;
                    }
                }
            }

            outParamVal += "|MESHPARTHIDE;";
            var meshList = hiddenMeshList.ItemsSource.OfType<HiddenMeshListEntry>().ToList();
            for (int i = 0; i < meshList.Count; i++)
            {
                if (meshList[i].Enabled == true)
                {
                    outParamVal += (i > 0 ? "," : "") + meshList[i].ID;
                }
            }

            CallFCBConverter("-xbgFP -xbg=\"" + fpXbgFile.Text + "\" -data=\"" + outParamVal + "\"");

            OpenInfoDialog("Fix XBG for FP", "XBG successfully edited.");
        }
        
        private async void FPXbgInfo_Click(object sender, RoutedEventArgs e)
        {
            string info = "";

            info += "Source XBG contains" + Environment.NewLine + Environment.NewLine;
            info += "Materials:" + Environment.NewLine;

            for (int i = 0; i < sourceXbgMatNames.Length; i++)
            {
                info += "  Material ID " + i.ToString() + Environment.NewLine;
                info += "    -Name: " + sourceXbgMatNames[i] + Environment.NewLine;
                info += "    -Path: " + sourceXbgMatPaths[i] + Environment.NewLine;
            }

            info += Environment.NewLine;

            info += "Number of skeleton IDs: " + sourceXbgSkel + Environment.NewLine + Environment.NewLine;

            info += "Count of materials in each skeleton ID:" + Environment.NewLine;

            for (int i = 0; i < sourceXbgSkel; i++)
            {
                info += "  -Skeleton " + i.ToString() + " contains " + sourceXbgSkelMats[i] + " materials" + Environment.NewLine;
                for (int j = 0; j < sourceXbgSkelMats[i]; j++)
                {
                    int faces = sourceXbgSkelFaces[i][j];
                    int verts = sourceXbgSkelVerts[i][j];
                    info += "    -material ID " + j.ToString() + " contains " + faces + " (" + (faces * 3) + ")" + " faces, " + verts + " verts" + Environment.NewLine;
                }
            }

            xbgInfoTB.Text = info;

            Animation(true, gridDialogXBGInfo);
        }

        private void LoadMeshParts()
        {
            hiddenMeshList.ItemsSource = null;

            var items = GetMeshParts();

            if (items != null)
            {
                List<HiddenMeshListEntry> a = new();

                foreach (var i in items)
                    a.Add(new() { ID = i.ID, Name = i.Name, Enabled = false });

                hiddenMeshList.ItemsSource = a;
            }
        }

        int sourceXbgSkel = 0;
        Dictionary<int, int> sourceXbgSkelMats;
        Dictionary<int, Dictionary<int, int>> sourceXbgSkelVerts;
        Dictionary<int, Dictionary<int, int>> sourceXbgSkelFaces;
        Dictionary<int, Dictionary<int, List<HiddenFacesListEntry>>> sourceXbgSkelHideFacesFP;
        string[] sourceXbgMatNames;
        string[] sourceXbgMatPaths;
        int hideFacesSelSkel = 0;
        int hideFacesSelMat = 0;

        private void LoadDataFromXBG(string file)
        {
            fpConvert.IsEnabled = false;
            fpXBGInfo.IsEnabled = false;
            fpAutoCalc.IsEnabled = false;

            var meshList = hiddenMeshList.ItemsSource.OfType<HiddenMeshListEntry>().ToList();
            foreach (var b in meshList)
                b.Enabled = false;
            hiddenMeshList.ItemsSource = meshList;

            hiddenFacesList.ItemsSource = new List<HiddenFacesListEntry>();
            
            fpSkelSel.Value = 0;
            fpSkelSel.Maximum = 0;
            fpMatSel.Value = 0;
            fpMatSel.Maximum = 0;

            if (!File.Exists(file))
                return;

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "FCBConverter.exe";
                process.StartInfo.Arguments = "-xbgData -xbg=\"" + file + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    if (line != null && line.StartsWith("data"))
                    {
                        string data = line.Replace("data", "");

                        string[] dataParse = data.Split('|');
                        foreach (string dP in dataParse)
                        {
                            string[] vals = dP.Split(';');
                            string[] valsData = vals[1].Split(',');

                            if (vals[0] == "MESHPARTHIDE")
                            {
                                foreach (string valsV in valsData)
                                {
                                    if (valsV != "" && valsV != "0")
                                    {
                                        var mp = GetMeshParts();
                                        var s = meshList.Where(a => a.ID == valsV)?.SingleOrDefault();
                                        if (s != null)
                                            s.Enabled = true;
                                    }
                                }
                            }

                            /*if (vals[0] == "FACEHIDEFP")
                            {
                                foreach (string valsV in valsData)
                                {
                                    if (valsV != "" && valsV != "0")
                                    {
                                        string[] partsV = valsV.Split('*');

                                        ListViewItem listViewItem = new ListViewItem();
                                        listViewItem.Text = GetMeshParts()[partsV[0]];
                                        listViewItem.SubItems.Add(partsV[1]);
                                        listViewItem.SubItems.Add(partsV[2]);
                                        listView1.Items.Add(listViewItem);
                                    }
                                }
                            }*/

                            if (vals[0] == "SKELFACES")
                            {
                                sourceXbgSkelFaces = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, int> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        matsArr.Add(j, int.Parse(matChilds[j]));
                                    }

                                    sourceXbgSkelFaces.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKELVERTS")
                            {
                                sourceXbgSkelVerts = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, int> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        matsArr.Add(j, int.Parse(matChilds[j]));
                                    }

                                    sourceXbgSkelVerts.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKELMATS")
                            {
                                sourceXbgSkelMats = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    sourceXbgSkelMats.Add(i, int.Parse(valsData[i]));
                                }
                            }

                            if (vals[0] == "SKELHIDEFACESFP")
                            {
                                sourceXbgSkelHideFacesFP = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, List<HiddenFacesListEntry>> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        List<HiddenFacesListEntry> hfSA = new();

                                        if (matChilds[j] != "" && matChilds[j] != "0")
                                        {
                                            string[] matParts = matChilds[j].Split('-');

                                            for (int k = 0; k < matParts.Length; k++)
                                            {
                                                string[] partData = matParts[k].Split('*');

                                                hfSA.Add(new()
                                                {
                                                    ID = partData[0],
                                                    Name = GetMeshParts().Single(a => a.ID == partData[0]).Name,
                                                    FaceStartIndex = ushort.Parse(partData[1]),
                                                    CountOfFaces = ushort.Parse(partData[2])
                                                });
                                            }
                                        }

                                        matsArr.Add(j, hfSA);
                                    }

                                    sourceXbgSkelHideFacesFP.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKEL")
                            {
                                sourceXbgSkel = int.Parse(vals[1]);
                            }

                            if (vals[0] == "MATS")
                            {
                                List<string> matNames = new List<string>();
                                List<string> matPaths = new List<string>();
                                foreach (string valsV in valsData)
                                {
                                    string[] partsV = valsV.Split('*');
                                    matNames.Add(partsV[0]);
                                    matPaths.Add(partsV[1]);
                                }
                                sourceXbgMatNames = matNames.ToArray();
                                sourceXbgMatPaths = matPaths.ToArray();
                            }
                        }
                    }
                }

                process.WaitForExit();

                fpConvert.IsEnabled = true;
                fpXBGInfo.IsEnabled = true;
                fpAutoCalc.IsEnabled = true;

                fpSkelSel.Value = 0;
                fpSkelSel.Maximum = sourceXbgSkel - 1;
                fpMatSel.Value = 0;
                fpMatSel.Maximum = sourceXbgSkelMats[(int)fpSkelSel.Value] - 1;

                hideFacesSelSkel = 0;
                hideFacesSelMat = 0;

                NumericsChangeSetData(0, 0);
            }
            catch (Exception)
            {
                OpenInfoDialog("Fix XBG for FP", "Error occured during opening XBG. Did you select right game?");
            }
        }

        private void NumericsChangeSetData(int skel, int material, int skelBef = -1, int matBef = -1)
        {
            if (skelBef != -1)
            {
                sourceXbgSkelHideFacesFP[skelBef][material] = hiddenFacesList.ItemsSource.OfType<HiddenFacesListEntry>().ToList();
            }
            else if (matBef != -1)
            {
                sourceXbgSkelHideFacesFP[skel][matBef] = hiddenFacesList.ItemsSource.OfType<HiddenFacesListEntry>().ToList();
            }

            hiddenFacesList.ItemsSource = sourceXbgSkelHideFacesFP[skel][material];

            hideFacesSelSkel = skel;
            hideFacesSelMat = material;
        }

        private void fpXbgFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDataFromXBG(fpXbgFile.Text);
        }

        private void FPSkelSel_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            NumericsChangeSetData((int)fpSkelSel.Value, 0, (int)e.OldValue);

            fpMatSel.Value = 0;
            fpMatSel.Maximum = sourceXbgSkelMats[(int)fpSkelSel.Value] - 1;
        }

        private void FPMatSel_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            NumericsChangeSetData((int)fpSkelSel.Value, (int)fpMatSel.Value, -1, (int)e.OldValue);
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