using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace RasTweaksCS;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Most tweaks touch HKLM/services and need admin rights. The exe's
        // embedded manifest normally forces a UAC prompt on launch, but that
        // can be bypassed depending on how the process is started (e.g. via
        // `dotnet run`, a debugger, or a RunAsInvoker compatibility override),
        // silently leaving the app running unelevated with no visible warning
        // beyond each tweak failing with "Script exited with code 1". This is
        // a belt-and-suspenders runtime check that doesn't depend on the
        // manifest being honored.
        if (!IsRunningAsAdministrator())
        {
            if (TryRelaunchElevated())
            {
                Shutdown();
                return;
            }

            // User declined the UAC prompt (or relaunch failed for some other
            // reason) - fall through and run unelevated. Tweaks that need
            // admin rights will fail with their existing error message,
            // same as before this check existed.
        }

        _ = RunStartupFlowAsync();
    }

    /// <summary>
    /// No StartupUri anymore (see App.xaml) - this app manually controls window
    /// creation so an update-check screen can run and, if needed, fully replace
    /// itself before MainWindow (with all its WMI/tweak-loading work) ever spins up.
    /// </summary>
    private async Task RunStartupFlowAsync()
    {
        var updateWindow = new UpdateCheckWindow();
        updateWindow.Show();

        bool updateApplied;
        try
        {
            updateApplied = await updateWindow.RunUpdateCheckAsync();
        }
        catch
        {
            updateApplied = false;
        }

        if (updateApplied)
        {
            // ApplyUpdateAndRestart already launched the helper script that swaps
            // the exe and relaunches it - this process just needs to release its
            // file lock by exiting.
            Shutdown();
            return;
        }

        // Show MainWindow before closing the update window, not after - closing
        // the last open window triggers Application shutdown by default, and we
        // don't want that race if this were the only window left even briefly.
        var main = new MainWindow();
        MainWindow = main;
        main.Show();

        updateWindow.Close();
    }

    private static bool IsRunningAsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static bool TryRelaunchElevated()
    {
        try
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                return false;
            }

            Process.Start(new ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                Verb = "runas"
            });
            return true;
        }
        catch (Win32Exception)
        {
            // Thrown when the user clicks "No" on the UAC prompt.
            return false;
        }
    }
}

