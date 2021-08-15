using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

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

        //google
        //private string versionFileLink = "https://drive.google.com/uc?export=download&id=1byyBIa-Wo1RXY1Ddv8Z_i7pic93dblsy";
        //private string zipFileLink = "https://drive.google.com/uc?export=download&id=1mMIbLpOjdu1YnmxktQxmKLSzHFK2GVr8";
        //bypass confirmation dialogue
        //private string zipFileLink = "https://drive.google.com/u/0/uc?export=download&confirm=H093&id=1mMIbLpOjdu1YnmxktQxmKLSzHFK2GVr8";

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

            if (File.Exists(versionFile)) {

                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                StateText.Text = "Checking for Updates";

                try {

                    WebClient webClient = new WebClient();
                    Version onlineVersion = 
                        new Version(webClient.DownloadString(versionFileLink));

                    if (onlineVersion.IsDifferentThan(localVersion)) {

                        StateText.Text = $"New update available ({onlineVersion})";
                        InstallGameFiles(true, onlineVersion);

                    } else {

                        Status = LauncherStatus.ready;
                    }

                } catch (Exception e) {

                    Status = LauncherStatus.failed;
                    StateText.Text = $"Check update failed: {e.Message}.";
                }
            } else {

                StateText.Text = "No installed files detected.";
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion) {

            try {

                WebClient webClient = new WebClient();

                if (_isUpdate) {

                    Status = LauncherStatus.downloadingUpdate;

                } else {

                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString(versionFileLink));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri(zipFileLink), gameZip, _onlineVersion);

            } catch (Exception e) {

                Status = LauncherStatus.failed;
                StateText.Text = $"Install failed: {e.Message}";
            }
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

            //StateText.Text =  $"Looking for {gameExe}";

            if(File.Exists(gameExe) && Status == LauncherStatus.ready) {

                StateText.Text = "All set to launch the game";

                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "conquest-remastered-build");
                Process.Start(startInfo);

                Close();

            } else {

                StateText.Text = "Encontered a problem executing the game file.";

                //if (Status == LauncherStatus.failed) {
                //
                //    CheckForUpdates();
                //}
            }
        }
    }

    struct Version {

        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor) {

            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        internal Version(string _version) {

            string[] _versionStrings = _version.Split('.');

            if(_versionStrings.Length != 3) {

                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion) {

            if(major != _otherVersion.major) {

                return true;

            } else {

                if(minor != _otherVersion.minor) {
                    return true;

                } else {

                    if (subMinor != _otherVersion.subMinor)
                        return true;
                }
            }

            return false;
        }

        public override string ToString() {

            return $"{major}.{minor}.{subMinor}";
        }
    }
}
