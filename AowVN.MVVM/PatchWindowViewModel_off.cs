using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using SevenZip;
using System;
using AowVN.MVVM.Helper;
using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace AowVN.MVVM
{
    public partial class PatchWindowViewModel_off : ObservableRecipient
    {
        #region Callbool
        [ObservableProperty]
        private bool playAudio = true, shortcut = false, statusVersion = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PatchButtonEnabled))]
        private bool statusButton = false;

        [ObservableProperty]
        private bool statusButtonH = false;

        [ObservableProperty]
        private bool installing = false;

        [ObservableProperty]
        public bool textBrowse = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PatchButtonEnabled))]
        private bool? validDirectory = null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Enable))]
        [NotifyPropertyChangedFor(nameof(PatchButtonEnabled))]
        private bool exiting = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Enable))]
        [NotifyPropertyChangedFor(nameof(PatchButtonEnabled))]
        private bool working = false;

        public bool Enable => !(Working || Exiting);
        public bool PatchButtonEnabled => Enable && StatusButton;


        #endregion

        #region Callstring
        [ObservableProperty]
        private static Dictionary<string, string> versionLinks = new Dictionary<string, string>();

        [ObservableProperty]
        private string patchDirectory = "", textLogColor = "Cyan", folderGameName = "SANABI", selectVersion, gameVersion, selectVersionV, audioName = "BGM.aow";

        [ObservableProperty]
        private string fileName = "", sourceCredit = "", sourceTitle = "/Assets/Image/TitleAnimation_frames/titleanimation_000_optimized.png", textLog = "Patch Việt Hóa";

        [ObservableProperty]
        private string fileDownName = "AowVN.aow", patchVersion = "v1.3.31";

        [ObservableProperty]
        private string sourceButton = "", sourceButtonH = "", sourceDirectoryBox = "", sourceInstallingCP = "", sourceInstallingst = "", strprogressVariable = "";

        [ObservableProperty]
        private string extractLog = "Vui lòng chọn đường dẫn tới tập tin \"SNB.exe\"!"; //File
                                                                                        //private string extractLog = "Vui lòng chọn đường dẫn đến nơi muốn chứa dữ liệu game!"; //Folder 
        #endregion

        [ObservableProperty]
        private double zoomTitleH = 300, zoomTitleW = 580;

        [ObservableProperty]
        private static List<string> listVersion = new List<string>();

        private readonly OpenFileDialog folderDialog = new OpenFileDialog(); //file
                                                                             //private readonly IOpenFolderService folderDialog = Ioc.Default.GetService<IOpenFolderService>()!; //folder

        public PatchWindowViewModel_off()
        {
            BeatTitle();
            AnimationDirectoryBox();
            AnimationCredit();
        }

        partial void OnPlayAudioChanged(bool value)
        {
            if (value)
            {
                AudioPlayer.Resume();
                BeatTitle();
            }
            else
            {
                AudioPlayer.Pause();
            }
        }

        public void CheckVersion()
        {
            string filePath = PatchDirectory + "/SNB_Data/globalgamemanagers"; //Đường dẫn tới file cần đọc

            List<string> versionsInFile = GetVersion.GetVersionsInFile(filePath);
            foreach (string version in versionsInFile)
            {
                GameVersion = version;
                if (version == PatchVersion)
                {
                    ExtractLog = "";
                    StatusVersion = true;
                }
            }
            if (!StatusVersion)
            {
                ExtractLog = "Game đang sử dụng phiên bản " + GameVersion + "!";
                MessageBox.Show("Game đang sử dụng phiên bản " + GameVersion + "\nKhông thể cài do bộ cài việt hóa chỉ hỗ trợ phiên bản " + PatchVersion + ". Vui lòng cài đúng phiên bản trò chơi!", "Lưu ý!");
            }

        }

        [RelayCommand]
        private async Task InstallPatch(CancellationToken token)
        {
            Working = true;
            //await AnimationDownloadingCP(token);
            await AnimationInstallingCP(token);

        }

        #region Directory
        /*
        [RelayCommand] // Folder
        private void GetDirectory()
        {
            if (folderDialog.Show() == true)
            {
                TextBrowse = true;
                PatchDirectory = folderDialog.Result;
                FileName = PatchDirectory + "\\" + FolderGameName;
            }
        }
        */
        //File
        [RelayCommand]
        private void GetDirectory()
        {
            folderDialog.Filter = "Tập tin (.exe)|*.exe";
            folderDialog.Title = "Chọn đường dẫn tới \"SNB.exe\" trong thư mục game!";
            folderDialog.ShowDialog();
            if (folderDialog.CheckPathExists == true)
            {
                TextBrowse = true;
                FileName = folderDialog.FileName;

            }
        }
        #endregion

        #region OnChanging
        [RelayCommand]
        partial void OnPatchDirectoryChanging(string value) => ExtractLog = "";

        [RelayCommand]
        partial void OnFileNameChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ValidDirectory = null;
                TextBrowse = false;
            }
            else
            {
                PatchDirectory = Path.GetDirectoryName(FileName);
                if (value == Path.Combine(PatchDirectory, ConstantStrings.GameExecutable) && Directory.Exists(Path.Combine(PatchDirectory, ConstantStrings.FolderData))) //File
                { ValidDirectory = true; }
                else { ValidDirectory = false; } //File
                                                 //ValidDirectory = true; //Folder
                if (ValidDirectory == true)
                {
                    CheckVersion();
                    if (value != "Chọn phiên bản...")
                    {
                        AnimationButtonAppear();
                        StatusButtonH = true;
                        AnimationButtonHovered();
                    }
                }
                else
                {
                    AnimationButtonDisppear();
                    StatusButtonH = false;
                }
            }
        }

        #endregion

        #region AnimationDisplay
        private int ButtonNum = 0;
        public async void AnimationButtonAppear()
        {
            while (ButtonNum < 90)
            {
                SourceButton = "/Assets/Image/CaiPatch1_frames/caipatch1_00" + ButtonNum + "_optimized.png";
                await Task.Delay(16);
                ButtonNum++;
                if (ValidDirectory == false || !StatusVersion)
                    break;
            }
            if (StatusButton == false && ButtonNum == 90)
            {
                StatusButton = true;
            }
        }

        public async void AnimationButtonDisppear()
        {
            if (StatusButton == true)
            {
                StatusButton = false;
            }
            while (ButtonNum >= 0)
            {
                SourceButton = "/Assets/Image/CaiPatch1_frames/caipatch1_00" + ButtonNum + "_optimized.png";
                await Task.Delay(16);
                ButtonNum--;
                if (ValidDirectory == true && StatusVersion)
                    break;
            }
        }


        public async void AnimationButtonHovered()
        {
            while (StatusButtonH == true)
            {
                for (byte i = 1; i <= 90; i++)
                {
                    SourceButtonH = "/Assets/Image/CaiPatch2_frames/caipatch2_00" + i + "_optimized.png";
                    await Task.Delay(16);
                }
            }
        }



        public async void AnimationDirectoryBox()
        {

            for (byte i = 1; i <= 60; i++)
            {
                SourceDirectoryBox = "/Assets/Image/DirectoryBox_frames/DirectoryBox_00" + i + ".png";
                await Task.Delay(32);
            }
            await Task.Delay(32);
            AnimationDirectoryBox();
        }

        #endregion

        #region AnimationInstall
        public async Task AnimationInstallingCP(CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            for (byte i = 1; i <= 42; i++)
            {
                SourceInstallingCP = "/Assets/Image/InstallingCP_frames/installingcp_00" + i + "_optimized.png";
                SourceButton = SourceInstallingCP;
                await Task.Delay(16);
            }
            var animationstTask = Task.Run(async () => await AnimationInstallingst());
            Installing = true;
            var extractionTask = Task.Run(async () =>
            {
                using Stream stream = AowVN.MVVM.Resource.PATCH;
                //using var memoryStream = new MemoryStream(AowVN.MVVM.Resource.PATCH);
                using var reader = new SevenZipExtractor(stream, null, true);
                await reader.ExtractToDirectoryAsync(PatchDirectory, cts.Token, new Progress<FileExtractedArgs>(file => ExtractLog = file.ToString())).ConfigureAwait(false);
                if (Shortcut)
                    ShortcutFile.ShortcutFileA(FileName);
            });


            await Task.WhenAll(extractionTask, animationstTask);
            File.Delete(Path.Combine(PatchDirectory, ConstantStrings.FolderData, FileDownName));
            AnimationDoneInstall();
        }


        public async Task AnimationInstallingst()
        {
            byte ProgressVariable = 0;
            while (Installing)
            {
                byte percentVariable = ArchiveReaderHelper.GetPercentVariable();
                while (ProgressVariable < percentVariable)
                {
                    SourceInstallingst = "/Assets/Image/Installingst_frames/installingst_00" + ProgressVariable + "_optimized.png";
                    await Task.Delay(16);
                    ProgressVariable += 1;
                    StrprogressVariable = ProgressVariable.ToString() + "%";
                }
                if (ProgressVariable == 100)
                {
                    Installing = false;
                    break;
                }
            }
        }

        public async void AnimationDoneInstall()
        {
            var DoneInstall = Task.Run(async () =>
            {
                for (int i = 100; i >= 0; i--)
                {
                    SourceInstallingst = "/Assets/Image/Installingst_frames/installingst_00" + i + "_optimized.png";
                    await Task.Delay(10);
                }
                SourceInstallingst = "";
            });
            var DoneInstall2 = Task.Run(async () =>
            {
                for (byte i = 90; i >= 1; i--)
                {
                    SourceButton = "/Assets/Image/InstallingCP_frames/installingcp_00" + i + "_optimized.png";
                    await Task.Delay(16);
                }
            });
            for (int i = StrprogressVariable.Length - 1; i >= 0; i--)
            {
                StrprogressVariable = StrprogressVariable.Remove(i, 1);
                await Task.Delay(50);
            }
            foreach (char c in "XONG")
            {
                StrprogressVariable += c;
                await Task.Delay(50);
            }
            await Task.Delay(500);
            for (int i = StrprogressVariable.Length - 1; i >= 0; i--)
            {
                StrprogressVariable = StrprogressVariable.Remove(i, 1);
                await Task.Delay(50);
            }
            await Task.WhenAll(DoneInstall, DoneInstall2);
            ExtractLog = "Cài đặt thành công! Thưởng thức trò chơi thôi nào~!";

            string filePath = PatchDirectory + "/SNB_Data/globalgamemanagers"; //Đường dẫn tới file cần đọc

            List<string> versionsInFile = GetVersion.GetVersionsInFile(filePath);
            List<string> versionLink = new List<string>();
            foreach (KeyValuePair<string, string> pair2 in versionLinks)
            {
                MatchCollection matches = Regex.Matches(pair2.Key, @"\bv1+\.\d+\.\d+\b");
                foreach (Match match in matches)
                {
                    versionLink.Add(match.Value);
                    foreach (string version in versionsInFile)
                    {
                        GameVersion = version;
                    }
                }

            }
            StatusButtonH = false;
            while (ButtonNum >= 0)
            {
                SourceButton = "/Assets/Image/CaiPatch1_frames/caipatch1_00" + ButtonNum + "_optimized.png";
                await Task.Delay(16);
                ButtonNum--;
            }
            Working = false;
        }
        #endregion

        #region Credit
        public async void AnimationCredit()
        {
            while (true)
            {
                foreach (char c in "DỊCH THUẬT: PIKA RIMU")
                {
                    SourceCredit += c;
                    await Task.Delay(50);
                }
                await Task.Delay(2000);
                for (int i = SourceCredit.Length - 1; i >= 0; i--)
                {
                    SourceCredit = SourceCredit.Remove(i, 1);
                    await Task.Delay(50);
                }
                foreach (char c in "KĨ THUẬT: PIKA RIMU")
                {
                    SourceCredit += c;
                    await Task.Delay(50);
                }
                await Task.Delay(2000);
                for (int i = SourceCredit.Length - 1; i >= 0; i--)
                {
                    SourceCredit = SourceCredit.Remove(i, 1);
                    await Task.Delay(50);
                }
                foreach (char c in "TESTER: ROSETTA")
                {
                    SourceCredit += c;
                    await Task.Delay(50);
                }
                await Task.Delay(2000);
                for (int i = SourceCredit.Length - 1; i >= 0; i--)
                {
                    SourceCredit = SourceCredit.Remove(i, 1);
                    await Task.Delay(50);
                }
            }
        }
        #endregion

        #region Title
        private int ititle = 0, jtitle = 0, wtitle = 0, ctitle = 7;
        bool StartBeatTitle = false, StartAudio = false;
        public void BeatTitle()
        {
            Task.Run(async () =>
            {
                if (StartAudio == false)
                {
                    StartAudio = true;
                    Bass.Init();
                    AudioPlayer.Play(AudioName);

                }
                while (PlayAudio)
                {

                    while (wtitle < 39)
                    {
                        await Task.Delay(1);
                        wtitle += 1;
                        if (!PlayAudio)
                            break;
                    }
                    if (!PlayAudio)
                        break;
                    wtitle = 0;
                    ZoomTitleH = 300 + 4 + jtitle;
                    ZoomTitleW = 580 + (4 + jtitle) * (580 / 300);

                    if (StartBeatTitle == false)
                    {
                        StartBeatTitle = true;
                        var SBeatTitle = Task.Run(async () =>
                        {
                            while (true)
                            {

                                if (ZoomTitleH > 300)
                                {
                                    ZoomTitleH -= 0.5;
                                    ZoomTitleW -= 0.5 * (580 / 300);
                                }
                                await Task.Delay(1);
                            }

                        });


                    }

                    ititle++;
                    if (ititle == 31)
                        jtitle = 6;
                    if (ititle == 252)
                        jtitle = 0;
                    if (ititle == 258)
                        jtitle = -4;
                    if (ititle == 260)
                    {
                        await Task.Delay(3800);
                        ititle = 0;
                        jtitle = 0;
                        ctitle = 7;
                    }
                    if (ititle >= 31 && ititle <= 251)
                    {

                        if (ctitle == 8)
                        {
                            ctitle = 0;
                            TitleAnimation();
                        }
                        ctitle++;
                    }
                }
            });
        }

        public bool TitleChangeType = false;
        public async void TitleAnimation()
        {
            if (!TitleChangeType)
            {
                TitleChangeType = true;
                for (byte i = 1; i <= 57; i++)
                {
                    SourceTitle = "/Assets/Image/TitleAnimation_frames/titleanimation_00" + i + "_optimized.png";
                    await Task.Delay(1);
                }
            }
            else
            {
                TitleChangeType = false;
                for (int i = 57; i >= 0; i--)
                {
                    SourceTitle = "/Assets/Image/TitleAnimation_frames/titleanimation_00" + i + "_optimized.png";
                    await Task.Delay(1);
                }
            }
        }
        #endregion

        

        [RelayCommand]
        private async void Exit()
        {
            Working = true;
            for (int i = TextLog.Length - 1; i >= 0; i--)
            {
                TextLog = TextLog.Remove(i, 1);
                await Task.Delay(10);
            }
            TextLogColor = "Orange";
            foreach (char c in "Tạm biệt nhé~!")
            {
                TextLog += c;
                await Task.Delay(30);
            }
            
            await Task.Delay(500);
            AudioPlayer.Stop(AudioName);
            Exiting = true;
        }
    }
}

