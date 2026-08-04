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
    private FrameworkElement _currentView = null!;

    // Live usage monitor state
    private const int MonitorMaxSamples = 60;
    private DispatcherTimer? _monitorTimer;
    private readonly List<double> _cpuHistory = new();
    private readonly List<double> _ramHistory = new();
    private string _activeMonitor = "CPU";
    private string _cpuName = "CPU";
    private bool _monitorSampling;

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
            CreditsView.Visibility = Visibility.Collapsed;
            _currentView = HomeView;
            HeaderIcon.Text = "🏠";
            CategoryTitle.Text = "System Information";
            CategoryDescription.Text = "Your current system specifications and status";
            LoadSystemInfo();
            StartUsageMonitor();

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
    /// All tweak scripts are embedded inside the exe as AES-encrypted blobs (see
    /// the EncryptTweaks target in the csproj) so the shipped exe never carries
    /// readable .bat/.ps1 source. This decrypts and unpacks them to disk on every
    /// launch, since cmd.exe/powershell.exe need real files to run - always
    /// overwriting keeps the extracted copies in sync with this exact build rather
    /// than a stale cache from a previous version.
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

            // Resource "Tweaks/foo.bat" holds the AES-encrypted bytes of foo.bat.
            var fileName = resourceName.Substring(prefix.Length);
            var destPath = IOPath.Combine(targetDir, fileName);

            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                continue;
            }

            using var buffer = new MemoryStream();
            resourceStream.CopyTo(buffer);
            var decrypted = TweakCrypto.Decrypt(buffer.ToArray());
            File.WriteAllBytes(destPath, decrypted);
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
            await FadeViewTransition(TweakListView, "📋", "All Tweaks", $"{count} tweak{(count == 1 ? "" : "s")} across every category");
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
            await FadeViewTransition(TweakListView, GetCategoryIcon(category), category, $"{count} tweak{(count == 1 ? "" : "s")} in this category");

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
            await FadeViewTransition(HomeView, "🏠", "System Information", "Your current system specifications and status");
            LoadSystemInfo();
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async void OnCreditsClick(object sender, RoutedEventArgs e)
    {
        SetActiveNav(sender as Button);
        await ShowCreditsAsync();
    }

    private async void OnCreditsCardClick(object sender, RoutedEventArgs e)
    {
        // Not a sidebar button, but clicking it should still land on Credits with
        // the Credits sidebar item shown as active, same as navigating there directly.
        SetActiveNav(CreditsNavBtn);
        await ShowCreditsAsync();
    }

    private async Task ShowCreditsAsync()
    {
        if (_isTransitioning) return;

        try
        {
            _isTransitioning = true;
            CreditsVersionText.Text = $"Version {UpdateChecker.CurrentVersion.ToString(3)}";
            await FadeViewTransition(CreditsView, "✨", "Credits", "The people and tools behind RasTweaks");
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

    private async Task FadeViewTransition(FrameworkElement toView, string icon, string title, string description)
    {
        var fromView = _currentView;
        if (toView == null) return;

        // Several distinct destinations (Network, Power, Boot, System, Kernel, GPU,
        // Aim, All Tweaks) all share the same TweakListView container. Skipping the
        // fade animation when the container's already visible is fine, but the
        // header text/icon below must always update regardless - it used to be
        // gated behind this same check, which meant switching between two category
        // tabs changed the cards but left the old tab's name on screen.
        var sameContainerAlreadyVisible = fromView == toView;

        if (!sameContainerAlreadyVisible && fromView != null)
        {
            fromView.Opacity = 1.0;

            for (double i = 1; i >= 0; i -= 0.1)
            {
                fromView.Opacity = i;
                await Task.Delay(15);
            }

            fromView.Visibility = Visibility.Collapsed;
            toView.Visibility = Visibility.Visible;
            toView.Opacity = 0;
        }

        HeaderIcon.Text = icon;
        CategoryTitle.Text = title;
        CategoryDescription.Text = description;
        RefreshInfoButton.Visibility = toView == HomeView ? Visibility.Visible : Visibility.Collapsed;

        if (!sameContainerAlreadyVisible)
        {
            for (double i = 0; i <= 1; i += 0.1)
            {
                toView.Opacity = i;
                await Task.Delay(15);
            }

            toView.Opacity = 1.0;
        }

        _currentView = toView;
    }

    private void SetActiveNav(Button? active)
    {
        foreach (var button in new[] { HomeNavBtn, AllTweaksNavBtn, NetworkNavBtn, PowerNavBtn, BootNavBtn, SystemNavBtn, KernelNavBtn, GpuNavBtn, AimNavBtn, CreditsNavBtn })
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
            Process.Start(new ProcessStartInfo("https://discord.gg/v5Hy39pxe") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Couldn't open the Discord link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---- Live usage monitor (CPU / RAM) ----
    // GPU is intentionally NOT live-monitored: reading GPU Engine counters on this
    // class of hardware costs ~2.4s per sample (measured), which would peg a core
    // and skew the very CPU reading beside it. CPU and RAM are single fast WMI
    // queries, sampled once a second on a background thread so the UI never stutters.

    private void StartUsageMonitor()
    {
        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            var cpu = cpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (cpu?["Name"] is string name && !string.IsNullOrWhiteSpace(name))
            {
                _cpuName = name.Trim();
            }
        }
        catch { /* fall back to "CPU" */ }

        _monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _monitorTimer.Tick += async (s, e) => await OnMonitorTickAsync();
        _monitorTimer.Start();
    }

    private async Task OnMonitorTickAsync()
    {
        // Skip if a previous sample is still running (WMI hiccup) so ticks can't stack.
        if (_monitorSampling) return;
        _monitorSampling = true;
        try
        {
            var (cpu, ram) = await Task.Run(SampleUsage);

            AppendSample(_cpuHistory, cpu);
            AppendSample(_ramHistory, ram);

            // Only bother drawing when the Home view is actually visible.
            if (HomeView.Visibility == Visibility.Visible)
            {
                RedrawMonitor();
            }
        }
        catch { /* a bad sample just skips a frame */ }
        finally
        {
            _monitorSampling = false;
        }
    }

    private static (double cpu, double ram) SampleUsage()
    {
        double cpu = 0, ram = 0;

        try
        {
            using var s = new ManagementObjectSearcher("SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'");
            var o = s.Get().Cast<ManagementObject>().FirstOrDefault();
            if (o?["PercentProcessorTime"] != null)
            {
                cpu = Convert.ToDouble(o["PercentProcessorTime"]);
            }
        }
        catch { }

        try
        {
            using var s = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem");
            var o = s.Get().Cast<ManagementObject>().FirstOrDefault();
            if (o != null)
            {
                double free = Convert.ToDouble(o["FreePhysicalMemory"]);
                double total = Convert.ToDouble(o["TotalVisibleMemorySize"]);
                if (total > 0) ram = (total - free) / total * 100.0;
            }
        }
        catch { }

        return (cpu, ram);
    }

    private static void AppendSample(List<double> history, double value)
    {
        history.Add(Math.Clamp(value, 0, 100));
        while (history.Count > MonitorMaxSamples)
        {
            history.RemoveAt(0);
        }
    }

    private void OnMonitorCpu(object sender, RoutedEventArgs e) => SetActiveMonitor("CPU");
    private void OnMonitorRam(object sender, RoutedEventArgs e) => SetActiveMonitor("RAM");

    private void SetActiveMonitor(string metric)
    {
        _activeMonitor = metric;
        MonCpuBtn.Tag = metric == "CPU" ? "Active" : null;
        MonRamBtn.Tag = metric == "RAM" ? "Active" : null;
        RedrawMonitor();
    }

    private void RedrawMonitor()
    {
        var history = _activeMonitor == "RAM" ? _ramHistory : _cpuHistory;

        double w = MonitorCanvas.ActualWidth;
        double h = MonitorCanvas.ActualHeight;

        var current = history.Count > 0 ? history[^1] : 0;
        MonitorTitle.Text = _activeMonitor == "RAM"
            ? $"Memory Usage  ·  {current:0}%"
            : $"{_cpuName}  ·  {current:0}%";

        // Layout may not be ready on the first ticks - just skip drawing until it is.
        if (w <= 0 || h <= 0 || history.Count == 0)
        {
            return;
        }

        var linePoints = new PointCollection();
        int n = history.Count;
        for (int i = 0; i < n; i++)
        {
            // Newest sample pinned to the right edge; older ones scroll left across
            // a fixed 60-sample window so the graph slides in from the right.
            int fromNewest = (n - 1) - i;
            double x = w - (double)fromNewest / (MonitorMaxSamples - 1) * w;
            double y = h - history[i] / 100.0 * h;
            linePoints.Add(new Point(x, y));
        }

        MonitorLine.Points = linePoints;

        var fillPoints = new PointCollection(linePoints)
        {
            new Point(linePoints[^1].X, h),
            new Point(linePoints[0].X, h)
        };
        MonitorFill.Points = fillPoints;
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