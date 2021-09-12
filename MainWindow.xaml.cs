using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace game_launcher {

    enum LauncherStatus {

        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        //onedrive
        private string versionFileLink = "https://onedrive.live.com/download?cid=105D6E48BBD23BD7&resid=105D6E48BBD23BD7%21597337&authkey=AFEUEVm9dac5UZY";
        private string zipFileLink = "https://onedrive.live.com/download?cid=105D6E48BBD23BD7&resid=105D6E48BBD23BD7%21597340&authkey=AI04J84cRNQRI44";

        private LauncherStatus _status;

        internal LauncherStatus Status {
            get => _status;
            set {

                _status = value;

                switch (_status) {

                    case LauncherStatus.ready:
                        StateText.Text = "Ready to launch the game";
                        InitializeGame();
                        break;
                    case LauncherStatus.failed:
                        StateText.Text = "Update failed";
                        break;
                    case LauncherStatus.downloadingGame:
                        StateText.Text = "Downloading game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        StateText.Text = "Downloading update";
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow() {

            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "conquest-remastered-build.zip");
            gameExe = Path.Combine(rootPath, "conquest-remastered-build", "conquest-remastered.exe");
        }

        private void CheckForUpdates() {

            try {

                StateText.Text = "Checking game files";

                WebClient webClient = new WebClient();

                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(CheckForUpdatesCallback);
                webClient.DownloadStringAsync(new Uri(versionFileLink));

            }
            catch (Exception e)
            {

                Status = LauncherStatus.failed;
                StateText.Text = $"Check update failed: {e.Message}.";
            }

            
        }

        private void CheckForUpdatesCallback(object sender, DownloadStringCompletedEventArgs e) {

            Version onlineVersion =
                        new Version(e.Result);

            if (File.Exists(versionFile)) {

                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                if (onlineVersion.IsDifferentThan(localVersion)) {

                    StateText.Text = $"New update available ({onlineVersion})";
                    InstallGameFiles(true, onlineVersion);

                } else {

                    Status = LauncherStatus.ready;
                }

            }  else {

                StateText.Text = "No installed files detected.";
                InstallGameFiles(false, onlineVersion);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion) {

            try {

                WebClient webClient = new WebClient();

                if (_isUpdate) {

                    Status = LauncherStatus.downloadingUpdate;
                    StateText.Text = $"Updating to v{_onlineVersion}...";

                } else {

                    Status = LauncherStatus.downloadingGame;
                    StateText.Text = $"Downloading v{_onlineVersion}...";
                    //_onlineVersion = new Version(webClient.DownloadString(versionFileLink));
                }

                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(PrintProgress);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);

                //Status = _isUpdate ? LauncherStatus.downloadingUpdate : LauncherStatus.downloadingGame;
                ProgressText.Text = "Retrieving information...";
                webClient.DownloadFileAsync(new Uri(zipFileLink), gameZip, _onlineVersion);

            } catch (Exception e) {

                Status = LauncherStatus.failed;
                StateText.Text = $"Install failed: {e.Message}";
            }
        }

        private void PrintProgress(object sender, DownloadProgressChangedEventArgs e) {

            ProgressText.Text = 
                ((e.BytesReceived / 1024f) / 1024f).ToString("0.00MB") + "/" + ((e.TotalBytesToReceive / 1024f) / 1024f).ToString("0.00MB");
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) {

            StateText.Text = $"Downloaded version {((Version)e.UserState).ToString()}, extracting...";

            try {

                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;

            } catch (Exception _e) {

                StateText.Text = $"Failed to extract: {_e.Message}";
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e) {

            StateText.Text = "Initializing launcher";
            CheckForUpdates();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {

            Close();
        }

        private void InitializeGame() {

            if(File.Exists(gameExe) && Status == LauncherStatus.ready) {

                StateText.Text = "All set to launch the game";

                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "conquest-remastered-build");
                Process.Start(startInfo);

                Close();

            } else {

                StateText.Text = "Encontered a problem executing the game file.";
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
