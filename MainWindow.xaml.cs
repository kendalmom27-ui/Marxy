using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IOPath = System.IO.Path;
using System.Management;
using System.Reflection;

namespace RasTweaksCS;

public partial class MainWindow : Window
{
    private static readonly string[] StartupLoadingMessages =
    {
        "Loading assets...",
        "Loading more assets...",
        "Talking to my dog...",
        "Almost done...",
        "Oops, I slipped...",
        "Almost done again..."
    };

    private TweakRunner? _tweakRunner;
    private List<TweakInfo>? _allTweaks;
    private bool _isTransitioning = false;
    private DispatcherTimer? _startupSpinnerTimer;
    private DispatcherTimer? _startupMessageTimer;

    public MainWindow()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            File.WriteAllText("crashlog.txt", $"FATAL CRASH:\n{ex?.Message}\n\n{ex?.StackTrace}");
        };
        
        this.Dispatcher.UnhandledException += (s, e) =>
        {
            File.WriteAllText("crashlog.txt", $"UI CRASH:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}");
            e.Handled = true;
        };

        try
        {
            InitializeComponent();
            InitializeTweaks();
            
            HomeView.Visibility = Visibility.Visible;
            TweakListView.Visibility = Visibility.Collapsed;
            HeaderIcon.Text = "🏠";
            CategoryTitle.Text = "System Information";
            CategoryDescription.Text = "Your current system specifications and status";
            LoadSystemInfo();

            RunStartupRestorePointSequence();
        }
        catch (Exception ex)
        {
            File.WriteAllText("crashlog.txt", $"INIT CRASH:\n{ex.Message}\n\n{ex.StackTrace}");
            MessageBox.Show($"Error initializing window: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeTweaks()
    {
        try
        {
            var tweaksPath = ExtractEmbeddedTweaks();
            _tweakRunner = new TweakRunner(tweaksPath);
            _allTweaks = TweakRegistry.GetAllTweaks();
            UpdateSidebarTooltips();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing tweaks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// All tweak scripts are embedded inside the exe (see EmbeddedResource in the
    /// csproj) so the app can ship as a single file. This unpacks them to disk on
    /// every launch, since cmd.exe/powershell.exe need a real file to run - always
    /// overwriting keeps the extracted copies in sync with whatever this exe build
    /// actually contains, instead of a stale cache from a previous version.
    /// </summary>
    private static string ExtractEmbeddedTweaks()
    {
        var targetDir = IOPath.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RasTweaksCS", "Tweaks");

        Directory.CreateDirectory(targetDir);

        var assembly = Assembly.GetExecutingAssembly();
        const string prefix = "Tweaks/";

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var fileName = resourceName.Substring(prefix.Length);
            var destPath = IOPath.Combine(targetDir, fileName);

            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                continue;
            }

            using var fileStream = File.Create(destPath);
            resourceStream.CopyTo(fileStream);
        }

        return targetDir;
    }

    private void UpdateSidebarTooltips()
    {
        if (_allTweaks == null) return;

        var counts = _allTweaks.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count());

        void SetCount(Button button, string category)
        {
            var count = counts.TryGetValue(category, out var c) ? c : 0;
            button.ToolTip = $"{category} ({count} tweak{(count == 1 ? "" : "s")})";
        }

        SetCount(NetworkNavBtn, "Network");
        SetCount(PowerNavBtn, "Power");
        SetCount(BootNavBtn, "Boot");
        SetCount(SystemNavBtn, "System");
        SetCount(KernelNavBtn, "Kernel");
        SetCount(GpuNavBtn, "GPU");
        SetCount(AimNavBtn, "Aim");
    }

    private async void RunStartupRestorePointSequence()
    {
        StartStartupSpinner();

        var messageIndex = 0;
        StartupStatusText.Text = StartupLoadingMessages[0];

        _startupMessageTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
        _startupMessageTimer.Tick += (s, e) =>
        {
            messageIndex = (messageIndex + 1) % StartupLoadingMessages.Length;
            StartupStatusText.Text = StartupLoadingMessages[messageIndex];
        };
        _startupMessageTimer.Start();

        string finalMessage;

        if (_tweakRunner != null)
        {
            var result = await Task.Run(() => _tweakRunner.RunPowerShellScriptAsync("create-restore-point"));

            if (result.Success)
            {
                finalMessage = result.Output.Contains("STATUS:SKIPPED_RECENT")
                    ? "Already protected - skipping!"
                    : "Done!";
            }
            else
            {
                finalMessage = "Couldn't create a restore point, continuing anyway...";
            }
        }
        else
        {
            finalMessage = "Done!";
        }

        _startupMessageTimer.Stop();
        _startupMessageTimer = null;

        StartupStatusText.Text = finalMessage;

        await Task.Delay(900);

        StopStartupSpinner();
        StartupOverlay.Visibility = Visibility.Collapsed;
    }

    private void StartStartupSpinner()
    {
        _startupSpinnerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _startupSpinnerTimer.Tick += (s, e) =>
        {
            StartupSpinnerRotate.Angle = (StartupSpinnerRotate.Angle + 6) % 360;
        };
        _startupSpinnerTimer.Start();
    }

    private void StopStartupSpinner()
    {
        if (_startupSpinnerTimer != null)
        {
            _startupSpinnerTimer.Stop();
            _startupSpinnerTimer = null;
        }
    }

    private async void ShowAllTweaks()
    {
        if (_isTransitioning) return;
        
        try
        {
            _isTransitioning = true;
            var count = _allTweaks?.Count ?? 0;
            await FadeViewTransition(HomeView, TweakListView, "📋", "All Tweaks", $"{count} tweak{(count == 1 ? "" : "s")} across every category");
            TweakList.ItemsSource = _allTweaks;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async void ShowCategory(string category)
    {
        if (_isTransitioning) return;
        
        try
        {
            _isTransitioning = true;
            var count = _allTweaks?.Count(t => t.Category == category) ?? 0;
            await FadeViewTransition(HomeView, TweakListView, GetCategoryIcon(category), category, $"{count} tweak{(count == 1 ? "" : "s")} in this category");

            if (_allTweaks != null)
            {
                var categoryTweaks = _allTweaks.Where(t => t.Category == category).ToList();
                TweakList.ItemsSource = categoryTweaks;
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async void OnHomeClick(object sender, RoutedEventArgs e)
    {
        if (_isTransitioning) return;

        SetActiveNav(sender as Button);

        try
        {
            _isTransitioning = true;
            await FadeViewTransition(TweakListView, HomeView, "🏠", "System Information", "Your current system specifications and status");
            LoadSystemInfo();
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private static string GetCategoryIcon(string category) => category switch
    {
        "Network" => "🌐",
        "Power" => "⚡",
        "Boot" => "🖥️",
        "System" => "🔧",
        "Kernel" => "💻",
        "GPU" => "🎮",
        "Aim" => "🎯",
        _ => "✨",
    };

    private async Task FadeViewTransition(FrameworkElement fromView, FrameworkElement toView, string icon, string title, string description)
    {
        if (fromView == null || toView == null) return;

        fromView.Opacity = 1.0;

        for (double i = 1; i >= 0; i -= 0.1)
        {
            fromView.Opacity = i;
            await Task.Delay(15);
        }

        fromView.Visibility = Visibility.Collapsed;
        toView.Visibility = Visibility.Visible;
        toView.Opacity = 0;

        HeaderIcon.Text = icon;
        CategoryTitle.Text = title;
        CategoryDescription.Text = description;
        RefreshInfoButton.Visibility = toView == HomeView ? Visibility.Visible : Visibility.Collapsed;

        for (double i = 0; i <= 1; i += 0.1)
        {
            toView.Opacity = i;
            await Task.Delay(15);
        }
        
        toView.Opacity = 1.0;
    }

    private void SetActiveNav(Button? active)
    {
        foreach (var button in new[] { HomeNavBtn, AllTweaksNavBtn, NetworkNavBtn, PowerNavBtn, BootNavBtn, SystemNavBtn, KernelNavBtn, GpuNavBtn, AimNavBtn })
        {
            button.Tag = null;
        }

        if (active != null)
        {
            active.Tag = "Active";
        }
    }

    private void OnAllTweaksClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowAllTweaks();
    }

    private void OnNetworkClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("Network");
    }

    private void OnPowerClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("Power");
    }

    private void OnBootClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("Boot");
    }

    private void OnSystemClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("System");
    }

    private void OnKernelClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("Kernel");
    }

    private void OnGpuClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("GPU");
    }

    private void OnAimClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        ShowCategory("Aim");
    }

    private async void OnApplyTweak(object sender, RoutedEventArgs e)
    {
        Button? button = null;
        object? originalContent = null;

        try
        {
            button = sender as Button;
            if (button == null) return;

            var tweakKey = button.Tag as string;
            if (string.IsNullOrEmpty(tweakKey)) return;

            if (_tweakRunner == null)
            {
                MessageBox.Show("Tweak runner not initialized", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            originalContent = button.Content;
            button.IsEnabled = false;
            button.Content = "Running...";

            var result = await Task.Run(() => _tweakRunner.RunTweakAsync(tweakKey));

            var notification = new TweakNotificationWindow();
            notification.Owner = this;

            if (result.Success)
            {
                notification.SetSuccessState("Success", "Tweak applied successfully!");
            }
            else
            {
                notification.SetErrorState("Tweak Failed", result.Message);
            }

            notification.Show();
        }
        catch (Exception ex)
        {
            File.WriteAllText("crashlog.txt", $"TWEAK CRASH:\n{ex.Message}\n\n{ex.StackTrace}");

            var notification = new TweakNotificationWindow();
            notification.Owner = this;
            notification.SetErrorState("Error", $"Error applying tweak: {ex.Message}");
            notification.Show();
        }
        finally
        {
            if (button != null)
            {
                button.IsEnabled = true;
                button.Content = originalContent ?? "Apply";
            }
        }
    }

    private void OnRefreshSystemInfo(object sender, RoutedEventArgs e)
    {
        var spin = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(500));
        RefreshIconRotate.BeginAnimation(RotateTransform.AngleProperty, spin);
        LoadSystemInfo();
    }

    private void OnDiscordClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://discord.gg/bat5hHZSt") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Couldn't open the Discord link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadSystemInfo()
    {
        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var cpuInfo = cpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (cpuInfo != null)
            {
                CpuInfo.Text = $"Name: {cpuInfo["Name"]}\nCores: {cpuInfo["NumberOfCores"]}\nMax Clock: {cpuInfo["MaxClockSpeed"]} MHz";
            }

            using var ramSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            var ramInfo = ramSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (ramInfo != null)
            {
                ulong totalRam = Convert.ToUInt64(ramInfo["TotalPhysicalMemory"]);
                RamInfo.Text = $"Total RAM: {totalRam / (1024 * 1024 * 1024)} GB";
            }

            using var gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            var gpuInfo = gpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (gpuInfo != null)
            {
                var adapterRam = gpuInfo["AdapterRAM"];
                if (adapterRam != null)
                {
                    ulong vramBytes = Convert.ToUInt64(adapterRam);
                    GpuInfo.Text = $"GPU: {gpuInfo["Name"]}\nVRAM: {vramBytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
                }
                else
                {
                    GpuInfo.Text = $"GPU: {gpuInfo["Name"]}\nVRAM: Unknown";
                }
            }

            using var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var osInfo = osSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (osInfo != null)
            {
                SystemInfo.Text = $"OS: {osInfo["Caption"]}\nVersion: {osInfo["Version"]}\nBuild: {osInfo["BuildNumber"]}";
            }

            using var driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'");
            var driveInfo = driveSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (driveInfo != null)
            {
                var freeSpace = driveInfo["FreeSpace"];
                var totalSpace = driveInfo["Size"];
                
                if (freeSpace != null && totalSpace != null)
                {
                    ulong freeBytes = Convert.ToUInt64(freeSpace);
                    ulong totalBytes = Convert.ToUInt64(totalSpace);
                    StorageInfo.Text = $"C: Drive\nFree: {freeBytes / (1024.0 * 1024.0 * 1024.0):F1} GB\nTotal: {totalBytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
                }
                else
                {
                    StorageInfo.Text = "C: Drive\nInformation unavailable";
                }
            }

            using var networkSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=true");
            var networkInfo = networkSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (networkInfo != null)
            {
                var speed = networkInfo["Speed"];
                if (speed != null)
                {
                    ulong speedBps = Convert.ToUInt64(speed);
                    string speedDisplay;
                    if (speedBps >= 1000000000)
                        speedDisplay = $"{speedBps / 1000000000.0:F1} Gbps";
                    else if (speedBps >= 1000000)
                        speedDisplay = $"{speedBps / 1000000.0:F1} Mbps";
                    else
                        speedDisplay = $"{speedBps} bps";
                    
                    NetworkInfo.Text = $"Adapter: {networkInfo["Name"]}\nSpeed: {speedDisplay}";
                }
                else
                {
                    NetworkInfo.Text = $"Adapter: {networkInfo["Name"]}\nSpeed: Unknown";
                }
            }
        }
        catch (Exception ex)
        {
            CpuInfo.Text = $"Error loading system info: {ex.Message}";
        }
    }

    private void OnMinimize(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void OnTitlebarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }
}