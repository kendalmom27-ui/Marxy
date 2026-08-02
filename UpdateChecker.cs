using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace RasTweaksCS
{
    public class UpdateInfo
    {
        public required Version Version { get; set; }
        public required string DownloadUrl { get; set; }
        public long Size { get; set; }
    }

    public static class UpdateChecker
    {
        private const string RepoOwner = "kendalmom27-ui";
        private const string RepoName = "Marxy";

        public static Version CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

        private static string AttemptMarkerPath => IOPath.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RasTweaksCS", "last_update_attempt.txt");

        /// <summary>
        /// Circuit breaker: if we already tried updating to this exact version within
        /// the last few minutes and are still not running it, something is wrong with
        /// the update itself (not just "haven't tried yet") - retrying immediately
        /// would just loop forever, so back off instead and let the current version
        /// keep running normally until the cooldown passes.
        /// </summary>
        private static bool RecentlyFailedToReach(Version targetVersion)
        {
            try
            {
                if (!File.Exists(AttemptMarkerPath))
                {
                    return false;
                }

                var parts = File.ReadAllText(AttemptMarkerPath).Split('|');
                if (parts.Length != 2)
                {
                    return false;
                }

                if (!Version.TryParse(parts[0], out var attemptedVersion) || attemptedVersion != targetVersion)
                {
                    return false;
                }

                if (!long.TryParse(parts[1], out var ticks))
                {
                    return false;
                }

                var attemptedAt = new DateTime(ticks, DateTimeKind.Utc);
                return DateTime.UtcNow - attemptedAt < TimeSpan.FromMinutes(5);
            }
            catch
            {
                return false;
            }
        }

        public static void RecordUpdateAttempt(Version targetVersion)
        {
            try
            {
                Directory.CreateDirectory(IOPath.GetDirectoryName(AttemptMarkerPath)!);
                File.WriteAllText(AttemptMarkerPath, $"{targetVersion}|{DateTime.UtcNow.Ticks}");
            }
            catch
            {
                // Best-effort - worst case the circuit breaker just doesn't trip this time.
            }
        }

        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RasTweaksCS-UpdateChecker", "1.0"));

            var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var json = await http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var versionString = tagName.TrimStart('v', 'V');
            if (!Version.TryParse(versionString, out var remoteVersion))
            {
                return null;
            }

            if (remoteVersion <= CurrentVersion)
            {
                return null;
            }

            if (RecentlyFailedToReach(remoteVersion))
            {
                return null;
            }

            if (!root.TryGetProperty("assets", out var assets))
            {
                return null;
            }

            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                if (name == null || !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = asset.TryGetProperty("browser_download_url", out var urlProp) ? urlProp.GetString() : null;
                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                var size = asset.TryGetProperty("size", out var sizeProp) ? sizeProp.GetInt64() : 0;

                return new UpdateInfo
                {
                    Version = remoteVersion,
                    DownloadUrl = downloadUrl,
                    Size = size
                };
            }

            return null;
        }

        public static async Task DownloadUpdateAsync(string url, string destPath, IProgress<double> progress)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RasTweaksCS-UpdateChecker", "1.0"));

            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(destPath);

            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    progress.Report((double)totalRead / totalBytes * 100.0);
                }
            }
        }

        /// <summary>
        /// A running exe can't overwrite itself on Windows, so this writes a small
        /// helper batch script that waits for this process to actually exit (by PID,
        /// not a fixed delay), swaps the downloaded exe into place, relaunches it,
        /// then deletes itself. Caller must shut down immediately after calling this
        /// so the file lock on the current exe is released.
        /// </summary>
        public static void ApplyUpdateAndRestart(string newExePath)
        {
            var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExePath))
            {
                return;
            }

            var pid = Environment.ProcessId;
            var scriptPath = IOPath.Combine(IOPath.GetTempPath(), "RasTweaksCS_apply_update.bat");

            // "move" can silently fail if the exe is still momentarily locked (WPF
            // shutdown cleanup, or Windows Defender scanning the freshly-downloaded
            // file are both common causes) - a swallowed failure here used to mean
            // this just relaunched the OLD exe, which would see the same GitHub
            // release as "newer" again and loop forever. "if exist newExePath" after
            // the move reliably tells us whether it actually succeeded (move
            // consumes the source file on success, leaves it in place on failure),
            // so this retries instead of trusting the move's exit code.
            var script = "@echo off\r\n" +
                         ":waitloop\r\n" +
                         $"tasklist /FI \"PID eq {pid}\" | find \"{pid}\" >nul\r\n" +
                         "if not errorlevel 1 (\r\n" +
                         "    timeout /t 1 /nobreak >nul\r\n" +
                         "    goto waitloop\r\n" +
                         ")\r\n" +
                         "set RETRIES=0\r\n" +
                         ":moveloop\r\n" +
                         $"move /y \"{newExePath}\" \"{currentExePath}\" >nul 2>&1\r\n" +
                         $"if exist \"{newExePath}\" (\r\n" +
                         "    set /a RETRIES+=1\r\n" +
                         "    if %RETRIES% GEQ 15 goto :done\r\n" +
                         "    timeout /t 1 /nobreak >nul\r\n" +
                         "    goto moveloop\r\n" +
                         ")\r\n" +
                         ":done\r\n" +
                         $"start \"\" \"{currentExePath}\"\r\n" +
                         "(goto) 2>nul & del \"%~f0\"\r\n";

            File.WriteAllText(scriptPath, script);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}
