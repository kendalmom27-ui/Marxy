using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using IOPath = System.IO.Path;

namespace RasTweaksCS
{
    public partial class UpdateCheckWindow : Window
    {
        private DispatcherTimer? _spinnerTimer;

        public UpdateCheckWindow()
        {
            InitializeComponent();
            StartSpinner();
        }

        /// <summary>Returns true if an update was downloaded and applied (caller should shut down immediately).</summary>
        public async Task<bool> RunUpdateCheckAsync()
        {
            UpdateInfo? info;
            try
            {
                info = await UpdateChecker.CheckForUpdateAsync();
            }
            catch
            {
                // Offline, GitHub unreachable, rate-limited, malformed response, etc. -
                // never let a failed update check block the app from starting.
                info = null;
            }

            if (info == null)
            {
                StopSpinner();
                return false;
            }

            StatusText.Text = $"Downloading update v{info.Version}...";
            ProgressTrack.Visibility = Visibility.Visible;

            var tempPath = IOPath.Combine(IOPath.GetTempPath(), "RasTweaksCS_update.exe");

            try
            {
                var progress = new Progress<double>(pct =>
                {
                    ProgressScale.ScaleX = Math.Clamp(pct / 100.0, 0.0, 1.0);
                    StatusText.Text = $"Downloading update v{info.Version}... {(int)pct}%";
                });

                await UpdateChecker.DownloadUpdateAsync(info.DownloadUrl, tempPath, progress);
            }
            catch
            {
                StopSpinner();
                return false;
            }

            StatusText.Text = "Installing update...";
            StopSpinner();

            UpdateChecker.ApplyUpdateAndRestart(tempPath);
            return true;
        }

        private void StartSpinner()
        {
            _spinnerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _spinnerTimer.Tick += (s, e) => SpinnerRotate.Angle = (SpinnerRotate.Angle + 6) % 360;
            _spinnerTimer.Start();
        }

        private void StopSpinner()
        {
            _spinnerTimer?.Stop();
            _spinnerTimer = null;
        }
    }
}
