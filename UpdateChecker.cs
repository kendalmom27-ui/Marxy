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
        // Points at the public releases-only repo, NOT the private source repo.
        // The app ships with no auth token (anything embedded in a distributed exe
        // is extractable), so the releases it checks have to live somewhere
        // publicly readable - hence the split: source private, builds published
        // across to this public repo by CI.
        private const string RepoOwner = "kendalmom27-ui";
        private const string RepoName = "Marxy-releases";

        public static Version CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

        private static string AttemptMarkerPath => IOPath.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RasTweaksCS", "last_update_attempt.txt");

        // A genuine interruption (user closes the app mid-download) should retry on the
        // very next launch. A real broken-update LOOP (swap keeps failing, app relaunches
        // itself over and over in seconds) should still be caught so it can't spin forever.
        // The two are told apart by TIMING + COUNT: a loop re-attempts within seconds and
        // racks up a count fast; a person reopening the app does so minutes later, which
        // resets the count. So we only back off after several *rapid* failures, never after
        // a single interrupted attempt.
        private const int MaxRapidAttempts = 3;
        private static readonly TimeSpan RapidWindow = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(5);

        private static (Version? version, DateTime time, int count) ReadMarker()
        {
            try
            {
                if (!File.Exists(AttemptMarkerPath))
                {
                    return (null, DateTime.MinValue, 0);
                }

                var parts = File.ReadAllText(AttemptMarkerPath).Split('|');
                if (parts.Length < 3
                    || !Version.TryParse(parts[0], out var v)
                    || !long.TryParse(parts[1], out var ticks)
                    || !int.TryParse(parts[2], out var count))
                {
                    return (null, DateTime.MinValue, 0);
                }

                return (v, new DateTime(ticks, DateTimeKind.Utc), count);
            }
            catch
            {
                return (null, DateTime.MinValue, 0);
            }
        }

        /// <summary>
        /// Only blocks when we've failed to reach this version several times in RAPID
        /// succession (a genuine update loop) AND are still within the cooldown. A single
        /// interrupted update - or attempts spaced minutes apart - never blocks.
        /// </summary>
        private static bool ShouldBackOff(Version targetVersion)
        {
            var (version, time, count) = ReadMarker();
            if (version == null || version != targetVersion)
            {
                return false;
            }

            return count >= MaxRapidAttempts && DateTime.UtcNow - time < Cooldown;
        }

        public static void RecordUpdateAttempt(Version targetVersion)
        {
            try
            {
                var (version, time, count) = ReadMarker();

                // Count up only for repeated attempts on the SAME version in quick
                // succession (the loop signature). Anything else - new version, or a gap
                // longer than the rapid window - starts fresh at 1, so a person reopening
                // the app after an interrupted update always gets a clean retry.
                var newCount = (version == targetVersion && DateTime.UtcNow - time < RapidWindow)
                    ? count + 1
                    : 1;

                Directory.CreateDirectory(IOPath.GetDirectoryName(AttemptMarkerPath)!);
                File.WriteAllText(AttemptMarkerPath, $"{targetVersion}|{DateTime.UtcNow.Ticks}|{newCount}");
            }
            catch
            {
                // Best-effort - worst case the loop guard just doesn't trip this time.
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

            if (ShouldBackOff(remoteVersion))
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
